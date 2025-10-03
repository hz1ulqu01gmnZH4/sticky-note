using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace SuperDuperStickyNotes.Services
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        private readonly Dictionary<int, Action> _hotkeyActions;
        private readonly HwndSource _source;
        private int _currentId;

        // Modifier keys
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        public HotkeyManager()
        {
            _hotkeyActions = new Dictionary<int, Action>();
            _currentId = 1;

            // Create a message-only window for receiving hotkey messages
            var windowParams = new HwndSourceParameters("HotkeyManager")
            {
                ParentWindow = new IntPtr(-3), // HWND_MESSAGE
                WindowStyle = 0,
                ExtendedWindowStyle = 0,
                PositionX = 0,
                PositionY = 0,
                Width = 0,
                Height = 0
            };

            _source = new HwndSource(windowParams);
            _source.AddHook(WndProc);
        }

        public bool RegisterHotkey(ModifierKeys modifiers, Key key, Action callback)
        {
            uint mod = 0;
            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                mod |= MOD_ALT;
            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                mod |= MOD_CONTROL;
            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                mod |= MOD_SHIFT;
            if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                mod |= MOD_WIN;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            int id = _currentId++;
            bool registered = RegisterHotKey(_source.Handle, id, mod, vk);

            if (registered)
            {
                _hotkeyActions[id] = callback;
            }

            return registered;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(id, out var action))
                {
                    action?.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            foreach (var id in _hotkeyActions.Keys)
            {
                UnregisterHotKey(_source.Handle, id);
            }

            _hotkeyActions.Clear();
            _source?.Dispose();
        }
    }
}