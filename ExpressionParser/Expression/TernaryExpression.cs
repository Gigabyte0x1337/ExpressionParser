namespace ExpressionParser.Expression
{
    public class TernaryExpression : Expression
    {
        public Expression Condition { get; set; }

        public Expression TrueExpression { get; set; }

        public Expression FalseExpression { get; set; }
    }
}