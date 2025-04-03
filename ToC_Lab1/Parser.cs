using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public class Parser
    {
        private List<Token> tokens;
        private int index;
        private List<string> errors = new();
        private List<string> correctedTokens = new();

        public string Parse(string input)
        {
            tokens = Lexer.Tokenize(input);
            index = 0;
            correctedTokens.Clear();
            errors.Clear();

            while (index < tokens.Count)
            {
                ParseRepeat();
            }

            return string.Join(" ", correctedTokens);
        }

        public string GetErrors() => string.Join("\n", errors);

        private void ParseRepeat()
        {
            if (!Match(TokenType.Repeat))
            {
                errors.Add($"Ошибка: Ожидалось 'repeat' на позиции {index}. Пропускаем токен: {tokens[index].Value}");
                index++;  // Принудительно двигаем индекс
                return;
            }

            correctedTokens.Add("repeat");

            if (!Match(TokenType.Number, out Token numToken))
            {
                errors.Add($"Ошибка: Ожидалось число после 'repeat'. Подставлено 1.");
                correctedTokens.Add("1");
            }
            else
            {
                correctedTokens.Add(numToken.Value);
            }

            if (!Match(TokenType.LeftBracket))
            {
                errors.Add($"Ошибка: Ожидалось '['. Автоматически добавлено.");
                correctedTokens.Add("[");
            }
            else
            {
                correctedTokens.Add("[");
            }

            while (index < tokens.Count && tokens[index].Type != TokenType.RightBracket)
            {
                int previousIndex = index; // Запоминаем позицию перед вызовом ParseCommand()
                ParseCommand();

                // Защита от зависания: если ParseCommand() не продвинулось, форсируем продвижение
                if (index == previousIndex)
                {
                    errors.Add($"Ошибка: Неизвестный токен '{tokens[index].Value}' на позиции {index}. Пропускаем.");
                    index++;
                }
            }

            if (!Match(TokenType.RightBracket))
            {
                errors.Add("Ошибка: Отсутствует ']'. Автоматически добавлено.");
                correctedTokens.Add("]");
            }
            else
            {
                correctedTokens.Add("]");
            }
        }


        private void ParseCommand()
        {
            if (!Match(TokenType.Command, out Token cmdToken))
            {
                errors.Add($"Ошибка: Ожидалась команда на позиции {index}");
                return;
            }

            correctedTokens.Add(cmdToken.Value);

            if (!Match(TokenType.Number, out Token numToken))
            {
                errors.Add($"Ошибка: Ожидалось число после команды '{cmdToken.Value}'. Подставлено 0.");
                correctedTokens.Add("0");
            }
            else
            {
                correctedTokens.Add(numToken.Value);
            }
        }

        private bool Match(TokenType type, out Token token)
        {
            if (index < tokens.Count && tokens[index].Type == type)
            {
                token = tokens[index++];
                return true;
            }

            token = null;
            return false;
        }

        private bool Match(TokenType type)
        {
            return Match(type, out _);
        }
    }
}
