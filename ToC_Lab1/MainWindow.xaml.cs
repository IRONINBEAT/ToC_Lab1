﻿using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToC_Lab1
{
    public partial class MainWindow : Window
    {
        private string currentFilePath;
        private UndoStack undoStack; // Для хранения отменённых операций
        private RedoStack redoStack; // Для хранения повторённых операций

        public MainWindow()
        {
            InitializeComponent();
            currentFilePath = string.Empty;
            undoStack = new UndoStack();
            redoStack = new RedoStack();

            // Устанавливаем обработчик события изменения текста
            TextEditor.PreviewKeyDown += TextEditor_PreviewKeyDown;
            //this.PreviewKeyDown += MainWindow_PreviewKeyDown; // Добавляем обработчик для Ctrl+Z, Ctrl+Y
        }

        // Обработчик нажатия клавиш
        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата клавиша Enter или Tab
            if (e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Space)
            {
                // Добавляем текущее состояние текста в стек отмены
                undoStack.Push(TextEditor.Text);
                redoStack.Clear(); // Очищаем стек повторов при изменении текста
            }
        }

        //// Обработчик для горячих клавиш Ctrl+Z, Ctrl+Y, Ctrl+V
        //private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (Keyboard.Modifiers == ModifierKeys.Control)
        //    {
        //        if (e.Key == Key.Z) // Ctrl+Z (Undo)
        //        {
        //            UndoText(sender, e);
        //        }
        //        else if (e.Key == Key.Y) // Ctrl+Y (Redo)
        //        {
        //            RedoText(sender, e);
        //        }
        //        else if (e.Key == Key.V) // Ctrl+V (Paste)
        //        {
        //            PasteText(sender, e);
        //        }
        //    }
        //}

        private void NewFile(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Хотите сохранить файл перед созданием нового файла?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            TextEditor.Clear();
            currentFilePath = string.Empty;

            // Очищаем стеки Undo и Redo
            undoStack.Clear();
            redoStack.Clear();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Хотите сохранить файл перед открытием нового файла?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TextEditor.Text = File.ReadAllText(openFileDialog.FileName);
                currentFilePath = openFileDialog.FileName;

                // Очищаем стеки Undo и Redo
                undoStack.Clear();
                redoStack.Clear();
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileAs(sender, e);
            }
            else
            {
                File.WriteAllText(currentFilePath, TextEditor.Text);

                // Очищаем стеки Undo и Redo после сохранения
                undoStack.Clear();
                redoStack.Clear();
            }
        }

        private void SaveFileAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = ".txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, TextEditor.Text);
                currentFilePath = saveFileDialog.FileName;
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {

                // Спрашиваем у пользователя, хочет ли он сохранить изменения
                MessageBoxResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения. Хотите сохранить файл перед выходом?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Если пользователь выбрал "Да", то сохраняем файл
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // Если пользователь выбрал "Отмена", то отменяем выход
                    return;
                }
            }

            // Закрываем приложение
            Application.Current.Shutdown();
        }

        private void CutText(object sender, RoutedEventArgs e)
        {
            undoStack.Push(TextEditor.Text);
            redoStack.Clear();
            TextEditor.Cut();
        }

        private void CopyText(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        private void PasteText(object sender, RoutedEventArgs e)
        {
            undoStack.Push(TextEditor.Text);
            redoStack.Clear();
            TextEditor.Paste();
        }

        private void DeleteText(object sender, RoutedEventArgs e)
        {
            undoStack.Push(TextEditor.Text);
            redoStack.Clear();
            TextEditor.SelectedText = string.Empty;
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            TextEditor.SelectAll();
        }

        private void UndoText(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                redoStack.Push(TextEditor.Text);
                TextEditor.Text = undoStack.Pop();
                TextEditor.CaretIndex = Math.Min(caretPos, TextEditor.Text.Length); // Восстанавливаем позицию курсора
            }
        }

        private void RedoText(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                undoStack.Push(TextEditor.Text);
                TextEditor.Text = redoStack.Pop();
                TextEditor.CaretIndex = Math.Min(caretPos, TextEditor.Text.Length); // Восстанавливаем позицию курсора
            }
        }


        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            
            string helpFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help.html");

            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(helpFilePath) { UseShellExecute = true });
        }

        private void AboutApp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Текстовый редактор на WPF\nРазработан для лабораторной работы.", "О программе");
        }


        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ToggleMaximize(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void TextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
        }


        private void UpdateLineNumbers()
        {
            // Разделяем текст по строкам
            string[] lines = TextEditor.Text.Split('\n');

            // Создаем строку для номеров строк
            StringBuilder lineNumbers = new StringBuilder();

            // Заполняем строку номерами строк
            for (int i = 1; i <= lines.Length; i++)
            {
                lineNumbers.AppendLine(i.ToString());  // Добавляем номер строки с новой строки
            }

            // Устанавливаем номера строк в TextBlock
            LineNumbers.Text = lineNumbers.ToString();
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            undoStack.Push(TextEditor.Text);
            redoStack.Clear();

            TextEditor.Clear();
            ErrorOutput.Clear();
        }

        // Метод для поиска ФИО в тексте
        private void FindFIO(object sender, RoutedEventArgs e)
        {
            // Получаем текст из TextEditor
            string inputText = TextEditor.Text;

            // Регулярное выражение для поиска ФИО (фамилия и инициалы)
            string pattern = @"\b([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой)\b)\s*[А-ЯЁ]\.\s*[А-ЯЁ]\.|\b[А-ЯЁ]\.\s*[А-ЯЁ]\.\s*([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))\b";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(inputText);

            // Очищаем ErrorOutput перед выводом новых данных
            ErrorOutput.Clear();

            // Если найдены совпадения, выводим их
            if (matches.Count > 0)
            {
                // Разделяем текст на строки
                string[] lines = inputText.Split('\n');

                // Создаем StringBuilder для формирования результата
                StringBuilder result = new StringBuilder();

                foreach (Match match in matches)
                {
                    // Находим номер строки и позицию в строке
                    int lineNumber = 1;
                    int positionInLine = match.Index;
                    int currentLength = 0;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (currentLength + lines[i].Length + 1 > match.Index) // +1 для учета символа новой строки
                        {
                            lineNumber = i + 1;
                            positionInLine = match.Index - currentLength;
                            break;
                        }
                        currentLength += lines[i].Length + 1; // +1 для учета символа новой строки
                    }

                    // Формируем строку результата
                    string matchInfo = $"Найдено: {match.Value} (Строка: {lineNumber}, Позиция: {positionInLine})";
                    result.AppendLine(matchInfo);

                    // Выводим результат в ErrorOutput
                    ErrorOutput.AppendText(matchInfo + Environment.NewLine);
                }

                // Предлагаем пользователю сохранить результат в файл
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    DefaultExt = ".txt",
                    Title = "Сохранить результаты поиска"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Сохраняем результат в файл
                    File.WriteAllText(saveFileDialog.FileName, result.ToString());
                    MessageBox.Show("Результаты поиска успешно сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Если ничего не найдено, показываем сообщение
                ErrorOutput.AppendText("Не найдено совпадений!" + Environment.NewLine);
            }
        }

        //private void FindFIO(object sender, RoutedEventArgs e)
        //{
        //    // Получаем текст из TextEditor
        //    string inputText = TextEditor.Text;

        //    // Регулярное выражение для поиска ФИО (фамилия и инициалы)
        //    string pattern = @"\b([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой)\b)\s*[А-ЯЁ]\.\s*[А-ЯЁ]\.|\b[А-ЯЁ]\.\s*[А-ЯЁ]\.\s*([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))\b";

        //    Regex regex = new Regex(pattern);
        //    MatchCollection matches = regex.Matches(inputText);

        //    // Очищаем ErrorOutput перед выводом новых данных
        //    ErrorOutput.Clear();

        //    // Если найдены совпадения, выводим их
        //    if (matches.Count > 0)
        //    {
        //        foreach (Match match in matches)
        //        {
        //            ErrorOutput.AppendText(match.Value + Environment.NewLine); // Добавляем перенос строки
        //        }
        //    }
        //    else
        //    {
        //        // Если ничего не найдено, показываем сообщение
        //        ErrorOutput.AppendText("Не найдено совпадений!" + Environment.NewLine);
        //    }
        //}

    }


    // Классы для хранения стека операций Undo и Redo
    public class UndoStack
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public void Push(string text)
        {
            _stack.Push(text);
        }

        public string Pop()
        {
            return _stack.Count > 0 ? _stack.Pop() : null;
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public int Count => _stack.Count;
    }

    public class RedoStack
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public void Push(string text)
        {
            _stack.Push(text);
        }

        public string Pop()
        {
            return _stack.Count > 0 ? _stack.Pop() : null;
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public int Count => _stack.Count;
    }
}
