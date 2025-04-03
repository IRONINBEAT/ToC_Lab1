using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public class Lexer
    {
        private static readonly Dictionary<string, TokenType> keywords = new()
        {
            { "repeat", TokenType.Repeat },
            { "forward", TokenType.Command },
            { "right", TokenType.Command },
            { "left", TokenType.Command },
            { "back", TokenType.Command }
        };

        private static readonly Regex numberPattern = new(@"^\d+$");

        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var words = Regex.Split(input, @"\s+|\b");

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                if (keywords.ContainsKey(word)) tokens.Add(new Token(keywords[word], word));
                else if (numberPattern.IsMatch(word)) tokens.Add(new Token(TokenType.Number, word));
                else if (word == "[") tokens.Add(new Token(TokenType.LeftBracket, word));
                else if (word == "]") tokens.Add(new Token(TokenType.RightBracket, word));
                else if ("+-*/".Contains(word)) tokens.Add(new Token(TokenType.Operator, word));
                else tokens.Add(new Token(TokenType.Unknown, word));
            }

            return tokens;
        }
    }

}
