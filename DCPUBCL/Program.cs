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

            var options = new DCPUB.CompileOptions();
            bool vStyle = false;
            bool emitIR = false;

            int argumentIndex = 0;
            while (argumentIndex < args.Length)
            {
                var argument = args[argumentIndex];

                if (argument == "-i" || argument == "--ir")
                {
                    emitIR = true;
                    argumentIndex += 1;
                }
                else if (argument == "-v" || argument == "--vstyle")
                {
                    options.skip_virtual_register_assignment = true;
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
                    {
                        Console.WriteLine("Did not understand argument '" + argument + "'");
                        return;
                    }
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
                {
                    Console.WriteLine("Specify an output file. Use -out \"filename\"");
                    return;
                }

                options.@out = System.IO.Path.ChangeExtension(options.@in,
                    options.binary ? "bin" : "dasm");
            }

            //try
            //{
                String file;
                if (options.@in == "-")
                    file = Console.In.ReadToEnd();
                else
                    file = System.IO.File.ReadAllText(options.@in);

                var context = new DCPUB.CompileContext();
                context.Initialize(options);
                if (context.Parse(file, Console.WriteLine))
                {

                    if (options.skip_virtual_register_assignment) //Ignore binary flag.
                    {
                        var assembly = context.Compile(Console.WriteLine);
                        var writer = new System.IO.StreamWriter(options.@out, false);
                        var stream = new FileEmissionStream(writer);
                        if (assembly != null)
                        {
                            if (emitIR) assembly.EmitIR(stream);
                            else assembly.Emit(stream);
                        }
                        writer.Close();
                    }
                    else
                    {
                        var assembly = context.Compile(Console.WriteLine);

                        if (options.binary)
                        {
                            var writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(options.@out));
                            var bin = new List<DCPUB.Assembly.Box<ushort>>();
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
                            if (emitIR) assembly.EmitIR(stream);
                            else assembly.Emit(stream);
                            writer.Close();
                        }
                    }
                }

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return;
            //}

        }
    }
}
