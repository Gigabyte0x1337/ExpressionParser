using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ExpressionParser
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BinaryOperator Operator { get; set; }

        public Expression Right { get; set; }
    }
}