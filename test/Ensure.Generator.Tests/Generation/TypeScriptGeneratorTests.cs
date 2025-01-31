using Ensure.Generator.Generation;
using Ensure.Generator.Models;

namespace Ensure.Generator.Tests;

public class TypeScriptGeneratorTests
{
    private readonly TypeScriptGenerator _generator = new();

    [Fact]
    public void GenerateSteps_WithSimpleSpec_GeneratesCorrectOutput()
    {
        // Arrange
        var step = new Step(
            "Navigate to \"login\" page",
            new Dictionary<string, string> { { "param1", "login" } }
        );
        
        var scenario = new Scenario(
            "Successful Login",
            new List<Step> { step }
        );
        
        var spec = new Spec(
            "Login Feature",
            new List<Step>(),
            new List<Scenario> { scenario }
        );

        // Act
        var result = _generator.GenerateSteps(spec, "TestNamespace", "LoginFeature");

        // Assert
        Assert.Contains("export abstract class LoginFeatureBase", result);
        Assert.Contains("abstract navigateToPage(param1: string): Promise<void>;", result);
        Assert.Contains("* Navigate to \\\"login\\\" page", result);
    }

    [Fact]
    public void GenerateTests_WithSimpleSpec_GeneratesCorrectOutput()
    {
        // Arrange
        var step = new Step(
            "Navigate to \"login\" page",
            new Dictionary<string, string> { { "param1", "login" } }
        );
        
        var scenario = new Scenario(
            "Successful Login",
            new List<Step> { step }
        );
        
        var spec = new Spec(
            "Login Feature",
            new List<Step>(),
            new List<Scenario> { scenario }
        );

        // Act
        var result = _generator.GenerateTests(spec, "TestNamespace", "LoginFeature");

        // Assert
        Assert.Contains("import { test, expect } from '@playwright/test';", result);
        Assert.Contains("export abstract class LoginFeatureTestsBase", result);
        Assert.Contains("protected abstract getSteps(page: Page): LoginFeatureBase;", result);
        Assert.Contains("test('Successful Login', async ({ page }) => {", result);
        Assert.Contains("await steps.navigateToPage('login');", result);
    }

    [Fact]
    public void GenerateSteps_WithTableData_GeneratesCorrectOutput()
    {
        // Arrange
        var step = new Step(
            "Validate users data",
            new Dictionary<string, string> { { "tableData", "[{\"Name\":\"John\",\"Age\":\"25\"}]" } }
        );
        
        var scenario = new Scenario(
            "Validate Users",
            new List<Step> { step }
        );
        
        var spec = new Spec(
            "User Data",
            new List<Step>(),
            new List<Scenario> { scenario }
        );

        // Act
        var result = _generator.GenerateSteps(spec, "TestNamespace", "UserData");

        // Assert
        Assert.Contains("abstract validateUsersData(tableData: Array<Record<string, string>>): Promise<void>;", result);
    }
} 