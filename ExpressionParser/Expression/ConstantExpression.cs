using ExpressionParser.Parser;

namespace ExpressionParser.Expression
{
    public class ConstantExpression : Expression
    {
        public string Value { get; set; }

        public TokenType Type { get; set; }
    }
}