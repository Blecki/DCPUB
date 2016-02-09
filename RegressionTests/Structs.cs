using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCPUB.Testing;

namespace RegressionTests
{
    [TestClass]
    public class Structs
    {
        [TestMethod]
        public void Structs_OffsetOf()
        {
            var test = TestBed.CompileTest(@"
struct s
{   
    foo;
    bar;
}

local _s:s[sizeof s];
_s.bar = 'g';

__out(*(_s + (offsetof bar in s)));
", TestHelperIncludes.TeleTypeTerminal);
            TestBed.Emulate(test);

            Assert.IsTrue(test.Completed);
            Assert.AreEqual(test.TeletypeOutputAsString, "g");
        }
    }
}
