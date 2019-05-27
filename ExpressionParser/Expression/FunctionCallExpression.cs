using System.Collections.Generic;

namespace ExpressionParser.Expression
{
    public class FunctionCallExpression : Expression
    {
        public string Name { get; set; }

        public List<Expression> Arguments { get; set; }
    }
}