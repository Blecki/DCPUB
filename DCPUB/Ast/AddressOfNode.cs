using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;
using DCPUB.Assembly;

namespace DCPUB
{
    public class AddressOfNode : CompilableNode
    {
        public Model.Variable variable = null;
        public Model.Function function = null;
        public String variableName;
        public Model.Label label = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            variable = enclosingScope.FindVariable(variableName);

            if (variable == null) 
            {
                function = enclosingScope.FindFunction(variableName);
                if (function == null)
                {
                    foreach (var l in enclosingScope.activeFunction.function.labels)
                    {
                        if (l.declaredName == variableName)
                            label = l;
                    }
                    if (label == null)
                        context.ReportError(this, "Could not find symbol " + variableName);
                }
                else
                    enclosingScope.activeFunction.function.Calls.Add(function);
            } 

            ResultType = "word";

            if (variable != null)
            {
                variable.addressTaken = true;
                if (variable.isArray) context.ReportError(this, "Can't take address of array.");
            }
        }

        public override Operand GetFetchToken()
        {
            if (variable != null)
            {
                if (variable.type == Model.VariableType.Static)
                    return Label(variable.staticLabel);
                else return null;
            }
            else if (function != null)
                return Label(function.label);
            else if (label != null)
                return Label(label.realName);
            return null;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            IRNode r = new TransientNode();

            if (variable != null)
            {
                if (variable.type == Model.VariableType.Static)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(variable.staticLabel));
                }
                else if (variable.type == Model.VariableType.Local)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("J"));
                    r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek), VariableOffset((ushort)variable.stackOffset));
                }
                else if (variable.type == Model.VariableType.External)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(new Intermediate.Label("EXTERNALS")));
                    r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)variable.constantValue));
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), target.GetOperand(TargetUsage.Peek, OperandSemantics.Dereference));
                }
                else
                    context.ReportError(this, "Can't take the address of this variable.");
            }
            else if (function != null)
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(function.label));
            else if (label != null)
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(label.realName));

            return r;
        }
    }

    
}
