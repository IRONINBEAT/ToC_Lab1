using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public enum TokenType
    {
        repeat,
        forward,
        back,
        right,
        left,

        Number,
        OpenBracket,    // [
        CloseBracket,   // ]

        UnknownWord,

        Invalid,
        Whitespace
    }
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int GlobalPosition { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int globalPosition, int line, int column)
        {
            Type = type;
            Value = value;
            GlobalPosition = globalPosition;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Type}: '{Value}' at Line {Line}, Column {Column}";
        }
    }

    /*--------------------------Для лабы--------------------------*/
    //public class Token
    //{
    //    public string Type { get; }
    //    public string Value { get; }
    //    public int Line { get; }
    //    public int Column { get; }

    //    public Token(string type, string value, int line, int column)
    //    {
    //        Type = type;
    //        Value = value;
    //        Line = line;
    //        Column = column;
    //    }

    //    public override string ToString()
    //    {
    //        return $"[Строка {Line}, Позиция {Column}] {Type}: {Value}";
    //    }
    //}



}
