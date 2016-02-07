using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Assembly;
using DCPUB.Intermediate;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class StringLiteralNode : CompilableNode
    {
        public string value;
        public Intermediate.Label staticLabel;

        public static String UnescapeString(String s)
        {
            var place = 0;
            var r = "";
            while (place < s.Length)
            {
                if (s[place] == '\\')
                {
                    if (place < s.Length - 1 && s[place + 1] == 'n')
                        r += '\n';
                    place += 2;
                }
                else
                {
                    r += s[place];
                    ++place;
                }
            }
            return r;
        }
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            value = treeNode.FindTokenAndGetText();
            value = value.Substring(1, value.Length - 2);

            value = UnescapeString(value);
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            staticLabel = Intermediate.Label.Make("_STRING");

            var data = new List<Intermediate.Operand>();
            data.Add(Constant((ushort)value.Length));
            foreach (var c in value)
                data.Add(Constant((ushort)c));
            context.AddData(staticLabel, data);
        }

        public override Operand GetFetchToken()
        {
            return Label(staticLabel);
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new Intermediate.TransientNode();
            r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(staticLabel));
            return r;
        }

    }

    
}
