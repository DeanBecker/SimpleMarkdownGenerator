using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MarkdownGenerator.Templating;
using System.Text.RegularExpressions;

namespace MarkdownGenerator.Tests.Templating
{
    [TestClass]
    public class TemplateEmitterTests
    {
        string simpleTemplate = @"
                                {{PlaceholderA}}Some other text
                                {{PlaceholderB}}{{PlaceholderC}}
                                ";
        IDictionary<string, object> vals = new Dictionary<string, object>
        {
            { "PlaceholderA", "Test" },
            { "PlaceholderB", 123 },
            { "PlaceholderC", "Test2" }
        };
        string simpleCompiledTemplate = @"
                                TestSome other text
                                123Test2
                                ";

        [TestMethod]
        public void SimpleTemplateCompilesSuccessfully()
        {
            var template = new Template(simpleTemplate);
            foreach (var kvp in vals)
            {
                template.Values.Add(kvp);
            }

            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(simpleCompiledTemplate, result);
        }


        string iterativeTemplate = @"{{PlaceholderA}}
                                    {{*ForEach(IterativePlaceholder)}}
                                    {{Value}}
                                    {{/EndForEach}}
                                    {{PlaceholderB}}";
        IDictionary<string, object> iterativeVals = new Dictionary<string, object>
        {
            { "PlaceholderA", "Iterative Test" },
            { "IterativePlaceholder", new int[] { 1, 2, 3, 4, 5 } },
            { "PlaceholderB", "Iterative Test End" }
        };
        string iterativeCompiledTemplate = @"Iterative Test
                                    1
                                    2
                                    3
                                    4
                                    5
                                    Iterative Test End";

        [TestMethod]
        public void TemplateWithIterationCompilesSuccessfully()
        {
            var template = new Template(iterativeTemplate);
            foreach (var kvp in iterativeVals)
            {
                template.Values.Add(kvp);
            }

            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            var normalisedExpectation = Regex.Replace(iterativeCompiledTemplate, @"\s", "");
            var normalisedResult = Regex.Replace(result, @"\s", "");
            Assert.IsTrue(string.Equals(normalisedExpectation, normalisedResult, StringComparison.OrdinalIgnoreCase));
        }

        string complexIterativeTemplate = @"
{{PlaceholderA}}
{{*ForEach(IterativePlaceholder)}}
Title: {{Title}}
Subtitle: {{Subtitle}}
{{/EndForEach}}
{{PlaceholderB}}
";
        IDictionary<string, object> complexIterativeVals = new Dictionary<string, object>
        {
            { "PlaceholderA", "Complex Iterative Test" },
            { "PlaceholderB", "Complex Iterative Test End" },
            { "IterativePlaceholder", new object[]
            {
                new { Title = "Test 1", Subtitle = "Subtitle 1" },
                new { Title = "Test 2", Subtitle = "Subtitle 2" },
                new { Title = "Test 3", Subtitle = "Subtitle 3" }
            } }
        };
        string complexIterativeCompiledTemplate = @"
Complex Iterative Test
Title: Test 1
Subtitle: Subtitle 1
Title: Test 2
Subtitle: Subtitle 2
Title: Test 3
Subtitle: Subtitle 3
Complex Iterative Test End
";
        [TestMethod]
        public void ComplexTemplateWithIterationCompilesSuccessfully()
        {
            var template = new Template(complexIterativeTemplate);
            foreach (var kvp in complexIterativeVals)
            {
                template.Values.Add(kvp);
            }

            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(complexIterativeCompiledTemplate, result);
        }
    }
}
