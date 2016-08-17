using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MarkdownGenerator.Templating;

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

            Assert.AreEqual(iterativeCompiledTemplate, result);
        }
    }
}
