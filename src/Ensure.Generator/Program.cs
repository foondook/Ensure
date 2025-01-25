namespace Ensure.Generator;

using CommandLine;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Generation;
using Ensure.Generator.Parsing;
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
        
        var parser = new SimpleMarkdownParser();
        var generator = new CSharpGenerator();
        
        foreach (var specFile in specFiles)
        {
            Console.WriteLine($"Processing spec file: {specFile}");
            var content = await File.ReadAllTextAsync(specFile);
            var spec = parser.Parse(content);
            
            var baseFileName = ToValidIdentifier(spec.Name);
            var stepsClassName = $"{baseFileName}Steps";
            
            // Generate Steps interface class
            var stepsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.Steps.g.cs");
            var stepsCode = generator.GenerateSteps(spec, opts.Namespace, stepsClassName);
            await File.WriteAllTextAsync(stepsPath, stepsCode);
            Console.WriteLine($"Generated steps file: {stepsPath}");
            
            // Generate Tests class
            var testsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.Tests.g.cs");
            var testCode = generator.GenerateTests(spec, opts.Namespace, baseFileName);
            await File.WriteAllTextAsync(testsPath, testCode);
            Console.WriteLine($"Generated tests file: {testsPath}");
        }
    }

    private static string ToValidIdentifier(string input)
    {
        // Split on any non-word character and convert to PascalCase
        var words = System.Text.RegularExpressions.Regex.Split(input, @"[^\w]")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
            
        return string.Join("", words);
    }
}