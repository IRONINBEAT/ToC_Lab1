using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    /*------------------------------Для курсача---------------------------*/
    public class Lexer
    {
        private static readonly Dictionary<TokenType, string> tokenPatterns = new()
{
    { TokenType.repeat,       @"\brepeat\b" },
    { TokenType.forward,      @"\bforward\b" },
    { TokenType.back,         @"\bback\b" },
    { TokenType.right,        @"\bright\b" },
    { TokenType.left,         @"\bleft\b" },

    { TokenType.Number,       @"\b\d+(\.\d+)?\b" },
    { TokenType.OpenBracket,  @"\[" },
    { TokenType.CloseBracket, @"\]" },

    // Добавим это:
    { TokenType.UnknownWord,  @"\b[a-zA-Z_][a-zA-Z0-9_]*\b" },

    { TokenType.Whitespace,   @"\s+" },
    { TokenType.Invalid,      @"." } // Последний — всё остальное
};

        private static readonly Regex combinedRegex;

        static Lexer()
        {
            string combined = string.Join("|",
                tokenPatterns.Select(kvp => $"(?<{kvp.Key}>{kvp.Value})"));
            combinedRegex = new Regex(combined, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();

            int line = 1;
            int column = 1;
            int globalIndex = 0;

            var matches = combinedRegex.Matches(input);

            foreach (Match match in matches)
            {
                TokenType matchedType = TokenType.Invalid;
                foreach (TokenType type in tokenPatterns.Keys)
                {
                    if (match.Groups[type.ToString()].Success)
                    {
                        matchedType = type;
                        break;
                    }
                }

                string value = match.Value;

                // Пропускаем пробелы
                if (matchedType != TokenType.Whitespace)
                {
                    tokens.Add(new Token(matchedType, value, match.Index, line, column));
                }

                // Обновляем позицию
                int newlines = value.Count(c => c == '\n');

                if (newlines == 0)
                {
                    column += value.Length;
                }
                else
                {
                    line += newlines;
                    int lastNewline = value.LastIndexOf('\n');
                    column = value.Length - lastNewline;
                }

                globalIndex += value.Length;
            }

            return tokens;
        }
    }




    /*-------------Для лабы-------------------------*/
    //class Lexer
    //{
    //    private static readonly Dictionary<string, string> tokenDefinitions = new()
    //{
    //    {"ЧИСЛО", "\\b\\d+(\\.\\d+)?\\b"},
    //    {"ИДЕНТИФИКАТОР", "\\b[a-zA-Z_][a-zA-Z0-9_]*\\b"},
    //    {"ОПЕРАТОР", "[+\\-*/=]"},
    //    {"ОТКРЫВАЮЩАЯ СКОБКА (КРУГЛАЯ)", "\\("},
    //    {"ЗАКРЫВАЮЩАЯ СКОБКА (КРУГЛАЯ)", "\\)"},
    //    {"ОТКРЫВАЮЩАЯ СКОБКА (ФИГУРНАЯ)", "\\{"},
    //    {"ЗАКРЫВАЮЩАЯ СКОБКА (ФИГУРНАЯ)", "\\}"},
    //    {"ОТКРЫВАЮЩАЯ СКОБКА (КВАДРАТНАЯ)", "\\["},
    //    {"ЗАКРЫВАЮЩАЯ СКОБКА (КВАДРАТНАЯ)", "\\]"},
    //    {"ПРОБЕЛ", "\\s+"},
    //    {"НЕИЗВЕСТНЫЙ", "."} // Ловит любой неизвестный символ
    //};

    //    public List<Token> Analyze(string input)
    //    {
    //        List<Token> tokens = new List<Token>();
    //        int position = 0;
    //        int line = 1;
    //        int column = 1;

    //        while (position < input.Length)
    //        {
    //            bool matched = false;

    //            foreach (var tokenDef in tokenDefinitions)
    //            {
    //                var regex = new Regex($"^{tokenDef.Value}");
    //                var match = regex.Match(input.Substring(position));

    //                if (match.Success)
    //                {
    //                    if (tokenDef.Key != "ПРОБЕЛ")
    //                    {
    //                        tokens.Add(new Token(tokenDef.Key, match.Value, line, column));
    //                    }

    //                    for (int i = 0; i < match.Length; i++)
    //                    {
    //                        if (input[position + i] == '\n')
    //                        {
    //                            line++;
    //                            column = 1;
    //                        }
    //                        else
    //                        {
    //                            column++;
    //                        }
    //                    }

    //                    position += match.Length;
    //                    matched = true;
    //                    break;
    //                }
    //            }

    //            if (!matched)
    //            {
    //                throw new Exception($"Неизвестный символ на позиции {position}: {input[position]}");
    //            }
    //        }

    //        return tokens;
    //    }
    //}



}
