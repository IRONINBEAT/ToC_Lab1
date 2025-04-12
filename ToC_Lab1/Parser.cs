using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{

    //public class Parser
    //{
    //    private readonly List<Token> tokens;
    //    private int position = 0;
    //    private List<string> errors = new List<string>();

    //    public Parser(List<Token> tokens)
    //    {
    //        this.tokens = tokens;
    //    }

    //    private Token Current => position < tokens.Count ? tokens[position] : null;

    //    private bool Match(TokenType type)
    //    {
    //        if (Current != null && Current.Type == type)
    //        {
    //            position++;
    //            return true;
    //        }
    //        return false;
    //    }

    //    private Token Expect(TokenType type)
    //    {
    //        if (Current != null && Current.Type == type)
    //        {
    //            return tokens[position++];
    //        }
    //        else
    //        {
    //            // Ошибка нейтрализации: ошибка в токене, добавляем исправленный токен
    //            var errorMsg = $"Ожидался '{type}', найдено '{Current?.Type}'";
    //            errors.Add($"Ошибка на строке {Current?.Line}, колонке {Current?.Column}: {errorMsg}. Пропускаем токен.");

    //            // Попробуем продолжить разбор, подставив ожидаемый токен
    //            return new Token(type, type.ToString(), Current?.GlobalPosition ?? 0, Current?.Line ?? 0, Current?.Column ?? 0);
    //        }
    //    }

    //    public void ParseProgram()
    //    {
    //        while (Current != null)
    //        {
    //            try
    //            {
    //                ParseCommand();
    //            }
    //            catch
    //            {
    //                // В случае ошибки просто пропускаем токен
    //                position++;
    //            }
    //        }
    //    }

    //    private void ParseCommand()
    //    {
    //        if (Current == null)
    //            return;

    //        switch (Current.Type)
    //        {
    //            case TokenType.Repeat:
    //                ParseRepeatCommand();
    //                break;

    //            case TokenType.Forward:
    //            case TokenType.Back:
    //            case TokenType.Left:
    //            case TokenType.Right:
    //                ParseSimpleCommand();
    //                break;

    //            default:
    //                // Ошибка нейтрализации: если команда неизвестна, пропускаем токен
    //                var errorMsg = $"Ожидалась команда (repeat, forward, back, left, right), найдено '{Current.Type}'";
    //                errors.Add($"Ошибка на строке {Current?.Line}, колонке {Current?.Column}: {errorMsg}. Пропускаем токен.");
    //                position++;
    //                break;
    //        }
    //    }

    //    private void ParseSimpleCommand()
    //    {
    //        var cmdToken = Expect(Current.Type); // forward, back, etc.
    //        Expect(TokenType.Number);  // после команды должно идти число
    //    }

    //    private void ParseRepeatCommand()
    //    {
    //        Expect(TokenType.Repeat);
    //        Expect(TokenType.Number); // после repeat обязательно число
    //        Expect(TokenType.OpenBracket);

    //        // хотя бы одна команда внутри
    //        bool atLeastOne = false;
    //        while (Current != null &&
    //              (Current.Type == TokenType.Repeat || IsSimpleCommand(Current.Type)))
    //        {
    //            ParseCommand();
    //            atLeastOne = true;
    //        }

    //        if (!atLeastOne)
    //        {
    //            var errorMsg = "repeat должен содержать хотя бы одну команду.";
    //            errors.Add($"Ошибка на строке {Current?.Line}, колонке {Current?.Column}: {errorMsg}. Добавлена команда forward 0.");
    //            // Добавляем команду по умолчанию для исправления
    //            position++;  // пропускаем текущий токен
    //        }

    //        Expect(TokenType.CloseBracket);
    //    }

    //    private bool IsSimpleCommand(TokenType type)
    //    {
    //        return type == TokenType.Forward || type == TokenType.Back ||
    //               type == TokenType.Left || type == TokenType.Right;
    //    }

    //    // Возвращаем ошибки как список строк
    //    public List<string> GetErrors()
    //    {
    //        return errors;
    //    }
    //}





    //public class Parser
    //{

    //    private readonly List<Token> tokens;
    //    private int position = 0;

    //    public List<string> Warnings { get; } = new();

    //    public Parser(List<Token> tokens)
    //    {
    //        this.tokens = tokens;
    //    }

    //    private Token Current => position < tokens.Count ? tokens[position] : null;

    //    private void Advance() => position++;

    //    private bool Match(TokenType type)
    //    {
    //        if (Current != null && Current.Type == type)
    //        {
    //            Advance();
    //            return true;
    //        }
    //        return false;
    //    }

    //    private Token Expect(TokenType type, string message, Token fallback = null)
    //    {
    //        if (Match(type))
    //        {
    //            return tokens[position - 1];
    //        }

    //        Warnings.Add(message);

    //        return fallback ?? new Token(type, GetDefaultValue(type), Current?.GlobalPosition ?? 0, Current?.Line ?? 0, Current?.Column ?? 0);
    //    }

    //    private string GetDefaultValue(TokenType type)
    //    {
    //        return type switch
    //        {
    //            TokenType.Number => "1",
    //            TokenType.OpenBracket => "[",
    //            TokenType.CloseBracket => "]",
    //            _ => ""
    //        };
    //    }

    //    public void Parse()
    //    {
    //        while (Current != null)
    //        {
    //            ParseCommand();
    //        }
    //    }

    //    private string ParseCommand()
    //    {
    //        if (Match(TokenType.Repeat))
    //        {
    //            ParseRepeat(); // Можно вернуть "repeat", если хочешь
    //            return "repeat";
    //        }
    //        else if (Match(TokenType.Forward) || Match(TokenType.Back) || Match(TokenType.Right) || Match(TokenType.Left))
    //        {
    //            var cmd = tokens[position - 1];
    //            var value = Expect(TokenType.Number, $"Ожидалось число после {cmd.Type}. Подставлено значение 0.",
    //                new Token(TokenType.Number, "0", cmd.GlobalPosition, cmd.Line, cmd.Column));
    //            return $"{cmd.Value} {value.Value}";
    //        }
    //        else
    //        {
    //            Warnings.Add($"Неизвестная команда: {Current?.Value}. Пропущена.");
    //            Advance(); // Пропустить
    //            return null;
    //        }
    //    }


    //    private void ParseRepeat()
    //    {
    //        Token numberToken;

    //        // Если следующий токен не число, то считаем, что это арифметическое выражение
    //        if (Current != null && Current.Type == TokenType.Number)
    //        {
    //            // Если это число, продолжаем
    //            numberToken = tokens[position];
    //            Advance();
    //        }
    //        else
    //        {
    //            // Если после repeat идет выражение, парсим его
    //            Warnings.Add("Ошибка: после repeat ожидается число. Подставлено значение 1.");

    //            // Используем ParseExpression для вычисления результата арифметического выражения
    //            numberToken = new Token(TokenType.Number, "1", Current?.GlobalPosition ?? 0, Current?.Line ?? 0, Current?.Column ?? 0);
    //        }

    //        // Теперь проверяем, что после числа (или выражения) идет открывающая квадратная скобка
    //        var openBracket = Expect(TokenType.OpenBracket, "Ошибка: отсутствует [. Автоматически добавлена.");

    //        var body = ParseCommandSequence();

    //        // Проверяем, есть ли закрывающая квадратная скобка
    //        var closeBracket = Match(TokenType.CloseBracket);
    //        if (!closeBracket)
    //        {
    //            Warnings.Add("Ошибка: отсутствует ]. Автоматически добавлена.");
    //        }

    //        // Если нет команд в блоке, добавляем пустую команду по умолчанию
    //        if (body.Count == 0)
    //        {
    //            Warnings.Add("Ошибка: пустой блок команд. Добавлена команда forward 0.");
    //            body.Add("forward 0");
    //        }

    //        // Тут можно использовать numberToken.Value для повторений
    //    }






    //    private List<string> ParseCommandSequence()
    //    {
    //        var commands = new List<string>();

    //        while (Current != null && Current.Type != TokenType.CloseBracket)
    //        {
    //            var cmd = ParseCommand();
    //            if (!string.IsNullOrEmpty(cmd))
    //            {
    //                commands.Add(cmd);
    //            }
    //        }

    //        return commands;
    //    }
    //}



    //public class Parser
    //{
    //    private List<Token> tokens;
    //    private int index;
    //    private List<string> errors = new();
    //    private List<string> correctedTokens = new();

    //    public string Parse(string input)
    //    {
    //        tokens = Lexer.Tokenize(input);
    //        index = 0;
    //        correctedTokens.Clear();
    //        errors.Clear();

    //        while (index < tokens.Count)
    //        {
    //            ParseRepeat();
    //        }

    //        return string.Join(" ", correctedTokens);
    //    }

    //    public string GetErrors() => string.Join("\n", errors);

    //    private void ParseRepeat()
    //    {
    //        if (!Match(TokenType.Repeat))
    //        {
    //            errors.Add($"Ошибка: Ожидалось 'repeat' на позиции {index}. Пропускаем токен: {tokens[index].Value}");
    //            index++;  // Принудительно двигаем индекс
    //            return;
    //        }

    //        correctedTokens.Add("repeat");

    //        if (!Match(TokenType.Number, out Token numToken))
    //        {
    //            errors.Add($"Ошибка: Ожидалось число после 'repeat'. Подставлено 1.");
    //            correctedTokens.Add("1");
    //        }
    //        else
    //        {
    //            correctedTokens.Add(numToken.Value);
    //        }

    //        if (!Match(TokenType.LeftBracket))
    //        {
    //            errors.Add($"Ошибка: Ожидалось '['. Автоматически добавлено.");
    //            correctedTokens.Add("[");
    //        }
    //        else
    //        {
    //            correctedTokens.Add("[");
    //        }

    //        while (index < tokens.Count && tokens[index].Type != TokenType.RightBracket)
    //        {
    //            int previousIndex = index; // Запоминаем позицию перед вызовом ParseCommand()
    //            ParseCommand();

    //            // Защита от зависания: если ParseCommand() не продвинулось, форсируем продвижение
    //            if (index == previousIndex)
    //            {
    //                errors.Add($"Ошибка: Неизвестный токен '{tokens[index].Value}' на позиции {index}. Пропускаем.");
    //                index++;
    //            }
    //        }

    //        if (!Match(TokenType.RightBracket))
    //        {
    //            errors.Add("Ошибка: Отсутствует ']'. Автоматически добавлено.");
    //            correctedTokens.Add("]");
    //        }
    //        else
    //        {
    //            correctedTokens.Add("]");
    //        }
    //    }


    //    private void ParseCommand()
    //    {
    //        if (!Match(TokenType.Command, out Token cmdToken))
    //        {
    //            errors.Add($"Ошибка: Ожидалась команда на позиции {index}");
    //            return;
    //        }

    //        correctedTokens.Add(cmdToken.Value);

    //        if (!Match(TokenType.Number, out Token numToken))
    //        {
    //            errors.Add($"Ошибка: Ожидалось число после команды '{cmdToken.Value}'. Подставлено 0.");
    //            correctedTokens.Add("0");
    //        }
    //        else
    //        {
    //            correctedTokens.Add(numToken.Value);
    //        }
    //    }

    //    private bool Match(TokenType type, out Token token)
    //    {
    //        if (index < tokens.Count && tokens[index].Type == type)
    //        {
    //            token = tokens[index++];
    //            return true;
    //        }

    //        token = null;
    //        return false;
    //    }

    //    private bool Match(TokenType type)
    //    {
    //        return Match(type, out _);
    //    }
    //}
}
