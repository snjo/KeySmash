using Hotkeys;
using KeySmash.Properties;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KeySmash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _hiddenText = true;
        int StartKeysDelayInMilliSeconds = 2000;
        int KeysIntervalInMilliSeconds = 50;
        private TimeSpan TimerStartKeysDelay;
        private TimeSpan TimerKeyStrokeInterval;
        private string TextToSend = "";
        private List<string> KeySequence = [];
        DispatcherTimer SendKeyStartTimer = new DispatcherTimer();
        DispatcherTimer SendKeyStrokeTimer = new DispatcherTimer();
        public nint Handle = 0;
            
        public bool UseKeyInterval { get; set; } = false;
        public bool FixScandinavianCaret { get; set; } = true;

        public bool HiddenText
        {
            get
            {
                return _hiddenText;
            }
            set
            {
                _hiddenText = value;
                //MenuItemToggleHiddenText.IsChecked = _hiddenText;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            StartKeysDelayInMilliSeconds = settings.TypingStartDelay;
            TimerStartKeysDelay = TimeSpan.FromMilliseconds(StartKeysDelayInMilliSeconds);
            TimerKeyStrokeInterval = TimeSpan.FromMilliseconds(KeysIntervalInMilliSeconds);
            HiddenText = true;
            MouseDown += Window_MouseDown;
            SendKeyStartTimer.Tick += StartKeyPresses;
            SendKeyStrokeTimer.Tick += SendKeyStroke;
            UpdateDelay();
            this.Topmost = true;

            HotkeyGetClipboardKey.Text = settings.hkGetClipboardKey;
            HotkeyGetClipboardCtrl.IsChecked = settings.hkGetClipboardCtrl;
            HotkeyGetClipboardAlt.IsChecked = settings.hkGetClipboardAlt;
            HotkeyGetClipboardShift.IsChecked = settings.hkGetClipboardShift;

            HotkeyTypeTextKey.Text = settings.hkTypeTextKey;
            HotkeyTypeTextCtrl.IsChecked = settings.hkTypeTextCtrl;
            HotkeyTypeTextAlt.IsChecked = settings.hkTypeTextAlt;
            HotkeyTypeTextShift.IsChecked = settings.hkTypeTextShift;
        }

        public void ExitApplication(object sender, EventArgs e)
        { 
            Close();
        }

        #region hotkeys

        Settings settings = Settings.Default;

        // For each hotkey below, add entries in Settings, hk???Key, hk???Ctrl, hk???Alt, hk???Shift, hk???Win
        public List<string> HotkeyNames = new List<string>
        {
            "GetClipboard",
            "TypeText",
        };
        public Dictionary<string, Hotkey> HotkeyList = new Dictionary<string, Hotkey>();

        const int MYACTION_HOTKEY_ID = 1;
        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.WriteLine($"wndproc {msg}");
            if (msg == 0x0312)
            {
                //Debug.WriteLine($"hotkey pressed: {hwnd}, {msg}, {wParam}, {lParam}");
                Keys key = (Keys)(((int)lParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)lParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = wParam.ToInt32();                                        // The id of the hotkey that was pressed.
                //Debug.WriteLine($"KEY: {key} MODIFIER {modifier} ID {id}");
                HandleHotkey(id);
            }

            //if (msg == Hotkeys.Constants.WM_HOTKEY_MSG_ID)
            //{
                //Keys key = (Keys)(((int)lParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //KeyModifier modifier = (KeyModifier)((int)lParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                //int id = wParam.ToInt32();                                        // The id of the hotkey that was pressed.
                ////MessageBox.Show("Hotkey " + id + " has been pressed!");
                //HandleHotkey(id);
            //}

            //HandleHotkey(id);

            return IntPtr.Zero;
        }

        private void HandleHotkey(int id)
        {
            //Debug.WriteLine($"Handle hotkey id: {id}");
            if (HotkeyList["GetClipboard"] != null)
            {
                //Debug.WriteLine($"GetClipboad id : {HotkeyList["GetClipboard"].ghk.id}");
                if (id == HotkeyList["GetClipboard"].ghk.id)
                {
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        string clip = System.Windows.Clipboard.GetText();
                        if (clip.Length == 0) TextBoxMain.Text = "";
                        if (clip.Length > 50) TextBoxMain.Text = "";
                        TextBoxMain.Text = clip;
                    }
                }
            }
            if (HotkeyList["TypeText"] != null)
            {
                //Debug.WriteLine($"GetClipboad id : {HotkeyList["GetClipboard"].ghk.id}");
                if (id == HotkeyList["TypeText"].ghk.id)
                {
                    SendText(500);
                }
            }
        }

        [Flags]
        public enum Modifiers
        {
            NoMod = 0x0000,
            Alt = 0x0001,
            Ctrl = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
            HotkeyList = HotkeyTools.LoadHotkeys(HotkeyList, HotkeyNames, this);
            Debug.WriteLine($"Handle: {Handle}");
            Debug.WriteLine($"Register hotkeys");
            var source = PresentationSource.FromVisual(this as Visual) as HwndSource;
            if (source == null)
                throw new Exception("Could not create hWnd source from window.");
            source.AddHook(WndProc);

            if (settings.RegisterHotkeys) // optional
            {
                Debug.WriteLine($"Register hotkeys is on");
                HotkeyTools.RegisterHotkeys(HotkeyList);
                // RegisterHotkeys returns a string[] with any failed hotkey registrations that can be used to output an error message
            }

            SetBackgroundColor(settings.BackgroundColor);

            //test
            //RegisterHotKey(new WindowInteropHelper(this).Handle, MYACTION_HOTKEY_ID, (int)Modifiers.Ctrl, (int)Keys.A);
            //RegisterHotKey(new WindowInteropHelper(this).Handle, 2, (int)Modifiers.Ctrl, (int)Keys.S);
            //RegisterHotKey(new WindowInteropHelper(this).Handle, 3, (int)Modifiers.Ctrl, (int)Keys.D);
        }

        #endregion

        private void ButtonSendText_Click(object sender, RoutedEventArgs e)
        {
            SendText(StartKeysDelayInMilliSeconds);
        }

        private void SendText(int delay)
        {
            TextToSend = TextBoxMain.Text;
            if (TextToSend == "") return;
            TimerStartKeysDelay = TimeSpan.FromMilliseconds(delay);
            SendKeyStartTimer.Interval = TimerStartKeysDelay;
            SendKeyStartTimer.Start();
            //Debug.WriteLine($"Text to send:{TextToSend}:");
            if (TextToSend == "¤") TextToSend = "Lorem Ipsum! {} () [] ^ % ~ ok"; // test string
            CreateKeySequence(TextToSend);
        }

        private void CreateKeySequence(string text)
        {
            KeySequence.Clear();
            string encloseChars = "{}()[]^+%~";
            // convert {} () [] ^ + % ~
            for (int i = 0; i < TextToSend.Length; i++)
            {
                string part = TextToSend[i].ToString();
                if (encloseChars.Contains(part))
                {
                    part = "{" + part + "}";
                }
                if (part == "{~}") part = "{~} ";
                if (part == "¨") part = "¨ ";
                if (part == "{^}") part = "{^} ";
                if (part == "{^} " && FixScandinavianCaret) // fix for Scandinavian / German keyboards not using Shift+6 for ^, but Shift+OEM5 (Shift+¨)
                {
                    part = "+(¨) ";
                }
                KeySequence.Add(part);
            }
        }

        private void StartKeyPresses(object? sender, EventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                Debug.WriteLine($"Awaiting modifier release");
                return;
            }
            SendKeyStartTimer.Stop();
            SendKeyStrokeTimer.Interval = TimerKeyStrokeInterval;

            if (UseKeyInterval)
            {
                SendKeyStrokeTimer.Start();
            }
            else
            {
                StringBuilder sb = new();
                foreach (string segment in KeySequence)
                {
                    sb.Append(segment);
                }
                SendKeys.SendWait(sb.ToString());
            }
            //Debug.WriteLine($"Starting keypresses");
        }

        private void SendKeyStroke(object? sender, EventArgs e)
        {
            
            //Debug.WriteLine($"Keystrokes: {KeySequence.Count}");
            if (KeySequence.Count > 0)
            {
                string text = KeySequence.First();
                
                //Debug.WriteLine($"Send keystroke {text}");
                SendKeys.SendWait(text);
                KeySequence.RemoveAt(0);
            }
            else
            {
                SendKeyStrokeTimer.Stop();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ToggleHiddenText(object sender, RoutedEventArgs e)
        {
            HiddenText = !HiddenText;
            Debug.WriteLine($"Toggle hidden {HiddenText}");
        }

        private void ClickDecreaseDelay(object sender, RoutedEventArgs e)
        {
            StartKeysDelayInMilliSeconds -= 500;
            UpdateDelay();
        }

        private void ClickIncreaseDelay(object sender, RoutedEventArgs e)
        {
            StartKeysDelayInMilliSeconds += 500;
            UpdateDelay();
        }

        private void UpdateDelay()
        {
            StartKeysDelayInMilliSeconds = Math.Clamp(StartKeysDelayInMilliSeconds, 500, 5000);
            DelayInput.Text = $"{(float)StartKeysDelayInMilliSeconds / 1000f:0.0}s";
            settings.TypingStartDelay = StartKeysDelayInMilliSeconds;
            settings.Save();
        }

        private void ClickSetBackGroundColor(object sender)//System.Windows.Media.Color color)
        {
            System.Windows.Media.Color? color = null;
            if (sender is System.Windows.Controls.Button button)
            {
                //System.Windows.Controls.Button? button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    if (button.Background is SolidColorBrush br)
                    {
                        color = br.Color;
                        //this.SetBackGroundColor(color);
                        this.Background = br;
                        Debug.WriteLine($"Setting background color to {color}");
                        settings.BackgroundColor = System.Drawing.Color.FromArgb(color.Value.R, color.Value.G, color.Value.B);
                        settings.Save();
                    }
                }
            }
            else
            {
                Debug.WriteLine($"sender is not button");
            }
        }

        private void SetBackgroundColor(System.Drawing.Color color)
        {
            this.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
        }

        private void ClickColor(object sender, RoutedEventArgs e)
        {
            //gray
            ClickSetBackGroundColor(sender);
        }

        private void UpdateHotkeys(object sender, RoutedEventArgs e)
        {
            settings.hkGetClipboardKey = HotkeyGetClipboardKey.Text;
            if (HotkeyGetClipboardCtrl.IsChecked != null)
            {
                settings.hkGetClipboardCtrl = (bool)HotkeyGetClipboardCtrl.IsChecked;
            }
            if (HotkeyGetClipboardAlt.IsChecked != null)
            {
                settings.hkGetClipboardAlt = (bool)HotkeyGetClipboardAlt.IsChecked;
            }
            if (HotkeyGetClipboardShift.IsChecked != null)
            {
                settings.hkGetClipboardShift = (bool)HotkeyGetClipboardShift.IsChecked;
            }
            //---
            settings.hkTypeTextKey = HotkeyTypeTextKey.Text;
            if (HotkeyTypeTextCtrl.IsChecked != null)
            {
                settings.hkTypeTextCtrl = (bool)HotkeyTypeTextCtrl.IsChecked;
            }
            if (HotkeyTypeTextAlt.IsChecked != null)
            {
                settings.hkTypeTextAlt = (bool)HotkeyTypeTextAlt.IsChecked;
            }
            if (HotkeyTypeTextShift.IsChecked != null)
            {
                settings.hkTypeTextShift = (bool)HotkeyTypeTextShift.IsChecked;
            }

            settings.Save();
            HotkeyTools.UpdateHotkeys(HotkeyList, HotkeyNames, this);
        }
    }
}