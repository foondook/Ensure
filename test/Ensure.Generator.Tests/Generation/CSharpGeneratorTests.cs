using Ensure.Generator.Generation;
using Ensure.Generator.Models;
using VerifyXunit;
using VerifyTests;

namespace Ensure.Generator.Tests;

public class CSharpGeneratorTests
{
    private readonly CSharpGenerator _generator = new();

    [Fact]
    public Task GenerateSteps_WithSimpleSpec_GeneratesCorrectOutput()
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
        var result = _generator.GenerateSteps(spec, "TestNamespace", "LoginFeatureSteps");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task GenerateTests_WithSimpleSpec_GeneratesCorrectOutput()
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
        var result = _generator.GenerateTests(spec, "TestNamespace", "LoginFeatureTests");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task GenerateSteps_WithTableData_GeneratesCorrectOutput()
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
        var result = _generator.GenerateSteps(spec, "TestNamespace", "UserDataSteps");

        // Assert
        return Verify(result);
    }
} 