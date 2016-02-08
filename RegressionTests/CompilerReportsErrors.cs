using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCPUB.Testing;

namespace RegressionTests
{
    [TestClass]
    public class CompilerReportsErrors
    {
        [TestMethod]
        public void PreprocessorError()
        {
            var test = TestBed.CompileTest("#foobar\n");

            Assert.IsTrue(test.BuildResult.Errors.Count != 0);
        }

        [TestMethod]
        public void CompileError_NonStaticInit()
        {
            var test = TestBed.CompileTest(@"
local foo = 5;
static bar = foo;
");

            Assert.IsTrue(test.BuildResult.Errors.Count != 0);
        }
    }
}
