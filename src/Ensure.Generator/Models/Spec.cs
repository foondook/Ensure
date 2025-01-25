namespace Ensure.Generator.Models;

public record Spec(string Name, List<Step> Background, List<Scenario> Scenarios);
public record Scenario(string Name, List<Step> Steps);
public record Step(string Text, Dictionary<string, string> Parameters); 