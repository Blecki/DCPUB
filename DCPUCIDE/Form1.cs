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
            var tree_node = new TreeNode(node.TreeLabel());
            foreach (var child in node.ChildNodes)
            {
                tree_node.Nodes.Add(buildAstTree(child as DCPUC.CompilableNode));
                if (child is DCPUC.FunctionDeclarationNode)
                    foreach (var subfunc in (child as DCPUC.FunctionDeclarationNode).function.localScope.functions)
                        tree_node.Nodes.Add(buildAstTree(subfunc.Node));
            }
            return tree_node;
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            var context = new DCPUC.CompileContext();
            context.options.p = peepholeCB.Checked;
            context.onWarning += (s) => { outputBox.AppendText(s + "\r\n"); };

                if (context.Parse(inputBox.Text, (s) => { outputBox.AppendText(s); }))
                {
                    context.GatherSymbols(outputBox.AppendText);
                    context.FoldConstants();
                    context.Emit((s) => { outputBox.AppendText(s); });

                    astBox.Nodes.Clear();
                    astBox.Nodes.Add(buildAstTree(context.rootNode));
                    foreach (var func in context.rootNode.function.localScope.functions)
                        astBox.Nodes.Add(buildAstTree(func.Node));

                    foreach (var str in context.instructions)
                        outputBox.AppendText(str.ToString() + "\r\n");
                }

        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "DCPUC source (*.dc)|*.dc|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var file = System.IO.File.ReadAllText(fileDialog.FileName);
                inputBox.Clear();
                inputBox.AppendText(file);
            }

        }
    }
}
