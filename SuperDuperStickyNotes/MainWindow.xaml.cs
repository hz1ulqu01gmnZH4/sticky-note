using System;
using System.Windows;
using System.Windows.Input;
using SuperDuperStickyNotes.Models;
using SuperDuperStickyNotes.Services;

namespace SuperDuperStickyNotes
{
    public partial class MainWindow : Window
    {
        private NoteManager? _noteManager;

        public MainWindow()
        {
            InitializeComponent();

            // This window is hidden by default and acts as the application container
            // Individual notes are shown in separate windows
        }

        public void SetNoteManager(NoteManager noteManager)
        {
            _noteManager = noteManager;
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            _noteManager?.CreateNewNote();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Toggle command bar with Ctrl+P
            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleCommandBar();
                e.Handled = true;
            }
            // Hide command bar with Escape
            else if (e.Key == Key.Escape && CommandBar.Visibility == Visibility.Visible)
            {
                CommandBar.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void ToggleCommandBar()
        {
            if (CommandBar.Visibility == Visibility.Collapsed)
            {
                CommandBar.Visibility = Visibility.Visible;
                CommandInput.Focus();
                CommandInput.Clear();
            }
            else
            {
                CommandBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}