using System.Collections.Generic;

namespace MarkdownGenerator.Templating
{
    public interface ITemplate
    {
        string Placeholder { get; }
        string Text { get; set; }
        IDictionary<string, object> Values { get; }

        string WrapToken(string token);
    }
}