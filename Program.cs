using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

// CommonMark specification: https://spec.commonmark.org/
const string SpecUrl = "https://spec.commonmark.org/0.31.2/spec.json";

Console.WriteLine("CommonMark Spec Tester using Markdig");
Console.WriteLine("======================================");
Console.WriteLine($"Fetching spec from: {SpecUrl}");
Console.WriteLine();

List<SpecExample> examples;

using (var http = new HttpClient())
{
    examples = await http.GetFromJsonAsync<List<SpecExample>>(SpecUrl)
        ?? throw new InvalidOperationException("Failed to deserialize spec.json");
}

Console.WriteLine($"Loaded {examples.Count} test cases.");
Console.WriteLine();

var pipeline = new MarkdownPipelineBuilder()
    // Uncomment to test with the List extension
    // .Use(new CommonMarkListExtension())
    .Build();

int passed = 0;
int failed = 0;

foreach (var example in examples)
{
    string actual = Markdown.ToHtml(example.Markdown, pipeline).TrimEnd();
    string expected = example.Html.TrimEnd();

    bool ok = string.Equals(actual, expected, StringComparison.Ordinal);

    if (ok)
    {
        passed++;
    }
    else
    {
        failed++;
        Console.WriteLine($"FAIL  Example #{example.Example} [{example.Section}]");
        Console.WriteLine($"      Markdown : {Escape(example.Markdown)}");
        Console.WriteLine($"      Expected : {Escape(expected)}");
        Console.WriteLine($"      Got      : {Escape(actual)}");
        Console.WriteLine();
    }
}

Console.WriteLine("======================================");
Console.WriteLine($"Results: {passed} passed, {failed} failed out of {examples.Count} total");

static string Escape(string s) =>
    s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

public class SpecExample
{
    [JsonPropertyName("markdown")]
    public string Markdown { get; set; } = "";

    [JsonPropertyName("html")]
    public string Html { get; set; } = "";

    [JsonPropertyName("example")]
    public int Example { get; set; }

    [JsonPropertyName("start_line")]
    public int StartLine { get; set; }

    [JsonPropertyName("end_line")]
    public int EndLine { get; set; }

    [JsonPropertyName("section")]
    public string Section { get; set; } = "";
}

// Fixes a Markdig rendering difference from the CommonMark spec: Markdig omits the \n
// after <li> when the list item contains block-level content (e.g. <p>, <hr />, <h1>-<h6>).
internal sealed class CommonMarkListExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.Replace<ListRenderer>(new FixedListRenderer());
        }
    }
}

internal sealed class FixedListRenderer : HtmlObjectRenderer<ListBlock>
{
    protected override void Write(HtmlRenderer renderer, ListBlock listBlock)
    {
        renderer.EnsureLine();
        var savedImplicit = renderer.ImplicitParagraph;
        renderer.ImplicitParagraph = !listBlock.IsLoose;

        if (listBlock.IsOrdered)
        {
            renderer.Write("<ol");
            if (listBlock.OrderedStart != null && listBlock.OrderedStart != listBlock.DefaultOrderedStart)
            {
                renderer.Write(" start=\"").Write(listBlock.OrderedStart).Write("\"");
            }

            renderer.WriteLine(">");
        }
        else
        {
            renderer.WriteLine("<ul>");
        }

        foreach (var itemObj in listBlock)
        {
            var item = (ListItemBlock)itemObj;
            renderer.EnsureLine();
            renderer.Write("<li>");

            if (item.Count > 0 && (!renderer.ImplicitParagraph || item[0] is not ParagraphBlock))
            {
                renderer.WriteLine();
            }

            renderer.WriteChildren(item);
            renderer.Write("</li>");
            renderer.WriteLine();
        }

        renderer.ImplicitParagraph = savedImplicit;

        if (listBlock.IsOrdered)
        {
            renderer.WriteLine("</ol>");
        }
        else
        {
            renderer.WriteLine("</ul>");
        }
    }
}
