namespace ExpressionParser
{
    public class UnaryExpression : Expression
    {
        public UnaryOperator Operator { get; set; }

        public Expression Expression { get; set; }
    }
}