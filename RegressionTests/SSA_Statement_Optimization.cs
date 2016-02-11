using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCPUB.Testing;
using DCPUB.Intermediate;
using DCPUB.Ast;
using DCPUB.Model;

namespace RegressionTests
{
    [TestClass]
    public class SSA_Statement_Optimization
    {
        [TestMethod]
        public void SSA_Statements_Merged()
        {
            var root = new IRNode();

            var statement_one = new StatementNode();
            statement_one.AddInstruction(Instructions.SET, CompilableNode.Virtual(0), CompilableNode.Operand(Register.A));
            root.AddChild(statement_one);

            var statement_two = new StatementNode();
            statement_two.AddInstruction(Instructions.ADD, CompilableNode.Virtual(0), CompilableNode.Constant(5));
            root.AddChild(statement_two);

            Assert.IsTrue(root.children.Count == 2);

            root.MergeConsecutiveStatements();

            Assert.IsTrue(root.children.Count == 1);
        }

        [TestMethod]
        public void SSA_Statement_Optimized()
        {
            /*              Should become ==>
                SET R0, [0x0002+J]     SET R0, [0x0002+J]
                SET R0, [0x0001+R0]    ADD [0x0001+R0], [0x0003+R0]
                SET R1, [0x0002+J]
                SET R1, [0x0003+R1]
                ADD R0, R1
                SET R2, [0x0002+J]
                SET [0x0001+R2], R0
            */

            var statement = new StatementNode();
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(0), CompilableNode.DereferenceVariableOffset(2));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(0), CompilableNode.Virtual(0, OperandSemantics.Dereference | OperandSemantics.Offset, 1));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(1), CompilableNode.DereferenceVariableOffset(2));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(1), CompilableNode.Virtual(1, OperandSemantics.Dereference | OperandSemantics.Offset, 3));
            statement.AddInstruction(Instructions.ADD, CompilableNode.Virtual(0), CompilableNode.Virtual(1));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(2), CompilableNode.DereferenceVariableOffset(2));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(2, OperandSemantics.Dereference | OperandSemantics.Offset, 1), CompilableNode.Virtual(0));

            Assert.IsTrue(statement.children.Count == 7);
            Assert.IsTrue(statement.children[0].ToString() == "SET VR0, [0x0002+J]");
            Assert.IsTrue(statement.children[1].ToString() == "SET VR0, [0x0001+VR0]");
            Assert.IsTrue(statement.children[2].ToString() == "SET VR1, [0x0002+J]");
            Assert.IsTrue(statement.children[3].ToString() == "SET VR1, [0x0003+VR1]");
            Assert.IsTrue(statement.children[4].ToString() == "ADD VR0, VR1");
            Assert.IsTrue(statement.children[5].ToString() == "SET VR2, [0x0002+J]");
            Assert.IsTrue(statement.children[6].ToString() == "SET [0x0001+VR2], VR0");

            statement.ApplySSA();

            Assert.IsTrue(statement.children.Count == 2);
            Assert.IsTrue(statement.children[0].ToString() == "SET VR0, [0x0002+J]");
            Assert.IsTrue(statement.children[1].ToString() == "ADD [0x0001+VR0], [0x0003+VR0]");
        }


        [TestMethod]
        public void SSA_Dependent_Operand_Not_Clobbered()
        {
            /*  
                SET R0, [0x0002+J]
                SET R1, [0x0003+J]  // Optimizing this wrong will cause the final set to have the wrong value
                ADD R1, [0x0001+R0]      
                SET [0x0003+J], R1
                SET [0x0002+J], [0x0005+R1] // Here is is NOT safe to eliminate R1.
            */

            var statement = new StatementNode();
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(0), CompilableNode.DereferenceVariableOffset(2));
            statement.AddInstruction(Instructions.SET, CompilableNode.Virtual(1), CompilableNode.DereferenceVariableOffset(3));
            statement.AddInstruction(Instructions.ADD, CompilableNode.Virtual(1), CompilableNode.Virtual(0, OperandSemantics.Dereference | OperandSemantics.Offset, 1));
            statement.AddInstruction(Instructions.SET, CompilableNode.DereferenceVariableOffset(3), CompilableNode.Virtual(1));
            statement.AddInstruction(Instructions.SET, CompilableNode.DereferenceVariableOffset(2), CompilableNode.Virtual(1, OperandSemantics.Dereference | OperandSemantics.Offset, 5));

            Assert.IsTrue(statement.children.Count == 5);
            Assert.IsTrue(statement.children[0].ToString() == "SET VR0, [0x0002+J]");
            Assert.IsTrue(statement.children[1].ToString() == "SET VR1, [0x0003+J]");
            Assert.IsTrue(statement.children[2].ToString() == "ADD VR1, [0x0001+VR0]");
            Assert.IsTrue(statement.children[3].ToString() == "SET [0x0003+J], VR1");
            Assert.IsTrue(statement.children[4].ToString() == "SET [0x0002+J], [0x0005+VR1]");

            statement.ApplySSA();

            Assert.IsTrue(statement.children.Count == 4);
            Assert.IsTrue(statement.children[0].ToString() == "SET VR0, [0x0002+J]");
            Assert.IsTrue(statement.children[1].ToString() == "ADD [0x0003+J], [0x0001+VR0]");
            Assert.IsTrue(statement.children[2].ToString() == "SET VR2, [0x0003+J]");
            Assert.IsTrue(statement.children[3].ToString() == "SET [0x0002+J], [0x0005+VR2]");
        }
    }
}
