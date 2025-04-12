using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace ToC_Lab1
{
    public class StateMachine
    {
        private string _currentState;
        private readonly List<string> _validSequences = new List<string>();
        private List<string> _currentSequence = new List<string>();
        private List<char> _hasSurnameChar = new List<char>();
        private List<string> _currentSurname = new List<string>();
        private List<string> _validSurname = new List<string>();

        public string ValidSurname { 
            get 
            {
                var fullSequence = string.Join("-> ", _validSurname);
                return fullSequence;
            } 
        }



        public StateMachine()
        {
            _currentState = "S0";
        }

        public List<string> Process(string input)
        {
            _validSequences.Clear();
            _validSurname.Clear();
            _currentSequence.Clear();
            _currentState = "S0";

            foreach (char symbol in input)
            {
                Transition(symbol);

                // Если достигли конечного состояния, сохраняем цепочку
                if (_currentState == "S6" || _currentState == "S12")
                {
                    // Добавляем конечное состояние
                    _currentSequence.Add($"{_currentState}");


                    // Формируем полную последовательность
                    var fullSequence = string.Join("-> ", _currentSequence);


                    _validSequences.Add(fullSequence);
                    _currentState = "S0"; // Сбрасываем для поиска следующего ФИО

                    _validSurname.Add($"{_currentSurname}");
                }

                // Если состояние ошибки, сбрасываем автомат и начинаем заново
                if (_currentState == "SE")
                {
                    _currentSequence.Add($"{_currentState} ({symbol})");
                    _currentSurname.Clear();
                    _hasSurnameChar.Clear();
                    _currentState = "S0";
                }
            }

            return _validSequences;
        }

        private void Transition(char symbol)
        {
            _currentSequence.Add($"{_currentState} ({symbol})");
            _currentSurname.Add($"{symbol}");
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
                    else if (symbol == ' ') _currentState = "SE";
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
                    if (IsRussianUpper(symbol))
                    {
                        _currentState = "S11";
                        _hasSurnameChar.Add(symbol);
                    }
                    else if (symbol == ' ') _currentState = "S10";
                    else _currentState = "SE";
                    break;

                //case "S11":
                //if (IsRussianLower(symbol))
                //{
                //    _currentState = "S11";
                //    // Запоминаем, что был хотя бы один символ фамилии
                //    _hasSurnameChar = true;
                //}
                //else if (symbol == '-')
                //{
                //    if (!_hasSurnameChar) // Если до дефиса не было символов
                //        _currentState = "SE";
                //    else
                //        _currentState = "S11A";
                //}
                //else if (IsRussianUpper(symbol))
                //{
                //    _currentState = "SE"; // Заглавные буквы в середине фамилии недопустимы
                //}
                //else
                //{
                //    // Переход в конечное состояние только если был хотя бы один символ фамилии
                //    if (_hasSurnameChar)
                //        _currentState = "S12";
                //    else
                //        _currentState = "SE";
                //}
                //break;
                case "S11":

                    if (IsRussianLower(symbol))
                    {
                        _currentState = "S11";
                        _hasSurnameChar.Clear();
                    }
                    else if (symbol == '-') _currentState = "S11A";
                    else if (IsRussianUpper(symbol))
                    {
                        _currentState = "SE";
                    }//если сломается - убрать
                    else if ((symbol == ' ' || symbol == '\0') && _hasSurnameChar.Count == 1)
                    {
                        _currentState = "SE";
                    }
                    else _currentState = "S12";
                    break;

                case "S11A":
                    if (IsRussianUpper(symbol)) _currentState = "S11";
                    else _currentState = "SE";
                    break;

                case "S12":
                    _currentState = "S0";
                    _hasSurnameChar.Clear();
                    break;

                case "SE":
                    _currentState = "SE";
                    _hasSurnameChar.Clear();
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
