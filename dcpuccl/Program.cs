using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;

namespace DCPUCCL
{
    class Program
    {
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


        static void Main(string[] args)
        {
            var options = new DCPUC.CompileOptions();

            if (args.Length == 0)
            {
                Console.WriteLine(CompileContext.Version);

                return;
            }

            ParseCommandLine(args, options, (s) => { Console.WriteLine(s); });

            if (String.IsNullOrEmpty(options.@in))
            {
                Console.WriteLine("Specify the file to compile with -in \"filename\"");
                return;
            }

            if (String.IsNullOrEmpty(options.@out))
            {
                Console.WriteLine("Specify the file to write to with -out \"filename\"");
                return;
            }

            try
            {
                var file = System.IO.File.ReadAllText(options.@in);
                var context = new DCPUC.CompileContext();
                context.Initialize(options);
                if (context.Parse(file, Console.WriteLine))
                {
                    context.Compile(Console.WriteLine);
                    var assembly = context.Emit(Console.WriteLine);

                    if (options.binary)
                    {
                        var writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(options.@out));
                        var bin = new List<DCPUC.Assembly.Box<ushort>>();
                        assembly.EmitBinary(bin);
                        foreach (var word in bin)
                        {
                            if (options.be)
                                writer.Write((ushort)(
                                    ((word.data & 0x00FF) << 8) + ((word.data & 0xFF00) >> 8)));
                            else
                                writer.Write(word.data);
                        }
                        writer.Close();
                    }
                    else
                    {
                        var writer = new System.IO.StreamWriter(options.@out, false);
                        var stream = new FileEmissionStream(writer);
                        assembly.Emit(stream);
                        writer.Close();
                    }

                    Console.WriteLine("Done.");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

        }
    }
}
