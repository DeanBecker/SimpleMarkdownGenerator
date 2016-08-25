using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownGenerator.Templating
{
    public class TemplateEmitter
    {
        private ITemplate _template;

        public TemplateEmitter(ITemplate template)
        {
            _template = template;
        }

        private string PlaceholderPattern
        {
            get
            {
                string[] placeholders = _template.Placeholder.Split(' ');
                if (placeholders.Length != 2)
                    throw new ArgumentException($"{_template.GetType().ToString()} does not implement ITemplate.Placeholder properly.");
                return $"{placeholders[0]}[^{placeholders[1].First()}{{2}}]*{placeholders[1]}";
            }
        }

        private string DefaultValue
        {
            get
            {
                return _template.WrapToken("Value");
            }
        }

        public string Compile()
        {
            string result = InnerCompile(_template.Text, _template.Data);
            return result;
        }

        private string InnerCompile(string template, dynamic data)
        {
            var result = new StringBuilder(template);
            var placeholders = Regex.Matches(template, PlaceholderPattern);
            int offset = 0;
            var indentStack = new Stack<Match>();
            int maxStack = 0;
            var indentPairs = new List<Tuple<Match, Match>>();

            foreach (Match placeholder in placeholders)
            {
                var key = placeholder.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                switch (key.First())
                {
                    case '*': // Start of block
                        indentStack.Push(placeholder);
                        break;
                    case '/': // End of block
                        var startTag = indentStack.Pop();
                        if (indentStack.Count() == 0)
                            indentPairs.Add(new Tuple<Match, Match>(startTag, placeholder));
                        maxStack++;
                        break;
                    default:
                        if (indentStack.Count() == 0)
                        {
                            string value;
                            if (placeholder.Value.Equals(this.DefaultValue))
                            {
                                value = data.ToString();
                            }
                            else
                            {
                                value = data.GetType().GetProperty(key)?.GetValue(data).ToString();
                                if (value == null)
                                    continue;
                            }
                            result = result.Replace(placeholder.Value, value, placeholder.Index - offset, placeholder.Length);
                            if (maxStack == 0)
                            {
                                // Keep offset caused by changes in length of replaced text
                                // to save recalculating Regex after every change.
                                offset += (placeholder.Value.Length - value.Length);
                            }
                        }
                        break;
                }
            }

            foreach (var mostOuterBlock in indentPairs)
            {
                const string methodParametersPattern = @"\([^)]+\)";
                var methodParameters = Regex.Match(mostOuterBlock.Item1.Value, methodParametersPattern).Value?
                                            .Replace("(", string.Empty).Replace(")", string.Empty);

                var enumerable = data.GetType().GetProperty(methodParameters).GetValue(data);
                if (enumerable is IEnumerable)
                {
                    var block = GetBlockInfo(mostOuterBlock.Item1, mostOuterBlock.Item2, offset);
                    var innerResult = new StringBuilder();
                    var innerContent = result.ToString(block.ContentStart, block.ContentLength).TrimStart();
                    foreach (var d in enumerable)
                    {
                        var r = InnerCompile(innerContent, d); // Recurse to resolve inner blocks
                        innerResult.Append(r);
                    }
                    var trimmedInnerResult = innerResult.ToString().TrimEnd();
                    result = result.Insert(block.End, trimmedInnerResult);
                    result = result.Remove(block.Start, block.Length);
                    offset += block.Length - trimmedInnerResult.Length;
                }
            }

            return result.ToString();
        }

        private struct BlockInfo
        {
            public int Start;
            public int End;
            public int Length;
            public int ContentStart;
            public int ContentEnd;
            public int ContentLength;
            public int Offset;
        }

        private static BlockInfo GetBlockInfo(Match startTag, Match endTag, int offset = 0)
        {
            var info = new BlockInfo()
            {
                Offset = offset
            };

            info.Start = startTag.Index - offset;
            info.ContentEnd = endTag.Index - offset;
            info.End = info.ContentEnd + endTag.Length;
            info.ContentStart = info.Start + startTag.Length;

            info.Length = info.End - info.Start;
            info.ContentLength = info.ContentEnd - info.ContentStart;

            return info;
        }
    }
}
