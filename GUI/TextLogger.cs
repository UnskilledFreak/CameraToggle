using System;
using System.Windows.Forms;
using Mover;

namespace GUI
{
    public class TextLogger : ILogger
    {
        private TextBox _textBox;

        public TextLogger(TextBox textBox)
        {
            _textBox = textBox;
        }

        public void Log(string message)
        {
            message = $"{DateTime.Now:HH:mm:ss}: {message}";
            
            _textBox.Text = message + Environment.NewLine + _textBox.Text;
        }
    }
}