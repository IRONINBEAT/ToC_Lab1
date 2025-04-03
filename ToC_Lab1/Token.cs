using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public enum TokenType { Repeat, Command, Number, LeftBracket, RightBracket, Operator, Unknown }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }


}
