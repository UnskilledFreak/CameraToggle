using System;
using System.Windows.Forms;
using Mover;

namespace GUI
{
    public class TextLogger : BaseLogger
    {
        private delegate void SafeCallDelegate(string text);
        
        private readonly TextBox _textBox;

        public TextLogger(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override void Log(string message)
        {
            //message = $"{DateTime.Now:HH:mm:ss}: {message}";
            //LogSafe(message + Environment.NewLine + _textBox.Text);
            LogSafe($"{DateTime.Now:HH:mm:ss}: {message}");
        }
        
        private void LogSafe(string text)
        {
            if (_textBox.InvokeRequired)
            {
                var scd = new SafeCallDelegate(LogSafe);
                _textBox.Invoke(scd, new object[] {text});
                return;
            }

            //var d = _textBox.Text.Split(Environment.NewLine).ToList().Where(t => !string.IsNullOrWhiteSpace(t));
            //_textBox.Text = $@"[{DateTime.Now:HH:mm:ss}] "+ text + Environment.NewLine + string.Join(Environment.NewLine, d.Skip(0).Take(10));
            _textBox.Text = text + Environment.NewLine + _textBox.Text;
        }
    }
}