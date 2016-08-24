using System.Collections.Generic;

namespace MarkdownGenerator.Templating
{
    public interface ITemplate
    {
        string Placeholder { get; }
        string Text { get; set; }
        object Data { get; }

        string WrapToken(string token);
    }
}