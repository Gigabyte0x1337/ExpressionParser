using System.Collections.Generic;
using System.Linq;

namespace ExpressionParser
{
    public class Lexer
    {
        private readonly CharReader _charReader;

        private readonly Dictionary<string, TokenType> _basicTokenType = new Dictionary<string, TokenType>(){
            { ">", TokenType.GreaterThan },
            { "<", TokenType.LessThan },
            { "+", TokenType.Plus },
            { "-", TokenType.Minus },
            { "*", TokenType.Multiply },
            { "/", TokenType.Divide },
            { "!", TokenType.Not },
            { "." , TokenType.Dot },
            { "," , TokenType.Comma },
            { "(" , TokenType.OpenParenthesis },
            { ")" , TokenType.CloseParenthesis },
            { "[" , TokenType.OpenBracket },
            { "]" , TokenType.ClosedBracket },
            { "^", TokenType.Exponent },
            { "=", TokenType.Assign },
            { ":", TokenType.Colon },
            { "?", TokenType.QuestionMark },
            { "&&" , TokenType.And },
            { "++" , TokenType.Increment },
            { "--" , TokenType.Decrement },
            { "||" , TokenType.Or },
            { "==" , TokenType.Equal } ,
            { "!=", TokenType.NotEqual },
            { ">=", TokenType.GreaterThanOrEqual },
            { "+=", TokenType.AddAssign },
            { "-=", TokenType.SubtractAssign },
            { "/=", TokenType.DivideAssign },
            { "*=", TokenType.MultiplyAssign },
            { "%=", TokenType.ModuloAssign },
            { "^=", TokenType.ExponentAssign },
        };

        public Token Current { get; set; }

        public Lexer(string text)
        {
            _charReader = new CharReader(text);

            Next();
        }

        public void Next()
        {
            if (_charReader.Current == 0)
            {
                Current = new Token()
                {
                    Type = TokenType.Eof
                };

                return;
            }

            LexWhiteSpaces();

            Current = LexBasicToken();

            // .0 is numeric value else its just a dot token
            if (Current != null && Current.Type == TokenType.Dot && char.IsDigit(_charReader.Current))
            {
                Current = LexDecimalNumber(_charReader.Position - 1);
            }
            else if(Current != null)
            {
                return;
            }
            else if (char.IsDigit(_charReader.Current)) Current = LexNumber();
            else if (char.IsLetter(_charReader.Current)) Current = LexIdentifier();
            else if (_charReader.Current == '"') Current = LexStringLiteral();
        }

        public void LexWhiteSpaces()
        {
            while (_charReader.Current == ' ')
            {
                _charReader.Next();
            }
        }

        public Token LexStringLiteral()
        {
            var stringValue = string.Empty;

            _charReader.Next();

            while (_charReader.Current != '"')
            {
                if (_charReader.Current == '/' || _charReader.Current == 92)
                {
                    _charReader.Next();
                }

                stringValue += _charReader.Current;

                _charReader.Next();
            }

            _charReader.Next();

            return new Token()
            {
                Type = TokenType.StringLiteral,
                Value = stringValue
            };
        }

        public Token LexIdentifier()
        {
            var startPosition = _charReader.Position;

            while (char.IsLetterOrDigit(_charReader.Current) || _charReader.Current == '_')
            {
                _charReader.Next();
            }

            var name = _charReader.GetRange(startPosition, _charReader.Position);

            var token = new Token()
            {
                Type = TokenType.Identifier,
                Value = name
            };
           
            var keywordLookup = new Dictionary<string, TokenType>()
            {
                { "if", TokenType.KeywordIf },
                { "for", TokenType.KeywordFor },
                { "false", TokenType.KeywordFalse },
                { "true", TokenType.KeywordTrue },
                { "var", TokenType.KeywordVar }
            };

            if (keywordLookup.ContainsKey(name))
            {
                token.Type = keywordLookup[name];
            }

            return token;
        }

        public Token LexNumber()
        {
            var startPosition = _charReader.Position;

            while (char.IsDigit(_charReader.Current))
            {
                _charReader.Next();
            }

            if (_charReader.Current != '.')
            {
                return new Token()
                {
                    Type = TokenType.Int,
                    Value = _charReader.GetRange(startPosition, _charReader.Position)
                };

            }

            _charReader.Next();

            return LexDecimalNumber(startPosition);
        }

        public Token LexDecimalNumber(int startPosition)
        {
            while (char.IsDigit(_charReader.Current))
            {
                _charReader.Next();
            }

            return new Token()
            {
                Type = TokenType.Decimal, // TODO check type suffixes
                Value = _charReader.GetRange(startPosition, _charReader.Position)
            };
        }

        public Token LexBasicToken()
        {
            var position = 0;
            var operators = _basicTokenType.Keys.ToArray();
            var operatorString = string.Empty;
            string currentMatch = null;

            while (true)
            {
                if (position != 0)
                {
                    _charReader.Next();
                }

                operatorString += _charReader.Current;
                var currentLength = position + 1;

                operators = operators
                    .Where(o => o.Substring(0, o.Length < currentLength ? o.Length : currentLength) == operatorString)
                    .ToArray();

                if (operators.Length == 0 && currentMatch != null)
                {
                    return new Token()
                    {
                        Type = _basicTokenType[currentMatch]
                    };
                }

                if (operators.Length == 0) return null;

                if (operators.Length == 1 && operators.First().Length == currentLength)
                {
                    _charReader.Next();

                    return new Token()
                    {
                        Type = _basicTokenType[operators.First()]
                    };
                }

                if (operators.Length > 1)
                {
                    currentMatch = operators.First();
                }

                position++;
            }
        }
    }
}
