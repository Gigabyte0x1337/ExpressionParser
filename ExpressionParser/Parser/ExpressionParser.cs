using System;
using System.Collections.Generic;

namespace ExpressionParser
{
    public class ExpressionParser
    {
        private readonly Lexer _lexer;

        public Dictionary<TokenType, (int Precedence, Associativity Associativity, BinaryOperator Operator)> Operations =
            new Dictionary<TokenType, (int, Associativity, BinaryOperator)>()
            {
                { TokenType.Or,             (2, Associativity.Left,  BinaryOperator.Or) },
                { TokenType.And,            (3, Associativity.Left,  BinaryOperator.And) },
                { TokenType.Equal,          (4, Associativity.Left,  BinaryOperator.Equal) },
                { TokenType.NotEqual,       (4, Associativity.Left,  BinaryOperator.NotEqual) },
                { TokenType.GreaterThan,    (5, Associativity.Left,  BinaryOperator.GreaterThan) },
                { TokenType.LessThan,       (5, Associativity.Left,  BinaryOperator.LessThan) },
                { TokenType.GreaterThanOrEqual,    (5, Associativity.Left,  BinaryOperator.GreaterThanOrEqual) },
                { TokenType.LessOrEqualThan,    (5, Associativity.Left,  BinaryOperator.LessThanOrEqual) },
                { TokenType.Plus,           (6, Associativity.Left,  BinaryOperator.Add) },
                { TokenType.Minus,          (6, Associativity.Left,  BinaryOperator.Subtract) },
                { TokenType.Multiply,       (7, Associativity.Left,  BinaryOperator.Multiply) },
                { TokenType.Divide,         (7, Associativity.Left,  BinaryOperator.Divide) },
                { TokenType.Modulo,         (7, Associativity.Left,  BinaryOperator.Modulo) },
                { TokenType.Exponent,       (8, Associativity.Right, BinaryOperator.Exponent) },
            };

        public ExpressionParser(Lexer lexer)
        {
            _lexer = lexer;
        }

        /// <summary>
        /// ParseBinaryExpression Code
        /// </summary>
        /// <returns></returns>
        public Expression Parse()
        {
            var expression = ParseTernaryExpression();

            return expression;
        }

        /// <summary>
        /// Parses {conditionExpression}?{trueExpression}:{falseExpression}
        /// </summary>
        /// <returns></returns>
        public Expression ParseTernaryExpression()
        {
            var condition = ParseBinaryExpression(0);

            while (CheckAndConsume(TokenType.QuestionMark))
            {
                var trueExpression = ParseTernaryExpression();

                Expect(TokenType.Colon);

                var falseExpression = ParseTernaryExpression();


                var ternaryExpression = new TernaryExpression()
                {
                    Condition = condition,
                    TrueExpression = trueExpression,
                    FalseExpression = falseExpression
                };

                trueExpression.Parent = ternaryExpression;
                falseExpression.Parent = ternaryExpression;
                condition.Parent = ternaryExpression;

                condition = ternaryExpression;
            }

            return condition;
        }
        
        /// <summary>
        /// Parses all the binary operators + - * / % && ||... etc.
        /// </summary>
        /// <param name="precedence"></param>
        /// <returns></returns>
        private Expression ParseBinaryExpression(int precedence)
        {
            var left = ParseUnary();

            while (Operations.ContainsKey(_lexer.Current.Type) && Operations[_lexer.Current.Type].Precedence >= precedence)
            {
                var (prec, associativity, op) = Operations[_lexer.Current.Type];

                _lexer.Next();

                var right = ParseBinaryExpression(associativity == Associativity.Left ? 1 + prec : prec);

                var binaryOperator = new BinaryExpression()
                {
                    Left = left,
                    Right = right,
                    Operator = op
                };

                binaryOperator.Left.Parent = binaryOperator;
                binaryOperator.Right.Parent = binaryOperator;

                left = binaryOperator;
            }

            return left;
        }

        private Expression ParseUnary()
        {
            // ++{expression} or --{expression} or !{expression} or +{expression} or -{expression}
            if (Check(TokenType.Decrement) ||
                Check(TokenType.Increment) ||
                Check(TokenType.Not) ||
                Check(TokenType.Plus) ||
                Check(TokenType.Minus))
            {
                return ParsePrefixUnaryExpression();
            }

            var expression = ParseMemberAccessAndArrayAccess();

            // {expression}++ or {expression}--
            if (Check(TokenType.Decrement) ||
                Check(TokenType.Increment))
            {
                return ParsePostfixUnaryExpression(expression);
            }

            return expression;
        }

        private Expression ParseMemberAccessAndArrayAccess()
        {
            // constants / functions / variables
            var expression = ParsePrimary();

            // {expression}[{expression}]
            if (CheckAndConsume(TokenType.OpenBracket))
            {
                return ParseArrayAccess(expression);
            }

            // {expression}.{expression}
            if (CheckAndConsume(TokenType.Dot))
            {
                return ParseMemberAccess(expression);
            }

            return expression;
        }

        /// <summary>
        /// Parse numbers, identifiers, strings
        /// TODO Use nice types instead of token type
        /// </summary>
        /// <returns></returns>
        private Expression ParsePrimary()
        {
            if (Check(TokenType.Identifier))
            {
                return ParseFunctionCallAndVariableExpression();
            }

            if (Check(TokenType.Decimal) ||
                Check(TokenType.Int) ||
                Check(TokenType.StringLiteral) ||
                Check(TokenType.KeywordTrue) || 
                Check(TokenType.KeywordFalse))
            {
                var token = _lexer.Current;

                _lexer.Next();

                return new ConstantExpression()
                {
                    Value = token.Value,
                    Type = token.Type
                };
            }

            if (CheckAndConsume(TokenType.OpenParenthesis))
            {
                var expression = ParseTernaryExpression();

                Expect(TokenType.CloseParenthesis);

                var groupExpression = new GroupExpression()
                {
                    Expression = expression
                };

                expression.Parent = groupExpression;

                return groupExpression;
            }

            throw new Exception($"Unexpected token: {_lexer.Current.Type}");
        }

        /// <summary>
        /// Parse {expression} or {expression}({expression}, {expression})
        /// </summary>
        /// <returns></returns>
        private Expression ParseFunctionCallAndVariableExpression()
        {
            var identifier = _lexer.Current;

            _lexer.Next();

            if (Check(TokenType.OpenParenthesis))
            {
                var functionCall = new FunctionCallExpression
                {
                    Name = identifier.Value,
                    Arguments = new List<Expression>()
                };

                _lexer.Next();

                while (!Check(TokenType.CloseParenthesis))
                {
                    var argument = Parse();

                    argument.Parent = functionCall;

                    functionCall.Arguments.Add(argument);

                    CheckAndConsume(TokenType.Comma);
                }

                Expect(TokenType.CloseParenthesis);

                return functionCall;
            }

            return new VariableExpression()
            {
                Name = identifier.Value
            };
        }

        /// <summary>
        /// Parse ++{expression} and --{expression}
        /// </summary>
        /// <returns></returns>
        private Expression ParsePrefixUnaryExpression()
        {
            var type = _lexer.Current.Type;

            _lexer.Next();

            var typeMapping = new Dictionary<TokenType, UnaryOperator>()
            {
                { TokenType.Increment, UnaryOperator.IncrementPrefix },
                { TokenType.Decrement, UnaryOperator.DecrementPrefix },
                { TokenType.Not, UnaryOperator.Not },
                { TokenType.Minus, UnaryOperator.Negate },
                { TokenType.Plus, UnaryOperator.Positive }
            };

            var unary = new UnaryExpression()
            {
                Expression = ParseMemberAccessAndArrayAccess(),
                Operator = typeMapping[type]
            };

            unary.Expression.Parent = unary;

            return unary;
        }

        /// <summary>
        /// Parse {expression}[{expression}]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression ParseArrayAccess(Expression expression)
        {
            var arrayAccessExpression = new ArrayAccessExpression()
            {
                Target = expression,
                Index = ParseBinaryExpression(0)
            };

            arrayAccessExpression.Target.Parent = arrayAccessExpression;
            arrayAccessExpression.Index.Parent = arrayAccessExpression;

            return arrayAccessExpression;
        }

        /// <summary>
        /// Parse {expression}.{expression}
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression ParseMemberAccess(Expression expression)
        {
            if (!Check(TokenType.Identifier))
            {
                throw new Exception($"Unexpected token {_lexer.Current}");
            }

            var arrayAccessExpression = new MemberAccessExpression
            {
                Target = expression,
                Member = ParseFunctionCallAndVariableExpression()
            };

            _lexer.Next();

            arrayAccessExpression.Target.Parent = arrayAccessExpression;
            arrayAccessExpression.Member.Parent = arrayAccessExpression;

            return arrayAccessExpression;
        }

        /// <summary>
        /// Parse {expression}++ and {expression}--
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression ParsePostfixUnaryExpression(Expression expression)
        {
            var token = _lexer.Current;

            _lexer.Next();

            var unary = new UnaryExpression()
            {
                Expression = expression,
                Operator = token.Type == TokenType.Increment ? UnaryOperator.IncrementSuffix : UnaryOperator.DecrementSuffix
            };

            expression.Parent = unary;

            return unary;
        }

        private bool CheckAndConsume(TokenType type)
        {
            if (_lexer.Current.Type != type) return false;

            _lexer.Next();

            return true;
        }

        private bool Check(TokenType type) => _lexer.Current.Type == type;

        private void Expect(TokenType type)
        {
            if (_lexer.Current.Type != type)
            {
                throw new Exception($"Expected token type: {type}");
            }

            _lexer.Next();
        }
    }
}