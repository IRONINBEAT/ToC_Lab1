﻿using Microsoft.Win32;
using System.ComponentModel;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentFilePath;
        private UndoStack _undoStack;
        private RedoStack _redoStack;

        public event PropertyChangedEventHandler? PropertyChanged;

        public UndoStack UndoStack
        {
            get => _undoStack;
            private set
            {
                _undoStack = value;
                OnPropertyChanged(nameof(UndoStack));
            }
        }

        public RedoStack RedoStack
        {
            get => _redoStack;
            private set
            {
                _redoStack = value;
                OnPropertyChanged(nameof(RedoStack));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            currentFilePath = string.Empty;
            _undoStack = new UndoStack();
            _redoStack = new RedoStack();
            DataContext = this; 
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
                _undoStack.Push(TextEditor.Text);
                _redoStack.Clear(); // Очищаем стек повторов при изменении текста
            }
        }

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
            _undoStack.Clear();
            _redoStack.Clear();
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
                _undoStack.Clear();
                _redoStack.Clear();
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
                _undoStack.Clear();
                _redoStack.Clear();
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
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.Cut();
        }

        private void CopyText(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        private void PasteText(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.Paste();
        }

        private void DeleteText(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.SelectedText = string.Empty;
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            TextEditor.SelectAll();
        }

        private void UndoText(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                _redoStack.Push(TextEditor.Text);
                TextEditor.Text = _undoStack.Pop();
                TextEditor.CaretIndex = Math.Min(caretPos, TextEditor.Text.Length); // Восстанавливаем позицию курсора
            }
        }

        private void RedoText(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                _undoStack.Push(TextEditor.Text);
                TextEditor.Text = _redoStack.Pop();
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

        private void HighlightedText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Передаем фокус на TextEditor
            TextEditor.Focus();

            // Вызываем метод TextEditor_TextChanged
            TextEditor_TextChanged(sender, null);
        }

        private void TextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
            TextEditor.Visibility = Visibility.Visible;
            HighlightedText.Visibility = Visibility.Collapsed;
            
        }


        private void UpdateLineNumbers()
        {
            // Получаем текст из RichTextBox
            string text = TextEditor.Text;

            // Разделяем текст по строкам
            string[] lines = text.Split('\n');

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
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();

            TextEditor.Clear();
            ErrorOutput.Clear();
        }

        private void FindFIO(object sender, RoutedEventArgs e)
        {
            // Получаем текст из TextEditor
            string inputText = TextEditor.Text;

            // Регулярное выражение для поиска ФИО (фамилия и инициалы)
            string pattern = @"([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))\s*[А-ЯЁ]\.\s*[А-ЯЁ]\.|[А-ЯЁ]\.\s*[А-ЯЁ]\.\s*([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(inputText);

            // Очищаем ErrorOutput перед выводом новых данных
            ErrorOutput.Clear();

            // Если найдены совпадения, выводим их
            if (matches.Count > 0)
            {
                // Создаем TextBlock для отображения текста с подсветкой
                HighlightedText.Inlines.Clear();

                // Разделяем текст на строки
                string[] lines = inputText.Split('\n');

                // Создаем StringBuilder для формирования результата
                StringBuilder result = new StringBuilder();

                int lastIndex = 0;
                foreach (Match match in matches)
                {
                    // Находим номер строки и позицию в строке
                    int lineNumber = 1;
                    int positionInLine = match.Index;
                    int currentLength = 0;

                    // Добавляем текст до совпадения
                    HighlightedText.Inlines.Add(new Run(inputText.Substring(lastIndex, match.Index - lastIndex)));

                    // Добавляем совпадение с подсветкой
                    HighlightedText.Inlines.Add(new Run(match.Value)
                    {
                        Background = Brushes.Yellow,
                        Foreground = Brushes.Black
                    });

                    // Вычисляем позицию конца вхождения
                    int endPosition = match.Index + match.Length;

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
                    string matchInfo = $"Найдено: {match.Value} (Строка: {lineNumber}, Позиция начала: {positionInLine}, Позиция конца: {endPosition})";
                    result.AppendLine(matchInfo);

                    // Выводим результат в ErrorOutput
                    ErrorOutput.AppendText(matchInfo + Environment.NewLine);

                    lastIndex = match.Index + match.Length;
                }

                // Добавляем оставшийся текст
                HighlightedText.Inlines.Add(new Run(inputText.Substring(lastIndex)));

                // Показываем TextBlock с подсветкой
                HighlightedText.Visibility = Visibility.Visible;
                TextEditor.Visibility = Visibility.Collapsed;

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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


    // Классы для хранения стека операций Undo и Redo
    public class UndoStack : INotifyPropertyChanged
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Push(string text)
        {
            _stack.Push(text);
            OnPropertyChanged(nameof(Count));
        }

        public string Pop()
        {
            var result = _stack.Count > 0 ? _stack.Pop() : null;
            OnPropertyChanged(nameof(Count));
            return result;
        }

        public void Clear()
        {
            _stack.Clear();
            OnPropertyChanged(nameof(Count));
        }

        public int Count => _stack.Count;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RedoStack : INotifyPropertyChanged
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Push(string text)
        {
            _stack.Push(text);
            OnPropertyChanged(nameof(Count));
        }

        public string Pop()
        {
            var result = _stack.Count > 0 ? _stack.Pop() : null;
            OnPropertyChanged(nameof(Count));
            return result;
        }

        public void Clear()
        {
            _stack.Clear();
            OnPropertyChanged(nameof(Count));
        }

        public int Count => _stack.Count;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
