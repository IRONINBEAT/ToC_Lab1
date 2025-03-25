using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public class FoundFIO
    {
        public string FIO { get; set; } // Найденная строка (ФИО)
        public int Position { get; set; } // Позиция в тексте
        public int LineNumber { get; set; } // Номер строки в файле
        public int EndPosition { get; set; } // Позиция конца вхождения

        public FoundFIO(string fio, int position, int lineNumber, int endPosition)
        {
            FIO = fio;
            Position = position;
            LineNumber = lineNumber;
            EndPosition = endPosition;
        }

        public override string ToString()
        {
            return $"ФИО: {FIO}, Строка: {LineNumber}, Начало: {Position}, Конец: {EndPosition}";
        }
    }

}
