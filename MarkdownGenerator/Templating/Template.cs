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

        public Template(string template, object data)
        {
            Text = template;
            Data = data;
        }

        public string Text { get; set; }
        public object Data { get; }

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
