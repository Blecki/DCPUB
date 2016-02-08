using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public class BuildResult
    {
        public List<String> Errors = new List<string>();
        public Intermediate.IRNode Assembly;
    }

    public class Build
    {
        public static BuildResult BuildForTest(String Code)
        {
            var result = new BuildResult();

            var context = new DCPUB.CompileContext();
            context.Initialize(new CompileOptions());
                        
            var file = DCPUB.Preprocessor.Parser.Preprocess("", Code, (include_name) =>
                {
                    throw new InvalidOperationException("Can't include in test.");
                },
                (error_message) =>
                {
                    result.Errors.Add(String.Format("%PREPROCESSOR ERROR: {0}", error_message));
                });

            if (result.Errors.Count == 0)
                if (context.Parse(Code, (str) => result.Errors.Add(str)))
                    result.Assembly = context.Compile((str) => result.Errors.Add(str));

            return result;
        }

        public static ushort[] AsLoadableBinary(Intermediate.IRNode Node)
        {
            var bin = new List<Intermediate.Box<ushort>>();
            Node.EmitBinary(bin);
            return bin.Select(b => b.data).ToArray();
        }
    }
}
