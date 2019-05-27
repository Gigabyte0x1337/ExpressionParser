using System;
using System.Collections.Generic;
using ExpressionParser.Parser;
using Newtonsoft.Json;

namespace ExpressionParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var globals = new Dictionary<string, object>()
            {
                {"parseInt", (Func<string, int>) (int.Parse)},
                {"eval", (Func<string, string>) ((s) => Eval(s))}
            };
            var i = 1;

            i *= i += i += i;

          //  var l = Eval("parseInt(eval(\"1 + 1\")) + 1 > 2", globals);
        }

        public static string Eval(string s, Dictionary<string, object> globals = null)
        {
            var ast = new Parser.ExpressionParser(new Lexer(s)).Parse();
            var evaluator = new CSharpExpressionGenerator(globals ?? new Dictionary<string, object>());

            return evaluator.Invoke(ast).ToString();
        }
    }
}
