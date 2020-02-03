using System;
using System.IO;
using System.Windows.Forms;
using CameraPlusExternalMover;

namespace GUI
{
    public partial class Form1 : Form
    {
        private readonly Random _random = new Random();
        private ILogger _logger;
        private Mover _mover;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new TextLogger(textBox1);
            _logger.Log("Loaded GUI");
            
            _mover = new Mover(_logger);
            
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteCommand("Restore");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WriteCommand("Front");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WriteCommand("FirstPerson");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WriteCommand("FirstPersonSmallCams");
        }

        private void WriteCommand(string command)
        {
            var fileName = $@"{Mover.DirectoryName}\commands_{_random.Next(1000)}.txt";
            File.WriteAllText(fileName, command);
            _logger.Log($"wrote \"{command}\" to {fileName}");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _mover.Destroy();
            _mover = new Mover(_logger);
        }
    }
}