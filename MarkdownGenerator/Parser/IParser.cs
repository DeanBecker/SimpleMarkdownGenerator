using System.Collections;

namespace MarkdownGenerator.Parser
{
    public interface IParser<TProduct>
    {
        TProduct Parse();
    }
}