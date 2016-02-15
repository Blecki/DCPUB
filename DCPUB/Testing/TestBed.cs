using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCPUB.Testing
{
    public class TestResult
    {
        public Emulator Emulator = null;
        public TeleTerminalHardware Teletype = null;
        public DCPUB.BuildResult BuildResult = null;
        public bool Completed { get; internal set; }
        public Exception Exception;
        public String IntermediateRepresentation;

        public String TeletypeOutputAsString
        {
            get
            {
                return new string(Teletype.Output.Select(c => (char)c).ToArray());
            }
        }
    }

    class StringEmissionStream : EmissionStream
        {
            public StringBuilder Builder = new StringBuilder();

            public override void WriteLine(string line)
            {
                Builder.AppendLine(line);
            }
        }

    public enum TestHelperIncludes
    {
        None = 0,
        TeleTypeTerminal = 1,
    }

    public class TestBed
    { 
        public static TestResult CompileTest(String Code, TestHelperIncludes Includes = TestHelperIncludes.None)
        {
            var result = new TestResult();

            if ((Includes & TestHelperIncludes.TeleTypeTerminal) == TestHelperIncludes.TeleTypeTerminal)
                Code = IncludeTeletypeLib(Code);

            result.Completed = false;

            try
            {
                result.BuildResult = DCPUB.Build.BuildForTest(Code);
            }
            catch (Exception e)
            {
                result.Exception = e;
            }

            return result;
        }

        public static void EmitIR(TestResult Test)
        {
            if (Test.BuildResult != null && Test.BuildResult.Assembly != null)
            {
                var stream = new StringEmissionStream();
                Test.BuildResult.Assembly.EmitIR(stream, false);
                Test.IntermediateRepresentation = stream.Builder.ToString();
            }
        }

        public static void Emulate(TestResult Test)
        { 
            try
            { 
            if (Test.BuildResult != null && Test.BuildResult.Assembly != null)
            { 
            Test.Emulator = new Emulator();
                    Test.Teletype = new TeleTerminalHardware();
                    Test.Emulator.AttachDevice(Test.Teletype);
                    Test.Emulator.Load(DCPUB.Build.AsLoadableBinary(Test.BuildResult.Assembly));

                    try
                    {
                        while (true)
                            Test.Emulator.Step();
                    }
                    catch (Halt)
                    {
                        Test.Completed = true;
                    }
                }
            }
            catch (Exception e)
            {
                Test.Exception = e;
            }
        }

        /// <summary>
        /// Prepend a tiny library for dealing with the TeleTerminalHardware to the code.
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public static String IncludeTeletypeLib(String Code)
        {
            return @"
function __out(c)
{
    asm (B = c)
    {
        HWI 0x0000
    }
}

function __out_str(s)
{
    local length = *s;
    local spot = s + 1;
    local end = spot + length;
    while (spot < end)
    {
        __out(*spot);
        spot += 1;
    }
}

"
                + Code;
        }
    }
}
