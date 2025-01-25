namespace Ensure.Generator.Parsing;

using System.Text.RegularExpressions;
using Ensure.Generator.Abstractions;
using Ensure.Generator.Models;

public class SimpleMarkdownParser : ISpecParser
{
    public Spec Parse(string content)
    {
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
} 