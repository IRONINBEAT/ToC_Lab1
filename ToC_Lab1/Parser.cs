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
        private int position;
        public List<string> Errors { get; } = new();

        public ASTNode Parse(List<Token> tokenList)
        {
            tokens = tokenList;
            position = 0;

            // Теперь передаем hadAnyCommands
            bool hadCommands;
            var body = ParseCommandSequence(out hadCommands);

            if (Errors.Count == 0 && position < tokens.Count)
                Errors.Add($"Лишние токены после конца программы, начиная с: {Peek()}");

            // Если в блоке команд есть хотя бы одна команда (несмотря на ошибки), не считаем его пустым
            if (!hadCommands)
            {
                Errors.Add($"Пустой блок команд в repeat на строке {Previous().Line}, столбце {Previous().Column}");
            }

            return body.Count == 1 ? body[0] : new RepeatNode("1", body); // Если несколько команд, оборачиваем их в repeat
        }

      

        private List<ASTNode> ParseCommandSequence(out bool hadAnyCommands)
        {
            var commands = new List<ASTNode>();
            hadAnyCommands = false;

            while (!IsAtEnd() && !Check(TokenType.CloseBracket))
            {
                var command = ParseCommand();
                hadAnyCommands = true; // ← Даже если с ошибкой, это попытка команды

                if (command != null)
                    commands.Add(command);
            }

            return commands;
        }




        private ASTNode? ParseCommand()
        {
            var token = Peek();

            if (Match(TokenType.forward, TokenType.back, TokenType.left, TokenType.right))
            {
                Token command = Previous();
                if (Match(TokenType.Number))
                {
                    return new CommandNode(command.Type, Previous().Value);
                }
                else
                {
                    Errors.Add($"Ожидалось число после {command.Type} на строке {command.Line}, столбце {command.Column}");
                    // Пытаемся продолжить дальше, даже если ошибка
                    return null;
                }
            }
            else if (Match(TokenType.repeat) || Check(TokenType.UnknownWord))
            {
                Token repeatToken;
                bool isMisspelledRepeat = false;

                // Определяем, это прямое repeat или опечатка
                if (Previous().Type == TokenType.repeat)
                {
                    repeatToken = Previous();
                }
                else
                {
                    repeatToken = Peek();
                    var expected = GetExpectedKeyword(repeatToken.Value);
                    isMisspelledRepeat = expected == "repeat";

                    if (!isMisspelledRepeat)
                    {
                        // Это не похоже на repeat, обрабатываем как другое ключевое слово
                        var wrong = Advance();
                        expected = GetExpectedKeyword(wrong.Value);

                        if (expected != null)
                        {
                            Errors.Add($"Неизвестное или неверно написанное ключевое слово '{wrong.Value}'. Ожидалось \"{expected}\" на строке {wrong.Line}, столбце {wrong.Column}");

                            // Для команд (forward/back/left/right) проглатываем число если есть
                            if (expected == "forward" || expected == "back" || expected == "left" || expected == "right")
                            {
                                if (Check(TokenType.Number))
                                    Advance();
                            }
                        }
                        else
                        {
                            Errors.Add($"Неизвестное ключевое слово '{wrong.Value}' на строке {wrong.Line}, столбце {wrong.Column}");

                            // Если после - число, проглотим его
                            if (Check(TokenType.Number))
                                Advance();
                        }

                        return null;
                    }

                    // Это опечатка repeat, но мы будем обрабатывать как repeat
                    Errors.Add($"Неверное написание 'repeat' ('{repeatToken.Value}') на строке {repeatToken.Line}, столбце {repeatToken.Column}");
                    Advance(); // Пропускаем неправильное слово
                }

                // Далее идет обработка repeat (как в оригинальном коде)
                string count = "1";
                bool hasNumber = false;

                // Обрабатываем число
                while (!IsAtEnd() && !Check(TokenType.OpenBracket))
                {
                    if (Check(TokenType.Number))
                    {
                        count = Advance().Value;
                        hasNumber = true;
                        break;
                    }
                    else
                    {
                        var unexpected = Advance();
                        if (!unexpected.Value.All(char.IsWhiteSpace))
                        {
                            Errors.Add($"Ожидалось число, но получено '{unexpected.Value}' на строке {unexpected.Line}, столбце {unexpected.Column}");
                        }
                    }
                }

                if (!hasNumber)
                {
                    Errors.Add($"Ожидалось число после repeat на строке {repeatToken.Line}, столбце {repeatToken.Column + repeatToken.Value.Length}");
                }

                // Ожидаем [
                if (!Match(TokenType.OpenBracket))
                {
                    Errors.Add($"Ожидалась [ после repeat {count} на строке {Peek().Line}, столбце {Peek().Column}");
                    return null;
                }

                bool hadCommands;
                var body = ParseCommandSequence(out hadCommands);

                if (!hadCommands)
                {
                    Errors.Add($"Пустой блок команд в repeat на строке {repeatToken.Line}, столбце {repeatToken.Column}");
                }

                if (!Match(TokenType.CloseBracket))
                {
                    Errors.Add($"Ожидалась ] в конце блока repeat на строке {Peek().Line}, столбце {Peek().Column}");
                    return null;
                }

                return new RepeatNode(count, body);
            }
            //else if (Match(TokenType.repeat) || Check(TokenType.UnknownWord))
            //{
            //    Token repeatToken;
            //    bool isMisspelledRepeat = false;

            //    // Определяем, это прямое repeat или опечатка
            //    if (Previous().Type == TokenType.repeat)
            //    {
            //        repeatToken = Previous();
            //    }
            //    else
            //    {
            //        repeatToken = Peek();
            //        var expected = GetExpectedKeyword(repeatToken.Value);
            //        isMisspelledRepeat = expected == "repeat";

            //        if (!isMisspelledRepeat)
            //        {
            //            Errors.Add($"Неизвестное ключевое слово '{repeatToken.Value}' на строке {repeatToken.Line}, столбце {repeatToken.Column}");
            //            Advance();
            //            return null;
            //        }

            //        // Это опечатка repeat, но мы будем обрабатывать как repeat
            //        Errors.Add($"Неверное написание 'repeat' ('{repeatToken.Value}') на строке {repeatToken.Line}, столбце {repeatToken.Column}");
            //        Advance(); // Пропускаем неправильное слово
            //    }

            //    string count = "1";
            //    bool hasNumber = false;

            //    // Обрабатываем число (кавычки не пропускаем, считаем ошибкой)
            //    while (!IsAtEnd() && !Check(TokenType.OpenBracket))
            //    {
            //        if (Check(TokenType.Number))
            //        {
            //            count = Advance().Value;
            //            hasNumber = true;
            //            break;
            //        }
            //        else
            //        {
            //            var unexpected = Advance();
            //            if (!unexpected.Value.All(char.IsWhiteSpace))
            //            {
            //                Errors.Add($"Ожидалось число, но получено '{unexpected.Value}' на строке {unexpected.Line}, столбце {unexpected.Column}");
            //            }
            //        }
            //    }

            //    if (!hasNumber)
            //    {
            //        Errors.Add($"Ожидалось число после repeat на строке {repeatToken.Line}, столбце {repeatToken.Column + repeatToken.Value.Length}");
            //    }

            //    // Ожидаем [
            //    while (!IsAtEnd() && !Check(TokenType.OpenBracket))
            //    {
            //        var unexpected = Advance();
            //        if (!unexpected.Value.All(char.IsWhiteSpace))
            //        {
            //            Errors.Add($"Ожидалась [ после repeat {count} на строке {unexpected.Line}, столбце {unexpected.Column}");
            //        }
            //    }

            //    if (!Match(TokenType.OpenBracket))
            //    {
            //        return null;
            //    }

            //    bool hadCommands;
            //    var body = ParseCommandSequence(out hadCommands);

            //    if (!hadCommands)
            //    {
            //        Errors.Add($"Пустой блок команд в repeat на строке {repeatToken.Line}, столбце {repeatToken.Column}");
            //    }

            //    if (!Match(TokenType.CloseBracket))
            //    {
            //        Errors.Add($"Ожидалась ] в конце блока repeat на строке {Peek().Line}, столбце {Peek().Column}");
            //        return null;
            //    }

            //    return new RepeatNode(count, body);
            //}
            //else if (Match(TokenType.repeat))
            //{
            //    string count = "1"; // значение по умолчанию
            //    bool hasNumber = false;

            //    // Если нет числа, то продолжаем искать [
            //    while (!IsAtEnd() && !Check(TokenType.OpenBracket))
            //    {
            //        var unexpectedToken = Peek();
            //        if (Check(TokenType.Number))
            //        {
            //            count = Advance().Value;
            //            hasNumber = true;
            //            break;
            //        }
            //        else if (Check(TokenType.UnknownWord))
            //        {
            //            var unknownWord = Advance();
            //            Errors.Add($"Неизвестное ключевое слово '{unknownWord.Value}' на строке {unknownWord.Line}, столбце {unknownWord.Column}");
            //            continue;
            //        }
            //        else
            //        {
            //            Errors.Add($"Неожиданный токен '{unexpectedToken.Value}' на строке {unexpectedToken.Line}, столбце {unexpectedToken.Column}");
            //            Advance(); // Пропускаем ошибку
            //            continue;
            //        }
            //    }

            //    if (!hasNumber)
            //    {
            //        Errors.Add($"Ожидалось число после repeat на строке {Previous().Line}, столбце {Previous().Column}");
            //    }

            //    // Теперь мы обязательно ожидаем [
            //    if (!Match(TokenType.OpenBracket))
            //    {
            //        Errors.Add($"Ожидалась [ после repeat {(hasNumber ? count : "(?)")}");
            //        return null;
            //    }

            //    bool hadCommands;
            //    var body = ParseCommandSequence(out hadCommands);

            //    if (!hadCommands)
            //    {
            //        Errors.Add($"Пустой блок команд в repeat {count} на строке {Previous().Line}, столбце {Previous().Column}");
            //    }

            //    if (!Match(TokenType.CloseBracket))
            //    {
            //        Errors.Add("Ожидалась ] в конце блока repeat");
            //        return null;
            //    }

            //    return new RepeatNode(count, body);
            //}
            //else if (Check(TokenType.UnknownWord))
            //{
            //    var wrong = Advance();

            //    var expected = GetExpectedKeyword(wrong.Value);
            //    // Пробуем распознать: это была попытка команды или repeat
            //    if (expected == "repeat")
            //    {
            //        Errors.Add($"Неизвестное или неверно написанное ключевое слово '{wrong.Value}'. Ожидалось \"repeat\" на строке {wrong.Line}, столбце {wrong.Column}");

            //        // Пытаемся съесть число (повторение)
            //        string count = "1";
            //        if (Check(TokenType.Number))
            //        {
            //            count = Advance().Value;
            //        }
            //        else
            //        {
            //            Errors.Add($"Ожидалось число после '{wrong.Value}' на строке {wrong.Line}, столбце {wrong.Column + wrong.Value.Length}");
            //        }

            //        // Пытаемся съесть [
            //        if (!Match(TokenType.OpenBracket))
            //        {
            //            Errors.Add($"Ожидалась [ после '{wrong.Value}' {count}");
            //            return null;
            //        }

            //        // Парсим тело, даже если repeat написан с ошибкой
            //        bool hadCommands;
            //        var body = ParseCommandSequence(out hadCommands);

            //        if (!hadCommands)
            //        {
            //            Errors.Add($"Пустой блок команд в '{wrong.Value}' {count} на строке {Previous().Line}, столбце {Previous().Column}");
            //        }

            //        if (!Match(TokenType.CloseBracket))
            //        {
            //            Errors.Add($"Ожидалась ] в конце блока '{wrong.Value}'");
            //            return null;
            //        }

            //        // Возвращаем RepeatNode, даже если repeat был написан с ошибкой
            //        return new RepeatNode(count, body);
            //    }
            //    else if (expected != null)
            //    {
            //        Errors.Add($"Неизвестное или неверно написанное ключевое слово '{wrong.Value}'. Ожидалось \"{expected}\" на строке {wrong.Line}, столбце {wrong.Column}");

            //        // Проглатываем число, если оно есть (например: forw5ard 10)
            //        if (Check(TokenType.Number))
            //            Advance();

            //        return null;
            //    }
            //    else
            //    {
            //        Errors.Add($"Неизвестное ключевое слово '{wrong.Value}' на строке {wrong.Line}, столбце {wrong.Column}");

            //        // Если после — число, проглотим его
            //        if (Check(TokenType.Number))
            //            Advance();

            //        return null;
            //    }
            //}
            else
            {
                Errors.Add($"Неожиданный токен {token.Value} на строке {token.Line}, столбце {token.Column}");
                Advance(); // Пропускаем ошибку
                return null;
            }
        }



        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) position++;
            return Previous();
        }

        private string? GetExpectedKeyword(string actual)
        {
            string[] keywords = { "repeat", "forward", "back", "left", "right" };
            int minDistance = int.MaxValue;
            string? closest = null;

            foreach (var keyword in keywords)
            {
                int dist = Levenshtein(actual.ToLower(), keyword);
                int maxAllowed = Math.Max(1, keyword.Length / 2);

                if (dist <= maxAllowed && dist < minDistance)
                {
                    minDistance = dist;
                    closest = keyword;
                }
            }

            return closest;
        }


        private bool IsAtEnd() => position >= tokens.Count;
        private Token Peek() => tokens[Math.Min(position, tokens.Count - 1)];
        private Token Previous() => tokens[Math.Max(position - 1, 0)];

        private int Levenshtein(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[s.Length, t.Length];
        }
    }



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





    //private ASTNode? ParseCommand()
    //{
    //    var token = Peek();

    //    if (Match(TokenType.Forward, TokenType.Back, TokenType.Left, TokenType.Right))
    //    {
    //        Token command = Previous();
    //        if (Match(TokenType.Number))
    //        {
    //            return new CommandNode(command.Type, Previous().Value);
    //        }
    //        else
    //        {
    //            Errors.Add($"Ожидалось число после {command.Type} на строке {command.Line}, столбце {command.Column}");
    //            return null;
    //        }
    //    }
    //    else if (Match(TokenType.Repeat))
    //    {
    //        if (!Match(TokenType.Number))
    //        {
    //            Errors.Add($"Ожидалось число после repeat на строке {Previous().Line}, столбце {Previous().Column}");
    //            return null;
    //        }

    //        string count = Previous().Value;

    //        if (!Match(TokenType.OpenBracket))
    //        {
    //            Errors.Add($"Ожидалась [ после repeat {count}");
    //            return null;
    //        }

    //        var body = ParseCommandSequence();

    //        if (!Match(TokenType.CloseBracket))
    //        {
    //            Errors.Add("Ожидалась ] в конце блока repeat");
    //            return null;
    //        }

    //        return new RepeatNode(count, body);
    //    }
    //    else
    //    {
    //        Errors.Add($"Неожиданный токен {token.Type} на строке {token.Line}, столбце {token.Column}");
    //        Advance(); // пропустим ошибку
    //        return null;
    //    }
    //}

    // Вспомогательные методы

    //private ASTNode? ParseCommand()
    //{
    //    var token = Peek();

    //    if (Match(TokenType.forward, TokenType.back, TokenType.left, TokenType.right))
    //    {
    //        Token command = Previous();
    //        if (Match(TokenType.Number))
    //        {
    //            return new CommandNode(command.Type, Previous().Value);
    //        }
    //        else
    //        {
    //            Errors.Add($"Ожидалось число после {command.Type} на строке {command.Line}, столбце {command.Column}");
    //            return null;
    //        }
    //    }
    //    else if (Match(TokenType.repeat))
    //    {
    //        if (!Match(TokenType.Number))
    //        {
    //            Errors.Add($"Ожидалось число после repeat на строке {Previous().Line}, столбце {Previous().Column}");
    //            return null;
    //        }

    //        string count = Previous().Value;

    //        if (!Match(TokenType.OpenBracket))
    //        {
    //            Errors.Add($"Ожидалась [ после repeat {count}");
    //            return null;
    //        }

    //        var body = ParseCommandSequence();

    //        if (body.Count == 0)
    //        {
    //            Errors.Add($"Пустой блок команд в repeat {count} на строке {Previous().Line}, столбце {Previous().Column}");
    //            return null;
    //        }

    //        if (!Match(TokenType.CloseBracket))
    //        {
    //            Errors.Add("Ожидалась ] в конце блока repeat");
    //            return null;
    //        }

    //        return new RepeatNode(count, body);
    //    }
    //    else
    //    {
    //        Errors.Add($"Неожиданный токен {token.Type} на строке {token.Line}, столбце {token.Column}");
    //        Advance(); // пропустим ошибку
    //        return null;
    //    }
    //}



    //public ASTNode Parse(List<Token> tokenList)
    //{
    //    tokens = tokenList;
    //    position = 0;
    //    var body = ParseCommandSequence();
    //    if (Errors.Count == 0 && position < tokens.Count)
    //        Errors.Add($"Лишние токены после конца программы, начиная с: {Peek()}");

    //    return body.Count == 1 ? body[0] : new RepeatNode("1", body); // обернем всё в repeat 1, если несколько команд
    //}


    //private List<ASTNode> ParseCommandSequence()
    //{
    //    var commands = new List<ASTNode>();

    //    while (!IsAtEnd() && !Check(TokenType.CloseBracket))
    //    {
    //        var command = ParseCommand();
    //        if (command != null)
    //            commands.Add(command);
    //        else
    //            break;
    //    }

    //    return commands;
    //}

}
