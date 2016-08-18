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

        private string RegexPattern
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
            offset = 0;
            string result = InnerCompile(_template.Text);
            return result;
        }

        private int offset = 0;
        private string InnerCompile(string text, string prefix = null)
        {
            Stack<Match> Nesting = new Stack<Match>();

            var result = text;
            var placeholders = Regex.Matches(result, RegexPattern);
            for (int i = 0; i < placeholders.Count; i++)
            {
                var match = placeholders[i];

                var key = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);

                switch (key.First())
                {
                    case '*':
                        Nesting.Push(match); // Start of block
                        break;

                    case '/':
                        var poppedMatch = Nesting.Pop(); // End of block
                        int poppedIndex = poppedMatch.Index - offset, endIndex = match.Index - offset;
                        var startOfBlock = poppedIndex;
                        var endOfBlock = endIndex + match.Length;
                        var innerText = result.Substring(startOfBlock + poppedMatch.Length, endIndex - (startOfBlock + poppedMatch.Length)).Trim();

                        result = result.Remove(startOfBlock, endOfBlock - startOfBlock);

                        const string methodParametersPattern = @"\([^)]+\)";
                        var methodParameters = Regex.Match(poppedMatch.Value, methodParametersPattern).Value?
                                                    .Replace("(", string.Empty).Replace(")", string.Empty);
                        if (methodParameters == null)
                            throw new ArgumentException($"Malformed construct: {poppedMatch.Value} : {poppedMatch.Index}");
                        result = result.Insert(startOfBlock, InnerCompile(innerText, methodParameters)); // Recurse to resolve loops
                        break;

                    default:
                        if (Nesting.Count == 0)
                        {
                            // Perform subsitution on top-level placeholders
                            object val;
                            if (!_template.Values.TryGetValue(prefix ?? key, out val))
                                continue;

                            if (prefix != null)
                            {
                                var enumerableVal = (IEnumerable)val;
                                var replacementText = new StringBuilder();
                                foreach (var e in enumerableVal)
                                {
                                    if (match.Value == this.DefaultValue)
                                    {
                                        replacementText.AppendLine(e.ToString());
                                    }
                                    else
                                    {
                                        // Parse complex object
                                        //TODO: Major refactor to be able to compile sub-templates as a whole
                                    }
                                }
                                const string newLine = "\r\n";
                                replacementText.Remove(replacementText.Length - newLine.Length, newLine.Length);
                                result = result.Replace(match.Value, replacementText.ToString());
                                offset += (match.Value.Length - replacementText.Length);
                            }
                            else
                            {
                                result = result.Replace(match.Value, val.ToString());
                                // Keep offset caused by changes in length of replaced text
                                // to save recalculating Regex after every change.
                                offset += (match.Value.Length - val.ToString().Length);
                            }
                        }
                        break;
                }
            }

            return result;
        }
    }
}
