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

        private ILogger _logger;
        private Factory _mover;
        private bool _noTick;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new TextLogger(textBox1);
            _logger.Log("Loaded GUI");

            _mover = new Factory(_logger);

            //textBox2.Text = _mover.GetBeatSaberPath();
            textBox2.Text = File.ReadAllText("beatsaberpath.txt");

            MaximumSize = Size;
            MinimumSize = Size;

            UpdateGui(_mover.IsLoaded);
        }

        private void UpdateGui(bool state)
        {
            button1.Enabled = state;
            button2.Enabled = state;
            button3.Enabled = state;
            button4.Enabled = state;
            button5.Enabled = state;
            button6.Enabled = state;

            if (_mover.Cam360State())
            {
                SafeButtonBehavior("Disable", Color.Chartreuse);
            }
            else
            {
                SafeButtonBehavior("Enable", Color.Crimson);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunCommand(_mover.RestoreAllCams);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RunCommand(_mover.CmdFront);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RunCommand(() =>
            {
                _mover.CmdFirstPerson(false);
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RunCommand(() =>
            {
                _mover.CmdFirstPerson(true);
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RunCommand(() =>
            {
                _mover.Destroy();
                _mover = new Factory(_logger);
                _mover.SetBeatSaberPath(textBox2.Text);
            });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RunCommand(_mover.CmdToggle360);
        }

        private void RunCommand(Action command)
        {
            _noTick = true;
            UpdateGui(false);
            _mover.RestoreAllCams();
            command.Invoke();
            _mover.SaveAllCams();
            UpdateGui(true);
            _noTick = false;
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
            if (_noTick)
            {
                return;
            }
            _mover.SetBeatSaberPath(textBox2.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateGui(_mover.IsLoaded);
        }
    }
}