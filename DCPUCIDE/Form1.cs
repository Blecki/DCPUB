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
            Parser = new Irony.Parsing.Parser(new DCPUC.Grammar());
            
        }

        private static String extractLine(String s, int c)
        {
            int lc = 0;
            int p = 0;
            while (p < s.Length && lc < c)
            {
                if (s[p] == '\n') lc++;
                ++p;
            }

            int ls = p;
            while (p < s.Length && s[p] != '\n') ++p;

            return s.Substring(ls, p - ls);
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            var program = Parser.Parse(inputBox.Text);
            if (program.HasErrors())
            {
                foreach (var msg in program.ParserMessages)
                {
                    outputBox.AppendText(msg.Level + ": " + msg.Message + " [line:" + msg.Location.Line + " column:" + msg.Location.Column + "]\r\n");
                    outputBox.AppendText(extractLine(inputBox.Text, msg.Location.Line) + "\r\n");
                    outputBox.AppendText(new String(' ', msg.Location.Column) + "^\r\n");
                }
                return;
            }

            var root = program.Root.AstNode as DCPUC.CompilableNode; //Irony.Interpreter.Ast.AstNode;
            var assembly = new DCPUC.Assembly();
            var scope = new DCPUC.Scope();

            try
            {
                root.Compile(assembly, scope, DCPUC.Register.DISCARD);
                assembly.Add("BRK", "", "", "Non-standard");
                foreach (var pendingFunction in scope.pendingFunctions)
                    pendingFunction.CompileFunction(assembly);
            }
            catch (DCPUC.CompileError c)
            {
                outputBox.AppendText(c.Message);
                return;
            }

            foreach (var str in assembly.instructions)
                outputBox.AppendText(str.ToString() + "\r\n");

        }
    }
}
