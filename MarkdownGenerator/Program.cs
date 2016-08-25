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
                var codeFile = new FileInfo(cFile);
                var code = File.ReadAllText(cFile);

                var parser = new CSharpParser(code);
                var methods = parser.GetMethods();

                var data = new
                {
                    ProjectTitle = codeFile.Name,
                    Methods = methods.Select(m => new
                    {
                        Identifier = m.Identifier.ToString(),
                        ArgsList = string.Join<ParameterSyntax>(", ", m.ParameterList.Parameters.ToArray()),
                        Args = m.ParameterList.Parameters.Select(p => new { Identifier = p.Identifier.Value, Type = p.Type.ToString() }),
                        ReturnType = m.ReturnType.ToString()
                    })
                };

                var template = new Template(rawTemplate, data);
                var emitter = new TemplateEmitter(template);

                var compiledTemplate = emitter.Compile();

                if (!Directory.Exists("docs"))
                    Directory.CreateDirectory("docs");
                var output = File.CreateText($"docs\\{codeFile.Name}.md");
                output.Write(compiledTemplate);
                output.FlushAsync();

                Console.WriteLine($"Documentation compiled for {codeFile.Name}");
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
