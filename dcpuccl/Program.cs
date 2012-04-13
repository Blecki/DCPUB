using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUCCL
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DCPUC.CompileOptions();
            DCPUC.CompileOptions.ParseCommandLine(args, options, (s) => { Console.WriteLine(s); });

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
                    context.GatherSymbols();
                    context.FoldConstants();
                    context.Emit(Console.WriteLine);

                    var writer = new System.IO.StreamWriter(options.@out, false);
                    foreach (var instruction in context.instructions)
                        writer.WriteLine(instruction.ToString());
                    writer.Close();

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
