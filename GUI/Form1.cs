using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mover;

namespace GUI
{
    public partial class Form1 : Form
    {
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
            
            _mover = new Factory(_logger);
            
            MaximumSize = Size;
            MinimumSize = Size;
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
                button6.BackColor = Color.Chartreuse;
                button6.Text = "Disable 360";
            }
            else
            {
                button6.BackColor = Color.Crimson;
                button6.Text = "Enable 360";
            }
        }
    }
}