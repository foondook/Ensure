using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Ensure.Generator.Models;
using Ensure.Generator.Abstractions;

namespace Ensure.Generator.Parsing;

public class MarkdigParser : ISpecParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdigParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public Spec Parse(string content)
    {
        var document = Markdown.Parse(content, _pipeline);
        var blocks = document.Skip(1).ToList(); // Skip the first heading which is the spec name
        
        var specName = document[0] is HeadingBlock heading
            ? GetInlineText(heading)
            : "Untitled";

        var background = new List<Step>();
        var scenarios = new List<Scenario>();
        
        Scenario? currentScenario = null;

        foreach (var block in blocks)
        {
            switch (block)
            {
                case HeadingBlock h when h.Level == 2:
                    if (currentScenario != null)
                    {
                        scenarios.Add(currentScenario);
                    }
                    currentScenario = new Scenario(GetInlineText(h), new List<Step>());
                    break;

                case ListBlock list:
                    foreach (var item in list)
                    {
                        if (item is ListItemBlock listItem)
                        {
                            var stepText = string.Empty;
                            foreach (var itemBlock in listItem)
                            {
                                if (itemBlock is ParagraphBlock p)
                                {
                                    stepText = GetInlineText(p);
                                    break;
                                }
                            }
                            
                            if (!string.IsNullOrWhiteSpace(stepText))
                            {
                                var step = new Step(stepText, ExtractParameters(stepText));
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
                    }
                    break;
            }
        }

        if (currentScenario != null)
        {
            scenarios.Add(currentScenario);
        }

        return new Spec(specName, background, scenarios);
    }

    private string GetInlineText(LeafBlock block)
    {
        if (block.Inline == null) return string.Empty;
        
        var text = string.Empty;
        foreach (var inline in block.Inline)
        {
            if (inline is LiteralInline literal)
            {
                text += literal.Content.ToString();
            }
            else if (inline is ContainerInline container)
            {
                foreach (var child in container)
                {
                    if (child is LiteralInline childLiteral)
                    {
                        text += childLiteral.Content.ToString();
                    }
                }
            }
        }
        return text.Trim();
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