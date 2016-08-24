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
        #region Simple Test Mocks
        string simpleTemplate = @"
                                {{PlaceholderA}}Some other text
                                {{PlaceholderB}}{{PlaceholderC}}
                                ";
        object vals = new
        {
            PlaceholderA = "Test",
            PlaceholderB = 123,
            PlaceholderC = "Test2"
        };
        string simpleCompiledTemplate = @"
                                TestSome other text
                                123Test2
                                ";
        #endregion
        [TestMethod]
        public void SimpleTemplateCompilesSuccessfully()
        {
            var template = new Template(simpleTemplate, vals);
            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(simpleCompiledTemplate, result);
        }

        #region Iterative Test Mocks
        string iterativeTemplate = @"{{PlaceholderA}}
                                    {{*(IterativePlaceholder)}}
                                    {{Value}}
                                    {{/}}
                                    {{PlaceholderB}}";
        object iterativeVals = new
        {
            PlaceholderA = "Iterative Test",
            IterativePlaceholder = new int[] { 1, 2, 3, 4, 5 },
            PlaceholderB = "Iterative Test End"
        };
        string iterativeCompiledTemplate = @"Iterative Test
                                    1
                                    2
                                    3
                                    4
                                    5
                                    Iterative Test End";
        #endregion
        [TestMethod]
        public void TemplateWithIterationCompilesSuccessfully()
        {
            var template = new Template(iterativeTemplate, iterativeVals);
            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            var normalisedExpectation = Regex.Replace(iterativeCompiledTemplate, @"\s", "");
            var normalisedResult = Regex.Replace(result, @"\s", "");
            Assert.IsTrue(string.Equals(normalisedExpectation, normalisedResult, StringComparison.OrdinalIgnoreCase));
        }

        #region Complex Test Mocks
        string complexIterativeTemplate = @"
{{PlaceholderA}}
{{*ForEach(IterativePlaceholder)}}
Title: {{Title}}
Subtitle: {{Subtitle}}
{{/EndForEach}}
{{PlaceholderB}}
";
        object complexIterativeVals = new
        {
            PlaceholderA = "Complex Iterative Test",
            PlaceholderB = "Complex Iterative Test End",
            IterativePlaceholder = new object[]
            {
                new { Title = "Test 1", Subtitle = "Subtitle 1" },
                new { Title = "Test 2", Subtitle = "Subtitle 2" },
                new { Title = "Test 3", Subtitle = "Subtitle 3" }
            }
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
#endregion
        [TestMethod]
        public void ComplexTemplateWithIterationCompilesSuccessfully()
        {
            var template = new Template(complexIterativeTemplate, complexIterativeVals);
            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(complexIterativeCompiledTemplate, result);
        }

        #region Nested Test Mocks
        string nestedIterativeTemplate = @"
{{PlaceholderA}}
{{*(OuterNest)}}
{{*(InnerNest)}}
Title: {{InnerTitle}}
{{/}}
{{Title}}
{{/}}
{{PlaceholderB}}
";
        object nestedIterativeVals = new
        {
            PlaceholderA = "Nest Iterative Test",
            PlaceholderB = "Nest Iterative Test End",
            OuterNest = new object[]
            {
                new
                {
                    Title = "Outer Title 1",
                    InnerNest = new object[]
                    {
                        new
                        {
                            InnerTitle = "Inner Title 1a"
                        }
                    }
                },
                new
                {
                    Title = "Outer Title 2",
                    InnerNest = new object[]
                    {
                        new
                        {
                            InnerTitle = "Inner Title 2a"
                        },
                        new
                        {
                            InnerTitle = "Inner Title 2b"
                        }
                    }
                },
                new
                {
                    Title = "Outer Title 3",
                    InnerNest = new object[]
                        {
                            new
                            {
                                InnerTitle = "Inner Title 3a"
                            }
                        }
                }
            }
        };
        string nestedIterativeResult = @"
Nest Iterative Test
Title: Inner Title 1a
Outer Title 1
Title: Inner Title 2a
Title: Inner Title 2b
Outer Title 2
Title: Inner Title 3a
Outer Title 3
Nest Iterative Test End
";
        #endregion
        [TestMethod]
        public void NestedIterativeTemplateCompilesSuccessfully()
        {
            var template = new Template(nestedIterativeTemplate, nestedIterativeVals);
            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(nestedIterativeResult, result);
        }

        #region Duplicate Test Mocks
        string duplicateIterativeTemplate = @"
{{PlaceholderA}}
{{*(OuterNest)}}
{{*(InnerNest)}}
Title: {{Title}}
{{/}}
{{Title}}
{{/}}
{{PlaceholderB}}
";
        object duplicateIterativeVals = new
        {
            PlaceholderA = "Nest Iterative Test",
            PlaceholderB = "Nest Iterative Test End",
            OuterNest = new object[]
            {
                new
                {
                    Title = "Outer Title 1",
                    InnerNest = new object[]
                    {
                        new
                        {
                            Title = "Inner Title 1a"
                        }
                    }
                },
                new
                {
                    Title = "Outer Title 2",
                    InnerNest = new object[]
                    {
                        new
                        {
                            Title = "Inner Title 2a"
                        },
                        new
                        {
                            Title = "Inner Title 2b"
                        }
                    }
                },
                new
                {
                    Title = "Outer Title 3",
                    InnerNest = new object[]
                        {
                            new
                            {
                                Title = "Inner Title 3a"
                            }
                        }
                }
            }
        };
        string duplicateIterativeResult = @"
Nest Iterative Test
Title: Inner Title 1a
Outer Title 1
Title: Inner Title 2a
Title: Inner Title 2b
Outer Title 2
Title: Inner Title 3a
Outer Title 3
Nest Iterative Test End
";
        #endregion
        [TestMethod]
        public void NestedIterativeTemplateWithDuplicateInnerMembersCompilesSuccessfully()
        {
            var template = new Template(duplicateIterativeTemplate, duplicateIterativeVals);
            var emitter = new TemplateEmitter(template);
            var result = emitter.Compile();

            Assert.AreEqual(duplicateIterativeResult, result);
        }
    }
}
