using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Ast.Assembly
{
    public class InstructionListAstNode : AstNode
    {
        public Intermediate.IRNode resultNode = new Intermediate.IRNode();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                resultNode.AddChild(InstructionAstNode.ParseInstruction(child));
            }

            var labelTable = new Dictionary<String, Intermediate.Label>();
            foreach (var child in resultNode.children)
                if (child is Intermediate.LabelNode)
                    labelTable.Add((child as Intermediate.LabelNode).label.rawLabel, (child as Intermediate.LabelNode).label);
            foreach (var child in resultNode.children)
                child.SetupLabels(labelTable);
        }        
    }
}