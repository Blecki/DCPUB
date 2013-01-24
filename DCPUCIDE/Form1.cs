using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DCPUCIDE
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.TextBox outputBox = new TextBox();
        private System.Windows.Forms.TreeView astBox = new TreeView();

        private RichTextBox inputBox = new RichTextBox();
        private RichTextBox codeOutputBox = new RichTextBox();

        private DockPanel dockPanel = new DockPanel();

        private DockContent inputDock;
        private DockContent outputDock;
        private DockContent astDock;
        private DockContent assemblyDock;

        private String documentPath = "untitled.dc";

        public Form1()
        {
            InitializeComponent();

            dockPanel.Dock = DockStyle.Fill;
            Controls.Add(dockPanel);
            dockPanel.BringToFront();

            inputBox.Multiline = true;
            inputBox.AcceptsTab = true;
            inputBox.Font = new System.Drawing.Font(new FontFamily("Envy Code R"), 12);
            inputDock = MakeDockContent(inputBox, "input", DockState.Document);
            inputDock.Show(dockPanel);
            inputDock.CloseButtonVisible = false;

            astDock = MakeDockContent(astBox, "ast", DockState.DockRightAutoHide);
            astDock.Show(dockPanel);
            astDock.CloseButtonVisible = false;

            outputBox.Multiline = true;
            outputDock = MakeDockContent(outputBox, "output", DockState.DockBottomAutoHide);
            outputDock.Show(dockPanel);
            outputDock.CloseButtonVisible = false;
            outputBox.Font = new System.Drawing.Font("Fixedsys Excelsior 2.00", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            outputBox.ScrollBars = ScrollBars.Vertical;

            codeOutputBox.Multiline = true;
            codeOutputBox.ReadOnly = true;
            assemblyDock = MakeDockContent(codeOutputBox, "assembly", DockState.DockRight);
            assemblyDock.Show(dockPanel);
            assemblyDock.CloseButtonVisible = false;

            this.Text = documentPath;
        }

        private DockContent MakeDockContent(Control control, String name, DockState dockState)
        {
            DockContent content = new DockContent();
            content.Name = name;
            content.TabText = name;
            content.Text = name;
            content.ShowHint = dockState;

            control.Dock = DockStyle.Fill;
            content.Controls.Add(control);
            return content;
        }

        TreeNode buildAstTree(DCPUC.CompilableNode node)
        {
            var tree_node = new TreeNode(node.TreeLabel() + (node.WasFolded ? " FOLDED" : ""));
            foreach (var child in node.ChildNodes)
            {
                tree_node.Nodes.Add(buildAstTree(child as DCPUC.CompilableNode));
                if (child is DCPUC.FunctionDeclarationNode)
                {
                    foreach (var subfunc in (child as DCPUC.FunctionDeclarationNode).function.localScope.functions)
                        tree_node.Nodes.Add(buildAstTree(subfunc.Node));
                    foreach (var substruct in (child as DCPUC.FunctionDeclarationNode).function.localScope.structs)
                        tree_node.Nodes.Add(buildAstTree(substruct.Node));
                }
            }
            return tree_node;
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
                Text = fileDialog.FileName;
                documentPath = fileDialog.FileName;
            }

        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            codeOutputBox.Clear();
            var context = new DCPUC.CompileContext();
            var options = new DCPUC.CompileOptions();
            context.Initialize(options);
            
            var errorCount = 0;
            var warningCount = 0;
            context.onWarning += (s) => { warningCount += 1;
                outputBox.AppendText(s + "\r\n"); };

            if (context.Parse(inputBox.Text, (s) => { outputBox.AppendText(s); errorCount += 1; }))
            {
                context.Compile((s) => { outputBox.AppendText(s); errorCount += 1; });
                var assembly = context.Emit((s) => { outputBox.AppendText(s); errorCount += 1; });

                astBox.Nodes.Clear();
                astBox.Nodes.Add(buildAstTree(context.rootNode));
                foreach (var func in context.rootNode.function.localScope.functions)
                    astBox.Nodes.Add(buildAstTree(func.Node));
                foreach (var substruct in context.rootNode.function.localScope.structs)
                    astBox.Nodes.Add(buildAstTree(substruct.Node));

                var emitter = new TextBoxStream(codeOutputBox);
                codeOutputBox.Clear();
                if (assembly != null) assembly.Emit(emitter);
            }

            if (errorCount == 0)
                statusLabel.Text = "Compile succeeded";
            else
                statusLabel.Text = "Compile failed (" + errorCount + " errors)";
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var file = System.IO.File.Open(documentPath, System.IO.FileMode.OpenOrCreate);
            var stream = new System.IO.StreamWriter(file);
            stream.Write(inputBox.Text);
            stream.Close();
        }

    }
}
