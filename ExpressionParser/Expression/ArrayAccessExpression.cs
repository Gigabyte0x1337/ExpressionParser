namespace ExpressionParser
{
    public class ArrayAccessExpression : Expression
    {
        public Expression Target { get; set; }

        public Expression Index { get; set; }
    }
}