namespace Ensure.Generator.Generation;

using System.Text;
using System.Text.RegularExpressions;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Models;
using System.Text.Json;

public class CSharpGenerator : ICodeGenerator
{
    public string GenerateSteps(Spec spec, string namespaceName, string className)
    {
        var builder = new StringBuilder();
        var baseClassName = $"{className}Base";

        builder.AppendLine($@"// <auto-generated />
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace {namespaceName}
{{
    public abstract class {baseClassName}
    {{
");

        // Generate step methods based on unique method names
        var allSteps = spec.Background.Concat(spec.Scenarios.SelectMany(s => s.Steps));
        var uniqueSteps = allSteps
            .GroupBy(s => ToValidMethodName(s.Text))
            .Select(g => g.First())
            .ToList();

        foreach (var step in uniqueSteps)
        {
            var methodName = ToValidMethodName(step.Text);
            var parameters = step.Parameters;
            var paramList = new List<string>();
            
            // Add string parameters from the step text
            paramList.AddRange(parameters
                .Where(p => !p.Key.Equals("tableData", StringComparison.OrdinalIgnoreCase))
                .Select((kvp, i) => $"string param{i + 1}"));
            
            // Add table parameter if present
            if (parameters.ContainsKey("tableData"))
            {
                paramList.Add("List<Dictionary<string, string>> tableData");
            }
            
            var parameterList = string.Join(", ", paramList);
            var escapedStepText = step.Text.Replace("\"", "\\\"");
            
            builder.AppendLine($@"        /// <summary>
        /// {escapedStepText}
        /// </summary>");
            builder.AppendLine($"        public abstract Task {methodName}({parameterList});");
            builder.AppendLine();
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    public string GenerateTests(Spec spec, string namespaceName, string className)
    {
        var builder = new StringBuilder();
        var baseClassName = $"{className}TestsBase";
        var stepsBaseClassName = $"{className}StepsBase";

        builder.AppendLine($@"// <auto-generated />
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace {namespaceName}
{{
    public abstract class {baseClassName}
    {{
        protected abstract {stepsBaseClassName} Steps {{ get; }}
");

        foreach (var scenario in spec.Scenarios)
        {
            GenerateScenarioTest(builder, scenario, spec.Background);
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static void GenerateScenarioTest(StringBuilder builder, Scenario scenario, List<Step> backgroundSteps)
    {
        var testName = ToValidIdentifier(scenario.Name);

        builder.AppendLine($@"
        [Fact]
        public async Task {testName}()
        {{");

        void WriteStepCall(Step step)
        {
            var methodName = ToValidMethodName(step.Text);
            var parameters = new List<string>();
            
            // Add string parameters from the step text
            parameters.AddRange(step.Parameters
                .Where(p => !p.Key.Equals("tableData", StringComparison.OrdinalIgnoreCase))
                .Select(kvp => $"\"{kvp.Value}\""));
            
            // Add table parameter if present
            if (step.Parameters.TryGetValue("tableData", out var tableJson))
            {
                parameters.Add($"JsonSerializer.Deserialize<List<Dictionary<string, string>>>(\"{tableJson.Replace("\"", "\\\"")}\")");
            }
            
            var parameterList = string.Join(", ", parameters);
            builder.AppendLine($"            await Steps.{methodName}({parameterList});");
        }

        // First run background steps
        foreach (var step in backgroundSteps)
        {
            WriteStepCall(step);
        }

        // Then run scenario steps
        foreach (var step in scenario.Steps)
        {
            WriteStepCall(step);
        }

        builder.AppendLine("        }");
    }

    private static string ToValidMethodName(string stepText)
    {
        // Remove parameters in quotes and colons
        var withoutParams = Regex.Replace(stepText, @"""[^""]*""", "")
            .TrimEnd(':')  // Remove trailing colons
            .Trim();
        
        // Convert to PascalCase and remove invalid characters
        var words = withoutParams.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }

    private static string ToValidIdentifier(string input)
    {
        // Split on any non-word character and convert to PascalCase
        var words = Regex.Split(input, @"[^\w]")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
            
        return string.Join("", words);
    }
} 