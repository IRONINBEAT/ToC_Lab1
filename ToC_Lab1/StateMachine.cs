using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public class StateMachine
    {
        private string _currentState;
        private readonly List<string> _validSequences = new List<string>();
        private List<string> _currentSequence = new List<string>();

        public StateMachine()
        {
            _currentState = "S0";
        }

        public List<string> Process(string input)
        {
            _validSequences.Clear();
            _currentSequence.Clear();
            _currentState = "S0";

            foreach (char symbol in input)
            {
                Transition(symbol);

                // Если достигли конечного состояния, сохраняем цепочку
                if (_currentState == "S6" || _currentState == "S12")
                {
                    _validSequences.Add(string.Join(" -> ", _currentSequence));
                    _currentSequence.Clear();
                    _currentState = "S0"; // Сбрасываем для поиска следующего ФИО
                }

                // Если состояние ошибки, сбрасываем автомат и начинаем заново
                if (_currentState == "SE")
                {
                    _currentSequence.Clear();
                    _currentState = "S0";
                }
            }

            return _validSequences;
        }

        private void Transition(char symbol)
        {
            _currentSequence.Add($"{_currentState} ({symbol})");

            switch (_currentState)
            {
                case "S0":
                    if (IsRussianUpper(symbol)) _currentState = "S1_temp";
                    else if (symbol == ' ') _currentState = "S0";
                    else _currentState = "SE";
                    break;

                case "S1_temp":
                    if (symbol == '.') _currentState = "S7";
                    else if (IsRussianLower(symbol) || symbol == '-') _currentState = "S1";
                    else if (symbol == ' ') _currentState = "S1_temp";
                    else _currentState = "SE";
                    break;

                case "S1":
                    if (IsRussianLower(symbol)) _currentState = "S1";
                    else if (symbol == '-') _currentState = "S1A";
                    else if (symbol == ' ') _currentState = "S2";
                    else _currentState = "SE";
                    break;

                case "S1A":
                    if (IsRussianUpper(symbol)) _currentState = "S1";
                    else _currentState = "SE";
                    break;

                case "S2":
                    if (symbol == ' ') _currentState = "S2";
                    else if (IsRussianUpper(symbol)) _currentState = "S3";
                    else _currentState = "SE";
                    break;

                case "S3":
                    if (symbol == '.') _currentState = "S4";
                    else _currentState = "SE";
                    break;

                case "S4":
                    if (symbol == ' ') _currentState = "S4";
                    else if (IsRussianUpper(symbol)) _currentState = "S5";
                    else _currentState = "SE";
                    break;

                case "S5":
                    if (symbol == '.') _currentState = "S6";
                    else _currentState = "SE";
                    break;

                case "S6":
                    _currentState = "S0";
                    break;

                case "S7":
                    if (IsRussianUpper(symbol)) _currentState = "S9";
                    else if (symbol == ' ') _currentState = "S8";
                    else _currentState = "SE";
                    break;

                case "S8":
                    if (symbol == ' ') _currentState = "S8";
                    else if (IsRussianUpper(symbol)) _currentState = "S9";
                    else _currentState = "SE";
                    break;

                case "S9":
                    if (symbol == '.') _currentState = "S10";
                    else _currentState = "SE";
                    break;

                case "S10":
                    if (IsRussianUpper(symbol)) _currentState = "S11";
                    else if (symbol == ' ') _currentState = "S10";
                    else _currentState = "SE";
                    break;

                case "S11":
                    if (IsRussianLower(symbol)) _currentState = "S11";
                    else if (symbol == '-') _currentState = "S11A";
                    else if (symbol == ' ' || symbol == '\0') _currentState = "S12";
                    else _currentState = "SE";
                    break;

                case "S11A":
                    if (IsRussianUpper(symbol)) _currentState = "S11";
                    else _currentState = "SE";
                    break;

                case "S12":
                    _currentState = "S0";
                    break;

                case "SE":
                    _currentState = "SE";
                    break;
            }
        }

        private bool IsRussianUpper(char c)
        {
            return "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".Contains(c);
        }

        private bool IsRussianLower(char c)
        {
            return "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".Contains(c);
        }
    }
}
