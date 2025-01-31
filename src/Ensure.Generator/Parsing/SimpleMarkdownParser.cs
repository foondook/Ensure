namespace Ensure.Generator.Parsing;

using System.Text.RegularExpressions;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Models;

public class SimpleMarkdownParser : ISpecParser
{
    public Spec Parse(string content)
    {
        var lines = content.Split('\n');
        var specName = string.Empty;
        var background = new List<Step>();
        var scenarios = new List<Scenario>();
        
        Scenario? currentScenario = null;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines and table lines (lines containing |)
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.Contains('|'))
                continue;

            if (trimmedLine.StartsWith("# "))
            {
                specName = trimmedLine[2..].Trim();
            }
            else if (trimmedLine.StartsWith("## "))
            {
                if (currentScenario != null)
                {
                    scenarios.Add(currentScenario);
                }
                currentScenario = new Scenario(trimmedLine[3..].Trim(), new List<Step>());
            }
            else if (!string.IsNullOrWhiteSpace(trimmedLine))
            {
                var step = new Step(trimmedLine, ExtractParameters(trimmedLine));
                if (currentScenario != null)
                {
                    currentScenario.Steps.Add(step);
                }
                else
                {
                    background.Add(step);
                }
            }
        }

        if (currentScenario != null)
        {
            scenarios.Add(currentScenario);
        }

        return new Spec(specName, background, scenarios);
    }

    private Dictionary<string, string> ExtractParameters(string text)
    {
        var parameters = new Dictionary<string, string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"""([^""]+)""");
        
        for (int i = 0; i < matches.Count; i++)
        {
            var value = matches[i].Groups[1].Value;
            parameters[$"param{i + 1}"] = value;
        }
        
        return parameters;
    }
} 