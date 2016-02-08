using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB;

namespace DCPUCCL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(CompileContext.Version);
                return;
            }

            try
            {
                var options = new DCPUB.CompileOptions();

                int argumentIndex = 0;
                while (argumentIndex < args.Length)
                {
                    var argument = args[argumentIndex];

                    if (argument == "-nopre")
                    {
                        options.preprocess = false;
                        argumentIndex += 1;
                    }
                    else if (argument == "-b" || argument == "--binary")
                    {
                        options.binary = true;
                        argumentIndex += 1;
                    }
                    else if (argument == "-be" || argument == "--big-endian")
                    {
                        options.be = true;
                        argumentIndex += 1;
                    }
                    else if (argument == "-p" || argument == "--peepholes")
                    {
                        if (argumentIndex == args.Length - 1)
                        {
                            Console.WriteLine("Specify a peephole definition file.");
                            return;
                        }
                        options.peephole = args[argumentIndex + 1];
                        argumentIndex += 2;
                    }
                    else if (argument == "-ir")
                    {
                        options.emit_ir = true;
                        argumentIndex += 1;
                    }
                    else if (argument == "-strip")
                    {
                        options.strip = true;
                        argumentIndex += 1;
                    }
                    else if (argument == "-e" || argument == "--externals")
                    {
                        options.externals = true;
                        argumentIndex += 1;
                    }
                    else if (argument == "-in" || argument == "--input-file")
                    {
                        if (argumentIndex == args.Length - 1)
                        {
                            Console.WriteLine("Specify an input file.");
                            return;
                        }
                        options.@in = args[argumentIndex + 1];
                        argumentIndex += 2;
                    }
                    else if (argument == "-out" || argument == "--output-file")
                    {
                        if (argumentIndex == args.Length - 1)
                        {
                            Console.WriteLine("Specify an output file.");
                            return;
                        }
                        options.@out = args[argumentIndex + 1];
                        argumentIndex += 2;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(options.@in))
                        {
                            options.@in = argument;
                            argumentIndex += 1;
                        }
                        else if (String.IsNullOrEmpty(options.@out))
                        {
                            options.@out = argument;
                            argumentIndex += 1;
                        }
                        else
                            throw new ConfigurationError("Did not understand argument '" + argument + "'");
                    }
                }

                if (String.IsNullOrEmpty(options.@in))
                {
                    Console.WriteLine("Specify an input file. Use -in \"filename\"");
                    return;
                }

                if (String.IsNullOrEmpty(options.@out))
                {
                    if (options.@in == "-")
                        options.@out = "-";
                    else
                        options.@out = System.IO.Path.ChangeExtension(options.@in,
                            options.binary ? "bin" : "dasm");
                }

                if (options.binary && options.@out == "-")
                    throw new ConfigurationError("Binary output and piped output are incompatible.");
                if (!options.binary && options.be)
                    throw new ConfigurationError("I do not know how you expect me to write DASM in big endian.");
                if (options.binary && options.emit_ir)
                    throw new ConfigurationError("Binary output and IR output are incompatible.");

                String file;
                if (options.@in == "-")
                    file = Console.In.ReadToEnd();
                else
                    file = System.IO.File.ReadAllText(options.@in);

                var context = new DCPUB.CompileContext();
                context.Initialize(options);

                if (options.preprocess)
                {
                    var error_count = 0;

                    file = DCPUB.Preprocessor.Parser.Preprocess(options.@in, file, (include_name) =>
                    {
                        if (options.@in == "-") throw new ConfigurationError("Include used when input piped: When input is piped, I don't know where to look for included files.");
                        return System.IO.File.ReadAllText(include_name);
                    },
                    (error_message) =>
                    {
                        Console.WriteLine("%PREPROCESSOR ERROR: {0}", error_message);
                        error_count += 1;
                    });

                    if (error_count != 0) throw new DCPUB.Preprocessor.PreprocessorAbort();
                }

                if (context.Parse(file, Console.WriteLine))
                {
                    var assembly = context.Compile(Console.WriteLine);
                    Console.WriteLine("{0} errors", context.ErrorCount);
                    if (assembly == null) return;

                    if (options.binary)
                    {
                        var writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(options.@out));
                        var bin = new List<DCPUB.Intermediate.Box<ushort>>();
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
                        var writer = options.@out == "-" ? Console.Out : new System.IO.StreamWriter(options.@out, false);
                        var stream = new FileEmissionStream(writer);
                        if (options.@out == "-")
                            stream.WriteLine("@- BEGIN PROGRAM -@");
                        if (options.emit_ir)
                            assembly.EmitIR(stream);
                        else
                            assembly.Emit(stream);
                        if (options.@out == "-")
                            stream.WriteLine("@- END PROGRAM -@");

                        writer.Close();
                    }
                }
            }
            catch (ConfigurationError config_error)
            {
                Console.WriteLine("%CONFIG ERROR: " + config_error.Message);
            }
            catch (DCPUB.Preprocessor.PreprocessorAbort a)
            {
                Console.WriteLine("%PREPROCESSOR ERROR: ABORTED");
            }
        }
    }
}
