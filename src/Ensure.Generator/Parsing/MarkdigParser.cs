using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Ensure.Generator.Models;
using Ensure.Generator.Abstractions;
using Markdig.Extensions.Tables;

namespace Ensure.Generator.Parsing;

public class MarkdigParser : ISpecParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdigParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseGridTables()
            .UsePipeTables()
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

        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            Console.WriteLine($"Processing block type: {block.GetType().Name}");
            
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
                            Table? associatedTable = null;

                            foreach (var itemBlock in listItem)
                            {
                                if (itemBlock is ParagraphBlock p)
                                {
                                    // Only take the text before any table data
                                    stepText = GetInlineText(p).Split('|')[0].Trim();
                                }
                                else if (itemBlock is Table table)
                                {
                                    Console.WriteLine("Found table within list item");
                                    associatedTable = table;
                                }
                            }
                            
                            if (!string.IsNullOrWhiteSpace(stepText))
                            {
                                ProcessStep(stepText, associatedTable, currentScenario, background, ref i, blocks);
                            }
                        }
                    }
                    break;

                case ParagraphBlock p:
                    var text = GetInlineText(p);
                    ProcessStep(text, null, currentScenario, background, ref i, blocks);
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

    private Dictionary<string, string> ExtractParameters(string text, Table? table = null)
    {
        var parameters = ExtractParameters(text);
        
        if (table != null)
        {
            var tableData = ParseTable(table);
            if (tableData.Any())
            {
                var json = System.Text.Json.JsonSerializer.Serialize(tableData);
                Console.WriteLine($"Table data: {json}");
                parameters["tableData"] = json;
            }
        }
        
        return parameters;
    }

    private string GetTableCellText(TableCell cell)
    {
        if (cell.Count == 0) return string.Empty;
        
        var text = string.Empty;
        foreach (var block in cell)
        {
            if (block is ParagraphBlock p)
            {
                text += GetInlineText(p);
            }
        }
        return text.Trim();
    }

    private List<Dictionary<string, string>> ParseTable(Table table)
    {
        var result = new List<Dictionary<string, string>>();
        
        // Get all rows including header
        var rows = table.Descendants<TableRow>().ToList();
        if (rows.Count < 2) // Need at least header and one data row
            return result;

        // Extract headers from the first row
        var headers = rows[0].Descendants<TableCell>()
            .Select(GetTableCellText)
            .ToList();

        // Process data rows
        foreach (var row in rows.Skip(1))
        {
            var rowData = new Dictionary<string, string>();
            var cells = row.Descendants<TableCell>().ToList();
            
            for (int i = 0; i < headers.Count && i < cells.Count; i++)
            {
                rowData[headers[i]] = GetTableCellText(cells[i]);
            }
            
            result.Add(rowData);
        }

        return result;
    }

    private void ProcessStep(string stepText, Table? associatedTable, Scenario? currentScenario, List<Step> background, ref int blockIndex, List<Block> blocks)
    {
        Console.WriteLine($"Found step: {stepText}");
        
        // If no table was found within the list item, look for one after it
        if (associatedTable == null)
        {
            var nextBlock = blockIndex < blocks.Count - 1 ? blocks[blockIndex + 1] : null;
            if (nextBlock is Table table)
            {
                Console.WriteLine("Found table after step");
                associatedTable = table;
                blockIndex++; // Skip the table in the next iteration
            }
        }

        var step = new Step(stepText, ExtractParameters(stepText, associatedTable));
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