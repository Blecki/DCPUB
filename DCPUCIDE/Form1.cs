using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DCPUCIDE
{
    public partial class Form1 : Form
    {
        Irony.Parsing.Parser Parser;

        public Form1()
        {
            InitializeComponent();
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            var assembly = new DCPUC.Assembly();
            DCPUC.Scope.CompileRoot(inputBox.Text, assembly, (s) => { outputBox.AppendText(s); });

            foreach (var str in assembly.instructions)
                outputBox.AppendText(str.ToString() + "\r\n");


        }
    }
}
