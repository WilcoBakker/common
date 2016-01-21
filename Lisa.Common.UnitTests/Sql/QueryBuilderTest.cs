﻿using Lisa.Common.Sql;
using System;
using Xunit;

namespace Lisa.Common.UnitTests.Sql
{
    public class QueryBuilderTest
    {
        [Fact]
        public void ItReturnsQueriesWithoutParametersUnchanged()
        {
            string query = "SELECT * FROM Planets";

            string result = QueryBuilder.Build(query);
            Assert.Equal("SELECT * FROM Planets", result);
        }

        [Fact]
        public void ItReplacesValueParameters()
        {
            string query = "SELECT * FROM Planets WHERE MoonCount=@MoonCount AND Star=@Star";
            object parameters = new
            {
                MoonCount = 2,
                Star = "Sol"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM Planets WHERE MoonCount='2' AND Star='Sol'", result);
        }

        [Fact]
        public void ItDoesNotAddUnnecessaryQuotes()
        {
            string query = "SELECT * FROM Planets WHERE MoonCount='@MoonCount' AND Star='@Star'";
            object parameters = new
            {
                MoonCount = 2,
                Star = "Sol"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM Planets WHERE MoonCount='2' AND Star='Sol'", result);
        }

        [Fact]
        public void ItSanitizesQuotesWithinValueParameters()
        {
            string query = "SELECT * FROM Planets WHERE Name='@Name'";
            object parameters = new
            {
                Name = "Q'onos"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM Planets WHERE Name='Q''onos'", result);
        }

        [Fact]
        public void ItReplacesNameParameters()
        {
            string query = "SELECT * FROM Planets WHERE $Column='Vulcan'";
            object parameters = new
            {
                Column = "Inhabitant"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM Planets WHERE [Inhabitant]='Vulcan'", result);
        }

        [Fact]
        public void ItRejectsNameParametersWithSquareBrackets()
        {
            string query = "SELECT * FROM Planets WHERE $Column='Vulcan'";
            object parameters = new
            {
                Column = "[Inhabitant]"
            };

            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(query, parameters));
        }

        [Fact]
        public void ItRejectsQueryIfParameterIsMissing()
        {
            string query = "SELECT * FROM Planets WHERE MoonCount=@MoonCount";
            object parameters = new
            {
                Name = "Vulcan"
            };

            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(query, parameters));
        }

        [Fact]
        public void ItConvertsAListOfValues()
        {
            string query = "SELECT * FROM Planets WHERE Name IN (@Names)";
            object parameters = new
            {
                Names = new string[] { "Romulus", "Q'onos", "Vulcan" }
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM Planets WHERE Name IN ('Romulus', 'Q''onos', 'Vulcan')", result);
        }

        [Fact]
        public void ItIgnoresAtSignsForTheObjectMapper()
        {
            string query = @"SELECT Id AS [@], #Moons_@Id, Moon.Name AS #Moons_Name FROM Planets LEFT JOIN Moons ON Planets.Id = Moon.Planet WHERE Planets.Name = @Name";
            object parameters = new
            {
                Name = "Vulcan"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT Id AS [@], #Moons_@Id, Moon.Name AS #Moons_Name FROM Planets LEFT JOIN Moons ON Planets.Id = Moon.Planet WHERE Planets.Name = 'Vulcan'", result);
        }

        [Fact]
        public void ItIgnoresSpecialSqlValues()
        {
            string query = "SELECT @@identity";
            string result = QueryBuilder.Build(query);
            Assert.Equal("SELECT @@identity", result);
        }

        [Fact]
        public void ItCanHandleNameAndValueParametersInTheSameQuery()
        {
            string query = "SELECT * FROM $Table WHERE Name='@Name'";
            object parameters = new
            {
                Table = "Planets",
                Name = "Q'onos"
            };

            string result = QueryBuilder.Build(query, parameters);
            Assert.Equal("SELECT * FROM [Planets] WHERE Name='Q''onos'", result);
        }
    }
}