using Ensure.Generator.Generation;
using Ensure.Generator.Models;

namespace Ensure.Generator.Tests;

public class CSharpGeneratorTests
{
    private readonly CSharpGenerator _generator = new();

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
        Assert.Contains("namespace TestNamespace", result);
        Assert.Contains("public abstract class LoginFeatureBase", result);
        Assert.Contains("public abstract Task NavigateToPage(string param1);", result);
        Assert.Contains("/// Navigate to \\\"login\\\" page", result);
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
        Assert.Contains("namespace TestNamespace", result);
        Assert.Contains("public abstract class LoginFeatureTestsBase", result);
        Assert.Contains("[Fact]", result);
        Assert.Contains("public async Task SuccessfulLogin()", result);
        Assert.Contains("await Steps.NavigateToPage(\"login\");", result);
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
        Assert.Contains("List<Dictionary<string, string>> tableData", result);
        Assert.Contains("public abstract Task ValidateUsersData(List<Dictionary<string, string>> tableData);", result);
    }
} 