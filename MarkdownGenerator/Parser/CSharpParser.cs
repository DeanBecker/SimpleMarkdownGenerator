using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections;

namespace MarkdownGenerator.Parser
{
    public class CSharpParser : IParser<CompilationUnitSyntax>
    {
        private string _code;
        private CompilationUnitSyntax _cachedRoot;
        private CompilationUnitSyntax _root
        {
            get
            {
                if (_cachedRoot == null)
                {
                    Parse();
                }
                return _cachedRoot;
            }
        }

        public CSharpParser(string code)
        {
            _code = code;
        }

        public CompilationUnitSyntax Parse()
        {
            var tree = CSharpSyntaxTree.ParseText(_code);
            var root = _cachedRoot = (CompilationUnitSyntax)tree.GetRoot();
            return root;
        }

        public IList<MethodDeclarationSyntax> GetMethods()
        {
            return _root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }
    }
}
