using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using MarkdownGenerator.Parser;
using MarkdownGenerator.Templating;

namespace MarkdownGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var tFile = args[0];
            if (!tFile.EndsWith(".mdgen"))
                throw new ArgumentException("First parameter needs to be an .mdgen file.");

            var cFile = args[1];
            foreach (var f in new List<string> { tFile, cFile })
            {
                if (!File.Exists(f))
                    throw new FileNotFoundException($"File not found: {f}");
            }

            try
            {
                var rawTemplate = File.ReadAllText(tFile);
                var code = File.ReadAllText(cFile);

                var template = new Template(rawTemplate, new object());

                var parser = new CSharpParser(code);
                var methods = parser.GetMethods();
                foreach (var method in methods)
                {
                    Console.WriteLine($"Method: {method.Identifier}");
                    Console.WriteLine($"Args: {string.Join<ParameterSyntax>(", ", method.ParameterList.Parameters.ToArray())}");
                    Console.WriteLine($"Returns: {method.ReturnType.ToString()}");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        public string GetMethod()
        {
            return "";
        }

        public List<int> ReturnList(int a, int b)
        {
            return new List<int> { a, b };
        }
    }
}
