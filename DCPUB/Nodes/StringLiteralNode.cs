using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override string TreeLabel()
        {
            return "literal " + value;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            staticLabel = Assembly.Label.Make("_STRING");

            var data = new List<ushort>();
            data.Add((ushort)value.Length);
            foreach (var c in value)
                data.Add((ushort)c);
            context.AddData(staticLabel, data);
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.ExpressionNode();
            r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                Label(staticLabel));
            return r;
        }

    }

    
}
