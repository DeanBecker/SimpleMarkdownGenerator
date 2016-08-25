using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarkdownGenerator.Parser;
using System.Collections;

namespace MarkdownGenerator.Tests.Parser
{
    [TestClass]
    public class CSharpParserTests
    {
        private string validCode = @"
            using System;
        
            namespace TestApplication
            {
                public class MyTestClass
                {
                    public MyTestClass(string arg1) { }
                    public void MyTestMethod() { /* Comments */ }
                    public int MyTestComplexMethod(int a, int b)
                    {
                        return a + b;
                    }
                }
            }
        ";

        [TestMethod]
        public void CSharpParserParsesCorrectly()
        {
            var parser = new CSharpParser(validCode);
            var methods = (IList)parser.GetMethods();

            Assert.AreEqual(2, methods.Count);
        }
    }
}
