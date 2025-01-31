using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Models;

namespace Ensure.Generator.Generation;

public class TypeScriptGenerator : ICodeGenerator
{
    public string GenerateSteps(Spec spec, string namespaceName, string className)
    {
        var builder = new StringBuilder();
        var baseClassName = $"{className}Base";
        
        builder.AppendLine("// Generated code - do not modify\n");
        builder.AppendLine($"export abstract class {baseClassName} {{");

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
                .Select((kvp, i) => $"param{i + 1}: string"));
            
            // Add table parameter if present
            if (parameters.ContainsKey("tableData"))
            {
                paramList.Add("tableData: Array<Record<string, string>>");
            }
            
            var parameterList = string.Join(", ", paramList);
            var escapedStepText = step.Text.Replace("\"", "\\\"");
            
            builder.AppendLine($@"    /**
     * {escapedStepText}
     */");
            builder.AppendLine($"    abstract {methodName}({parameterList}): Promise<void>;\n");
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    public string GenerateTests(Spec spec, string namespaceName, string className)
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("// Generated code - do not modify");
        builder.AppendLine("import { test, expect } from '@playwright/test';");
        builder.AppendLine($"import {{ {className}Base }} from './{className}.steps';\n");

        builder.AppendLine($"export abstract class {className}TestsBase {{");
        builder.AppendLine($"    protected abstract getSteps(page: Page): {className}Base;\n");

        foreach (var scenario in spec.Scenarios)
        {
            GenerateScenarioTest(builder, scenario, spec.Background);
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private void GenerateScenarioTest(StringBuilder builder, Scenario scenario, List<Step> backgroundSteps)
    {
        var testName = ToValidMethodName(scenario.Name);

        builder.AppendLine($"    test('{scenario.Name}', async ({{ page }}) => {{");
        builder.AppendLine("        const steps = this.getSteps(page);\n");

        void WriteStepCall(Step step)
        {
            var methodName = ToValidMethodName(step.Text);
            var parameters = new List<string>();
            
            // Add string parameters from the step text
            parameters.AddRange(step.Parameters
                .Where(p => !p.Key.Equals("tableData", StringComparison.OrdinalIgnoreCase))
                .Select(kvp => $"'{kvp.Value}'"));
            
            // Add table parameter if present
            if (step.Parameters.TryGetValue("tableData", out var tableJson))
            {
                parameters.Add(tableJson);
            }
            
            var parameterList = string.Join(", ", parameters);
            builder.AppendLine($"        await steps.{methodName}({parameterList});");
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

        builder.AppendLine("    });\n");
    }

    private string ToValidMethodName(string stepText)
    {
        // Remove parameters in quotes and colons
        var withoutParams = System.Text.RegularExpressions.Regex.Replace(stepText, @"""[^""]*""", "")
            .TrimEnd(':')  // Remove trailing colons
            .Trim();
        
        // Split on any non-word character and convert to camelCase
        var words = System.Text.RegularExpressions.Regex.Split(withoutParams, @"[^\w]")
            .Where(w => !string.IsNullOrWhiteSpace(w));
            
        var firstWord = words.First().ToLower();
        var remainingWords = words.Skip(1)
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
            
        return firstWord + string.Join("", remainingWords);
    }
} 