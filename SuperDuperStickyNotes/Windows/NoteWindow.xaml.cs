using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SuperDuperStickyNotes.Models;

namespace SuperDuperStickyNotes.Windows
{
    public partial class NoteWindow : Window
    {
        private Note _note;
        private DispatcherTimer _saveTimer;
        private bool _isDirty;

        public event Action<Note>? NoteUpdated;
        public event Action<string>? NoteDeleted;

        // Available note colors
        private readonly string[] _colors = new[]
        {
            "#FFEB3B", // Yellow
            "#4FC3F7", // Blue
            "#81C784", // Green
            "#F48FB1", // Pink
            "#CE93D8", // Purple
            "#FFAB91", // Orange
            "#EF5350", // Red
            "#B0BEC5"  // Gray
        };
        private int _currentColorIndex = 0;

        public NoteWindow(Note note)
        {
            InitializeComponent();

            _note = note;
            DataContext = _note;

            // Set window position and size
            Left = _note.Position.X;
            Top = _note.Position.Y;
            Width = _note.Size.Width;
            Height = _note.Size.Height;

            // Set background color
            SetNoteColor(_note.Color);

            // Find current color index
            for (int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i].Equals(_note.Color, StringComparison.OrdinalIgnoreCase))
                {
                    _currentColorIndex = i;
                    break;
                }
            }

            // Initialize save timer (auto-save after 2 seconds of inactivity)
            _saveTimer = new DispatcherTimer();
            _saveTimer.Interval = TimeSpan.FromSeconds(2);
            _saveTimer.Tick += SaveTimer_Tick;

            // Track window position/size changes
            LocationChanged += Window_LocationChanged;
            SizeChanged += Window_SizeChanged;

            // Load content after window is fully loaded
            this.Loaded += Window_Loaded;

            // Add keyboard shortcuts for formatting
            this.KeyDown += Window_KeyDown;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load content into RichTextBox after UI is fully initialized
            LoadContentIntoRichTextBox();
        }

        private void LoadContentIntoRichTextBox()
        {
            if (ContentRichTextBox == null)
            {
                // UI not ready yet, skip loading
                return;
            }

            if (!string.IsNullOrEmpty(_note.Content))
            {
                var flowDocument = new FlowDocument();
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(_note.Content));
                flowDocument.Blocks.Add(paragraph);
                ContentRichTextBox.Document = flowDocument;
            }
        }

        private string GetTextFromRichTextBox()
        {
            if (ContentRichTextBox == null || ContentRichTextBox.Document == null)
            {
                return string.Empty;
            }

            TextRange textRange = new TextRange(
                ContentRichTextBox.Document.ContentStart,
                ContentRichTextBox.Document.ContentEnd);
            return textRange.Text.Trim();
        }

        private void SetNoteColor(string colorHex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                var brush = new SolidColorBrush(color);

                if (MainBorder != null)
                {
                    MainBorder.Background = brush;
                }

                if (HeaderBorder != null)
                {
                    HeaderBorder.Background = new SolidColorBrush(Color.FromArgb(255,
                        (byte)(color.R * 0.9),
                        (byte)(color.G * 0.9),
                        (byte)(color.B * 0.9)));
                }

                if (ContentRichTextBox != null)
                {
                    ContentRichTextBox.Background = brush;
                }
            }
            catch
            {
                // Default to yellow if color parsing fails
                SetNoteColor("#FFEB3B");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to edit
                ContentRichTextBox.Focus();
                ContentRichTextBox.SelectAll();
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ContentRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the note content from RichTextBox
            _note.Content = GetTextFromRichTextBox();
            _isDirty = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveTimer_Tick(object? sender, EventArgs e)
        {
            _saveTimer.Stop();
            SaveNote();
        }

        private void Window_LocationChanged(object? sender, EventArgs e)
        {
            _note.Position.X = Left;
            _note.Position.Y = Top;
            _isDirty = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _note.Size.Width = Width;
            _note.Size.Height = Height;
            _isDirty = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveNote()
        {
            if (_isDirty)
            {
                _isDirty = false;
                NoteUpdated?.Invoke(_note);
            }
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _note.Pinned = !_note.Pinned;
            Topmost = _note.Pinned;
            SaveNote();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Cycle through colors
            _currentColorIndex = (_currentColorIndex + 1) % _colors.Length;
            _note.Color = _colors[_currentColorIndex];
            SetNoteColor(_note.Color);
            SaveNote();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation if note has content
            if (!string.IsNullOrWhiteSpace(_note.Content))
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this note?",
                    "Delete Note",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            NoteDeleted?.Invoke(_note.Id);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _saveTimer.Stop();
            SaveNote();
            base.OnClosed(e);
        }

        // Tags Methods

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a simple input dialog for tag name
            var dialog = new Window
            {
                Title = "Add Tag",
                Width = 300,
                Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(textBox, 0);
            grid.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);

            var okButton = new Button
            {
                Content = "OK",
                Width = 60,
                Height = 25,
                Margin = new Thickness(0, 0, 5, 0),
                IsDefault = true
            };
            okButton.Click += (s, args) =>
            {
                var tagName = textBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(tagName) && !_note.Tags.Contains(tagName))
                {
                    _note.Tags.Add(tagName);
                    SaveNote();
                    dialog.DialogResult = true;
                }
            };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 60,
                Height = 25,
                IsCancel = true
            };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            dialog.Content = grid;

            textBox.Focus();
            dialog.ShowDialog();
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagName)
            {
                _note.Tags.Remove(tagName);
                SaveNote();
            }
        }

        // Formatting Methods

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle keyboard shortcuts for formatting
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.B:
                        ToggleBold();
                        e.Handled = true;
                        break;
                    case Key.I:
                        ToggleItalic();
                        e.Handled = true;
                        break;
                    case Key.U:
                        ToggleUnderline();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBold();
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleItalic();
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleUnderline();
        }

        private void BulletListButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleList(TextMarkerStyle.Disc);
        }

        private void NumberedListButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleList(TextMarkerStyle.Decimal);
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                if (double.TryParse(item.Tag.ToString(), out double fontSize))
                {
                    ApplyFontSize(fontSize);
                }
            }
        }

        private void ToggleBold()
        {
            if (!ContentRichTextBox.IsFocused)
                ContentRichTextBox.Focus();

            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);
                var newWeight = (currentWeight is FontWeight weight && weight == FontWeights.Bold)
                    ? FontWeights.Normal
                    : FontWeights.Bold;

                selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
            }
        }

        private void ToggleItalic()
        {
            if (!ContentRichTextBox.IsFocused)
                ContentRichTextBox.Focus();

            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);
                var newStyle = (currentStyle is FontStyle style && style == FontStyles.Italic)
                    ? FontStyles.Normal
                    : FontStyles.Italic;

                selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
            }
        }

        private void ToggleUnderline()
        {
            if (!ContentRichTextBox.IsFocused)
                ContentRichTextBox.Focus();

            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentDecoration = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                var newDecoration = (currentDecoration != DependencyProperty.UnsetValue &&
                                    currentDecoration is TextDecorationCollection decorations &&
                                    decorations == TextDecorations.Underline)
                    ? null
                    : TextDecorations.Underline;

                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newDecoration);
            }
        }

        private void ToggleList(TextMarkerStyle markerStyle)
        {
            if (!ContentRichTextBox.IsFocused)
                ContentRichTextBox.Focus();

            var selection = ContentRichTextBox.Selection;
            if (selection != null)
            {
                var startParagraph = selection.Start.Paragraph;
                var endParagraph = selection.End.Paragraph;

                if (startParagraph != null)
                {
                    // Check if we're already in a list
                    var currentList = startParagraph.Parent as List;
                    if (currentList != null)
                    {
                        // Remove from list
                        var items = new System.Collections.Generic.List<ListItem>();
                        foreach (ListItem item in currentList.ListItems)
                        {
                            items.Add(item);
                        }

                        var blocks = new System.Collections.Generic.List<Block>();
                        foreach (var item in items)
                        {
                            foreach (Block block in item.Blocks)
                            {
                                blocks.Add(block);
                            }
                        }

                        var parentList = currentList.Parent as FlowDocument;
                        if (parentList != null)
                        {
                            var index = parentList.Blocks.ToList().IndexOf(currentList);
                            parentList.Blocks.Remove(currentList);

                            foreach (var block in blocks)
                            {
                                parentList.Blocks.InsertBefore(parentList.Blocks.ElementAt(Math.Min(index, parentList.Blocks.Count - 1)), block);
                            }
                        }
                    }
                    else
                    {
                        // Create new list
                        var list = new List() { MarkerStyle = markerStyle };
                        var listItem = new ListItem();

                        // Move paragraph into list item
                        var paragraph = new Paragraph(new Run(selection.Text));
                        listItem.Blocks.Add(paragraph);
                        list.ListItems.Add(listItem);

                        // Replace paragraph with list
                        var doc = ContentRichTextBox.Document;
                        doc.Blocks.InsertBefore(startParagraph, list);
                        doc.Blocks.Remove(startParagraph);
                    }
                }
            }
        }

        private void ApplyFontSize(double fontSize)
        {
            if (!ContentRichTextBox.IsFocused)
                ContentRichTextBox.Focus();

            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            }
            else
            {
                // Apply to entire document if no selection
                ContentRichTextBox.FontSize = fontSize;
            }
        }
    }
}