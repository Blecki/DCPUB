using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Assembly;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class StringLiteralNode : CompilableNode
    {
        public string value;
        public Assembly.Label staticLabel;

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

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            staticLabel = Assembly.Label.Make("_STRING");

            var data = new List<Assembly.Operand>();
            data.Add(Constant((ushort)value.Length));
            foreach (var c in value)
                data.Add(Constant((ushort)c));
            context.AddData(staticLabel, data);
        }

        public override Operand GetFetchToken()
        {
            return Label(staticLabel);
        }

        public override Assembly.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), Label(staticLabel));
            return r;
        }

    }

    
}
