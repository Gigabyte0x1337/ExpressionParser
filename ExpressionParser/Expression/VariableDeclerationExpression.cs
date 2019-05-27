using ExpressionParser.Parser;

namespace ExpressionParser.Expression
{
    public class VariableDeclerationExpression : Expression
    {
        public string Name { get; set; }

        public TokenType Type  { get; set; }
    }
}