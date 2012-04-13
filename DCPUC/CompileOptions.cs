using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class CompileOptions
    {
        public bool test = false;
        public string @in = null;
        public string @out = null;

        public static bool ParseCommandLine(string[] arguments, CompileOptions options, Action<string> onError)
        {
            for (int i = 0; i < arguments.Length; )
            {
                var argName = arguments[i];
                ++i;

                if (argName[0] == '-')
                {
                    argName = argName.Substring(1);
                    var field = typeof(CompileOptions).GetField(argName);
                    if (field == null)
                    {
                        onError("Unknown option '" + argName + "'.");
                        return false;
                    }

                    if (field.FieldType == typeof(Boolean))
                        field.SetValue(options, true);
                    else
                    {
                        if (i >= arguments.Length)
                        {
                            onError("Argument required for option '" + argName + "'.");
                            return false;
                        }
                        field.SetValue(options, System.Convert.ChangeType(arguments[i], field.FieldType));
                        ++i;
                    }
                }
                else if (argName[0] == '"')
                {
                    options.@in = argName.Substring(1, argName.Length - 2);
                }
                else
                {
                    onError("Unknown option '" + argName + "'.");
                    return false;
                }
            }

            return true;
        }
    }
}
