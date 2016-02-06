using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly
{
    public class InstructionListAstNode : AstNode
    {
        public IRNode resultNode = new IRNode();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                resultNode.AddChild(InstructionAstNode.ParseInstruction(child));
            }

            var labelTable = new Dictionary<String, Label>();
            foreach (var child in resultNode.children)
                if (child is LabelNode) labelTable.Add((child as LabelNode).label.rawLabel, (child as LabelNode).label);
            foreach (var child in resultNode.children)
                child.SetupLabels(labelTable);
        }        
    }
}