using ExpressionParser.Expression;

namespace ExpressionParser.Parser
{
    internal class AssignmentExpression : Expression.Expression
    {
        public Expression.Expression Left { get; set; }
        public Expression.Expression Right { get; set; }
        public TokenType Type { get; set; }
    }
}