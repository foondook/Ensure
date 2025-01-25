﻿namespace Ensure.Generator;

using CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

public class Program
{
    public class Options
    {
        [Option('s', "specs", Required = true, HelpText = "Path to specs directory")]
        public string SpecsPath { get; set; } = "";

        [Option('o', "output", Required = true, HelpText = "Output directory for generated tests")]
        public string OutputPath { get; set; } = "";

        [Option('n', "namespace", Required = true, HelpText = "Namespace for generated tests")]
        public string Namespace { get; set; } = "";
    }

    public static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(RunAsync);
    }

    private static async Task RunAsync(Options opts)
    {
        var searchPatterns = new[] { "*.spec", "*.spec.md", "*.md" };
        var specFiles = new List<string>();

        foreach (var pattern in searchPatterns)
        {
            specFiles = Directory.GetFiles(opts.SpecsPath, pattern, SearchOption.AllDirectories).ToList();
            if (specFiles.Any())
            {
                Console.WriteLine($"Found {specFiles.Count} spec files using pattern '{pattern}'");
                break;
            }
            Console.WriteLine($"No spec files found using pattern '{pattern}', trying next pattern...");
        }

        if (!specFiles.Any())
        {
            Console.WriteLine("No spec files found in the specified directory.");
            return;
        }
        
        Directory.CreateDirectory(opts.OutputPath);
        
        foreach (var specFile in specFiles)
        {
            Console.WriteLine($"Processing spec file: {specFile}");
            var spec = await ParseSpecFile(specFile);
            var (stepsCode, testCode) = GenerateClasses(spec, opts.Namespace);
            
            var baseFileName = ToValidIdentifier(spec.Name);
            
            // Generate Steps interface class
            var stepsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.Steps.g.cs");
            await File.WriteAllTextAsync(stepsPath, stepsCode);
            Console.WriteLine($"Generated steps file: {stepsPath}");
            
            // Generate Tests class
            var testsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.Tests.g.cs");
            await File.WriteAllTextAsync(testsPath, testCode);
            Console.WriteLine($"Generated tests file: {testsPath}");
        }
    }

    private static (string StepsCode, string TestCode) GenerateClasses(Spec spec, string namespaceName)
    {
        var className = ToValidIdentifier(spec.Name);
        var stepsClassName = $"{className}Steps";

        return (
            GenerateStepsClass(spec, namespaceName, stepsClassName),
            GenerateTestClass(spec, namespaceName, className, stepsClassName)
        );
    }

    private static string GenerateStepsClass(Spec spec, string namespaceName, string stepsClassName)
    {
        var builder = new StringBuilder();
        var baseClassName = $"{stepsClassName}Base";

        builder.AppendLine($@"// <auto-generated />
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var parameters = ExtractParameters(step.Text);
            var paramList = new List<string>();
            
            // Add string parameters from the step text
            paramList.AddRange(parameters.Select((kvp, i) => $"string param{i + 1}"));
            
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

    private static string GenerateTestClass(Spec spec, string namespaceName, string className, string stepsClassName)
    {
        var builder = new StringBuilder();
        var baseClassName = $"{className}TestsBase";
        var stepsBaseClassName = $"{stepsClassName}Base";

        builder.AppendLine($@"// <auto-generated />
using System.Threading.Tasks;
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
            parameters.AddRange(step.Parameters.Values.Select(v => $"\"{v}\""));
            
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

    private static Dictionary<string, string> ExtractParameters(string stepText)
    {
        var parameters = new Dictionary<string, string>();
        var matches = Regex.Matches(stepText, "\"([^\"]*)\"");
        
        for (int i = 0; i < matches.Count; i++)
        {
            parameters[$"param{i + 1}"] = matches[i].Groups[1].Value;
        }
        
        return parameters;
    }

    private record Spec(string Name, List<Step> Background, List<Scenario> Scenarios);
    private record Scenario(string Name, List<Step> Steps);
    private record Step(string Text, Dictionary<string, string> Parameters);

    private static async Task<Spec> ParseSpecFile(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        var lines = content.Split('\n');
        
        var name = "";
        var scenarios = new List<Scenario>();
        var backgroundSteps = new List<Step>();
        Scenario? currentScenario = null;
        var steps = new List<Step>();
        var parsingBackground = true;

        foreach (var line in lines.Select(l => l.Trim()))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("#") && !line.StartsWith("##"))
            {
                name = line.TrimStart('#', ' ');
            }
            else if (line.StartsWith("##"))
            {
                parsingBackground = false;
                if (currentScenario != null)
                {
                    scenarios.Add(currentScenario with { Steps = steps.ToList() });
                }
                
                var scenarioName = line.TrimStart('#', ' ');
                currentScenario = new Scenario(scenarioName, new List<Step>());
                steps = new List<Step>();
            }
            else if (line.StartsWith("*"))
            {
                var stepText = line.TrimStart('*', ' ');
                var parameters = ExtractParameters(stepText);
                var step = new Step(stepText, parameters);
                
                if (parsingBackground)
                {
                    backgroundSteps.Add(step);
                }
                else if (currentScenario != null)
                {
                    steps.Add(step);
                }
            }
        }

        if (currentScenario != null)
        {
            scenarios.Add(currentScenario with { Steps = steps });
        }

        return new Spec(name, backgroundSteps, scenarios);
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