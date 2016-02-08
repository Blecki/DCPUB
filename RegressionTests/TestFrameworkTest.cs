using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCPUB.Testing;

namespace RegressionTests
{
    [TestClass]
    public class TestFrameworkTest
    {
        [TestMethod]
        public void HelloWorld()
        {
            var test = TestBed.CompileTest(@"__out_str(""Hello World!"");", TestHelperIncludes.TeleTypeTerminal);
            TestBed.Emulate(test);

            Assert.IsTrue(test.Completed);
            Assert.AreEqual(test.TeletypeOutputAsString, "Hello World!");
        }

        [TestMethod]
        public void Basic()
        {
            var test = TestBed.CompileTest("");
            TestBed.Emulate(test);

            Assert.IsTrue(test.Completed);
        }
    }
}
