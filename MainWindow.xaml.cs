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
        int KeysIntervalInMilliSeconds = 20;
        private TimeSpan TimerStartKeysDelay;
        private TimeSpan TimerKeyStrokeInterval;
        private string TextToSend = "";
        private List<string> KeySequence = [];
        DispatcherTimer SendKeyStartTimer = new DispatcherTimer();
        DispatcherTimer SendKeyStrokeTimer = new DispatcherTimer();
        bool UseKeyInterval = false;
        bool FixScandinavianCaret = true;

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
            TimerStartKeysDelay = TimeSpan.FromMilliseconds(StartKeysDelayInMilliSeconds);
            TimerKeyStrokeInterval = TimeSpan.FromMilliseconds(KeysIntervalInMilliSeconds);
            HiddenText = true;
            MouseDown += Window_MouseDown;
            SendKeyStartTimer.Tick += StartKeyPresses;
            SendKeyStrokeTimer.Tick += SendKeyStroke;
        }

        private void ButtonSendText_Click(object sender, RoutedEventArgs e)
        {
            SendKeyStartTimer.Interval = TimerStartKeysDelay;
            SendKeyStartTimer.Start();
            TextToSend = TextBoxMain.Text;
            Debug.WriteLine($"Text to send:{TextToSend}:");
            if (TextToSend == "") TextToSend = "Lorem Ipsum! {} () [] ^ % ~ ok";
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
    }
}