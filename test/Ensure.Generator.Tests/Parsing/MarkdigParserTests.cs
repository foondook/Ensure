using Xunit;
using Ensure.Generator.Parsing;
using System.Text.Json;

namespace Ensure.Generator.Tests.Parsing;

public class MarkdigParserTests
{
    private readonly MarkdigParser _parser;

    public MarkdigParserTests()
    {
        _parser = new MarkdigParser();
    }

    [Fact]
    public void Parse_BasicSpec_ReturnsCorrectStructure()
    {
        var markdown = @"# Login Feature

## Successful Login
* Given I am on the login page
* When I enter ""admin"" as username
* And I enter ""password123"" as password
* Then I should be logged in";

        var spec = _parser.Parse(markdown);

        Assert.Equal("Login Feature", spec.Name);
        Assert.Single(spec.Scenarios);
        Assert.Equal("Successful Login", spec.Scenarios[0].Name);
        Assert.Equal(4, spec.Scenarios[0].Steps.Count);
        
        // Verify parameter extraction
        Assert.Equal("admin", spec.Scenarios[0].Steps[1].Parameters["param1"]);
        Assert.Equal("password123", spec.Scenarios[0].Steps[2].Parameters["param1"]);
    }

    [Fact]
    public void Parse_WithBackground_ProcessesBackgroundSteps()
    {
        var markdown = @"# Feature with Background

* Given all users are logged out
* And the database is clean

## First Scenario
* When I do something";

        var spec = _parser.Parse(markdown);

        Assert.Equal(2, spec.Background.Count);
        Assert.Equal("Given all users are logged out", spec.Background[0].Text);
        Assert.Equal("And the database is clean", spec.Background[1].Text);
        Assert.Single(spec.Scenarios);
    }

    [Fact]
    public void Parse_WithTable_ExtractsTableData()
    {
        var markdown = @"# Table Feature

## Scenario with Table
* Given I have the following users

|Username | Password  |
|---------|-----------|
|admin    | pass123  |
|user     | pass456  |";

        var spec = _parser.Parse(markdown);
        var tableStep = spec.Scenarios[0].Steps[0];
        
        Assert.Equal("Given I have the following users", tableStep.Text);
        Assert.Contains("tableData", tableStep.Parameters.Keys);
        var tableData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(
            tableStep.Parameters["tableData"]);
        
        Assert.Equal(2, tableData.Count);
        Assert.Equal("admin", tableData[0]["Username"]);
        Assert.Equal("pass123", tableData[0]["Password"]);
        Assert.Equal("user", tableData[1]["Username"]);
        Assert.Equal("pass456", tableData[1]["Password"]);
    }

    [Fact]
    public void Parse_WithFormattedText_HandlesBasicFormatting()
    {
        var markdown = @"# Formatting Feature

## Text Formatting
* Given I have **bold** and *italic* text
* When I have text with ""quoted parameters""
* Then the text should be processed correctly";

        var spec = _parser.Parse(markdown);
        var steps = spec.Scenarios[0].Steps;

        Assert.Equal("Given I have bold and italic text", steps[0].Text);
        Assert.Equal("When I have text with \"quoted parameters\"", steps[1].Text);
        Assert.Contains("param1", steps[1].Parameters);
        Assert.Equal("quoted parameters", steps[1].Parameters["param1"]);
    }
} 