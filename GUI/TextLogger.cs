using System;
using System.IO;
using System.Windows.Forms;
using Mover;

namespace GUI
{
    public class TextLogger : BaseLogger, IDisposable
    {
        private delegate void SafeCallDelegate(string text);
        
        private readonly TextBox _textBox;
        private readonly StreamWriter _writer;

        public TextLogger(TextBox textBox)
        {
            _writer = new StreamWriter("debug.log");
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
            
            _writer.WriteLine(text);
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}