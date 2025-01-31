namespace Ensure.Generator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Generation;
using Ensure.Generator.Parsing;

public class Program
{
    public class Options
    {
        [Option('s', "specs", Required = false, HelpText = "Path to specs directory")]
        public string SpecsPath { get; set; } = "";

        [Option('o', "output", Required = false, HelpText = "Output directory for generated tests")]
        public string OutputPath { get; set; } = "";

        [Option('n', "namespace", Required = false, HelpText = "Namespace for generated C# tests")]
        public string Namespace { get; set; } = "";

        [Value(0, Required = false, HelpText = "Target language (csharp or typescript)")]
        public string Language { get; set; } = "csharp";
    }

    public static async Task Main(string[] args)
    {
        var parser = new Parser(config =>
        {
            config.EnableDashDash = true;
            config.HelpWriter = Console.Out;
        });

        await parser.ParseArguments<Options>(args)
            .WithParsedAsync(RunAsync);
    }

    private static async Task RunAsync(Options opts)
    {
        // Normalize language input
        opts.Language = opts.Language?.ToLower() switch
        {
            "ts" or "typescript" => "typescript",
            "cs" or "csharp" or "" => "csharp",
            var lang => lang
        };

        // Set default paths based on language
        if (string.IsNullOrWhiteSpace(opts.SpecsPath))
        {
            opts.SpecsPath = opts.Language switch
            {
                "typescript" => "specs",
                _ => "Specs"
            };
        }

        // Set default output path if not provided
        if (string.IsNullOrWhiteSpace(opts.OutputPath))
        {
            opts.OutputPath = opts.Language switch
            {
                "typescript" => "tests",
                _ => "Generated"
            };
        }

        // Set default namespace for C# if not provided
        if (opts.Language.Equals("csharp", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(opts.Namespace))
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            opts.Namespace = $"{currentDir.Name}.Generated";
        }

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
        
        var parser = new MarkdigParser();
        var generator = GetGenerator(opts.Language);

        if (generator == null)
        {
            Console.WriteLine($"Unsupported language: {opts.Language}");
            Console.WriteLine("Supported languages: csharp, typescript");
            return;
        }

        // Validate namespace is provided for C#
        if (opts.Language.Equals("csharp", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(opts.Namespace))
        {
            Console.WriteLine("Namespace parameter is required for C# code generation");
            return;
        }

        foreach (var specFile in specFiles)
        {
            Console.WriteLine($"Processing spec file: {specFile}");
            var content = await File.ReadAllTextAsync(specFile);
            var spec = parser.Parse(content);
            
            var baseFileName = ToValidIdentifier(spec.Name);
            var stepsClassName = $"{baseFileName}Steps";
            
            var fileExtension = GetFileExtension(opts.Language);
            
            // Generate Steps interface class
            var stepsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.steps{fileExtension}");
            var stepsCode = generator.GenerateSteps(spec, opts.Namespace, stepsClassName);
            await File.WriteAllTextAsync(stepsPath, stepsCode);
            Console.WriteLine($"Generated steps file: {stepsPath}");
            
            // Generate Tests class
            var testsPath = Path.Combine(opts.OutputPath, $"{baseFileName}.tests{fileExtension}");
            var testCode = generator.GenerateTests(spec, opts.Namespace, baseFileName);
            await File.WriteAllTextAsync(testsPath, testCode);
            Console.WriteLine($"Generated tests file: {testsPath}");
        }
    }

    private static ICodeGenerator? GetGenerator(string language) => language.ToLower() switch
    {
        "csharp" => new CSharpGenerator(),
        "typescript" => new TypeScriptGenerator(),
        _ => null
    };

    private static string GetFileExtension(string language) => language.ToLower() switch
    {
        "csharp" => ".g.cs",
        "typescript" => ".ts",
        _ => ".txt"
    };

    private static string ToValidIdentifier(string input)
    {
        // Split on any non-word character and convert to PascalCase
        var words = System.Text.RegularExpressions.Regex.Split(input, @"[^\w]")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
        
        return string.Join("", words);
    }
}