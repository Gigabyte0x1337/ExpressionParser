namespace ExpressionParser
{
    public class ConstantExpression : Expression
    {
        public string Value { get; set; }

        public TokenType Type { get; set; }
    }
}