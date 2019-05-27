namespace ExpressionParser.Expression
{
    public class UnaryExpression : Expression
    {
        public UnaryOperator Operator { get; set; }

        public Expression Expression { get; set; }
    }
}