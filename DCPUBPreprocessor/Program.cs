﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;

namespace DCPUBPreprocessor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                WriteHelp();
                return;
            }

            try
            {
                var file = System.IO.File.ReadAllText(args[0]);
                var processedFile = DCPUC.Preprocessor.Parser.Preprocess(file, (str) =>
                    { return System.IO.File.ReadAllText(str); });
                System.IO.File.WriteAllText(args[1], processedFile);
                Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        static void WriteHelp()
        {
            Console.WriteLine("DCPUB Preprocessor 1.0");
            Console.WriteLine("Requires two arguments. Input file first, output filename second.");
        }
    }
}
