using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownGenerator.Templating
{
    public class Template : ITemplate
    {
        private const string _startingPlaceholder = "{{";
        private const string _endingPlaceholder = "}}";

        public Template()
        {
            Values = new Dictionary<string, object>();
        }

        public Template(string template)
            : this()
        {
            Text = template;
        }

        public string Text { get; set; }
        public IDictionary<string, object> Values { get; }

        public string Placeholder
        {
            get
            {
                return $"{_startingPlaceholder} {_endingPlaceholder}";
            }
        }

        public string WrapToken(string token)
        {
            return $"{_startingPlaceholder}{token}{_endingPlaceholder}";
        }
    }
}
