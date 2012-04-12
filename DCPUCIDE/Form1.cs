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
        public Form1()
        {
            InitializeComponent();
        }

        TreeNode buildAstTree(DCPUC.CompilableNode node)
        {
            var tree_node = new TreeNode(node.Role + ": " + node.TreeLabel());
            foreach (var child in node.ChildNodes)
                tree_node.Nodes.Add(buildAstTree(child as DCPUC.CompilableNode));
            return tree_node;
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            var ast = DCPUC.Scope.Parse(inputBox.Text, (s) => { outputBox.AppendText(s); });
            if (ast != null)
            {
                var assembly = new DCPUC.Assembly();
                DCPUC.Scope.CompileRoot(ast, assembly, (s) => { outputBox.AppendText(s); });

                astBox.Nodes.Clear();
                foreach (var n in buildAstTree(ast).Nodes[0].Nodes) astBox.Nodes.Add((TreeNode)n);

                foreach (var str in assembly.instructions)
                    outputBox.AppendText(str.ToString() + "\r\n");
            }

        }
    }
}
