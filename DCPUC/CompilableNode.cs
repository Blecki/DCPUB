using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class CompilableNode : AstNode
    {
        public bool WasFolded = false;
        public string ResultType = "void";

        public virtual void Emit(CompileContext context, Scope scope) {  }
        public virtual void Compile(CompileContext context, Scope scope, Register target) { }
        public virtual int GetConstantValue() { return 0; }
        public virtual string GetConstantToken() { return "0x0000"; }
        public virtual bool IsIntegralConstant() { return false; }
        public virtual string TreeLabel() { return AsString; }

        public CompilableNode Child(int n) { return ChildNodes[n] as CompilableNode; }

        public virtual void GatherSymbols(CompileContext context, Scope enclosingScope) 
        {
            foreach (var child in ChildNodes)
                (child as CompilableNode).GatherSymbols(context, enclosingScope);
        }

        public virtual CompilableNode FoldConstants(CompileContext context)
        {
            var childrenCopy = new AstNodeList();
            foreach (var child in ChildNodes)
            {
                var nChild = (child as CompilableNode).FoldConstants(context);
                if (nChild != null) childrenCopy.Add(nChild);
            }
            ChildNodes.Clear();
            ChildNodes.AddRange(childrenCopy);
            return this;
        }

        public virtual void AssignRegisters(RegisterBank parentState, Register target)
        {

        }

        public virtual int CountRegistersUsed()
        {
            return 0;
        }

        public static Scope BeginBlock(Scope scope)
        {
            return scope.Push(new Scope());
        }

        public static void EndBlock(CompileContext context, Scope scope)
        {
            if (scope.stackDepth - scope.parentDepth > 0) 
                context.Add("ADD", "SP", Hex.hex(scope.stackDepth - scope.parentDepth), "End block");
        }

        /*
        public void InsertLibrary(List<string> library)
        {
            for (int i = 0; i < library.Count; ++i)
            {
                if (library[i].StartsWith(";DCPUC FUNCTION"))
                {
                    //Parse function..
                    var funcHeader = library[i].Split(' ');
                    List<String> funcCode = new List<string>();
                    while (i < library.Count && !library[i].StartsWith(";DCPUC END"))
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    if (i < library.Count)
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    var funcNode = new LibraryFunctionNode();
                    funcNode.AsString = funcHeader[2];
                    funcNode.label = funcHeader[3];
                    funcNode.parameterCount = Convert.ToInt32(funcHeader[4]);
                    funcNode.code = funcCode;
                    ChildNodes.Add(funcNode);
                }
                else if (library[i].StartsWith(";DCPUC STATIC"))
                {
                    List<String> funcCode = new List<string>();
                    while (i < library.Count && !library[i].StartsWith(";DCPUC END"))
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    if (i < library.Count)
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    var funcNode = new LibraryFunctionNode();
                    funcNode.AsString = "%STATICDATA%";
                    funcNode.code = funcCode;
                    ChildNodes.Add(funcNode);
                }
            }
        
        }*/
    }
}
