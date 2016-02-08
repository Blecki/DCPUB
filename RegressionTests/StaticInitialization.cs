using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCPUB.Testing;

namespace RegressionTests
{
    [TestClass]
    public class StaticInitialization
    {
        [TestMethod]
        public void StaticInitialization_Simple()
        {
            var test = TestBed.CompileTest(@"static foo = 'a'; __out(foo);", TestHelperIncludes.TeleTypeTerminal);
            TestBed.Emulate(test);

            Assert.IsTrue(test.Completed);
            Assert.AreEqual(test.TeletypeOutputAsString, "a");
        }
    }
}
