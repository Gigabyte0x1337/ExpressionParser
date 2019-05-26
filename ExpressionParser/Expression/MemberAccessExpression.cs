namespace ExpressionParser
{
    public class MemberAccessExpression : Expression
    {
        public Expression Target { get; set; }

        public Expression Member { get; set; }
    }
}