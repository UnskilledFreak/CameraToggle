using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mover;

namespace GUI
{
    public partial class Form1 : Form
    {
        private delegate void SafeCallDelegate(string text, Color color);

        private readonly Random _random = new Random();
        private ILogger _logger;
        private Factory _mover;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new TextLogger(textBox1);
            _logger.Log("Loaded GUI");

            _mover = new Factory(_logger)
            {
                Callback360Toggle = Button6Behavior
            };
            textBox2.Text = _mover.GetBeatSaberPath();

            MaximumSize = Size;
            MinimumSize = Size;

            UpdateGui();
        }

        private void UpdateGui()
        {
            var state = _mover.IsLoaded;

            button1.Enabled = state;
            button2.Enabled = state;
            button3.Enabled = state;
            button4.Enabled = state;
            button5.Enabled = state;
            button6.Enabled = state;

            Button6Behavior();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteCommand(Factory.CommandRestore);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WriteCommand(Factory.CommandFront);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WriteCommand(Factory.CommandFirstPerson);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WriteCommand(Factory.CommandFirstPersonSmallCams);
        }

        private void WriteCommand(string command)
        {
            var fileName = $@"{Factory.DirectoryName}\commands_{_random.Next(1000)}.txt";
            File.WriteAllText(fileName, command);
            _logger.Log($"wrote \"{command}\" to {fileName}");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _mover.Destroy();
            _mover = new Factory(_logger);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WriteCommand(Factory.CommandToggle360);
        }

        private void Button6Behavior()
        {
            if (_mover.AreCamsIn360())
            {
                SafeButtonBehavior("Disable", Color.Chartreuse);
            }
            else
            {
                SafeButtonBehavior("Enable", Color.Crimson);
            }
        }

        private void SafeButtonBehavior(string text, Color color)
        {
            if (button6.InvokeRequired)
            {
                var scd = new SafeCallDelegate(SafeButtonBehavior);
                button6.Invoke(scd, text, color);
                return;
            }

            button6.Text = text + @" 360 now";
            button6.BackColor = color;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            _mover.SetBeatSaberPath(textBox2.Text);
            UpdateGui();
        }
    }
}