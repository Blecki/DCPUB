﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB;

namespace DCPUBPreprocessor
{
    class Program
    {
        static void Main(string[] args)
        {
            bool useStandardOut = false;
            if (args.Length == 1)
            {
                useStandardOut = true;
            }
            else if (args.Length != 2)
            {
                WriteHelp();
                return;
            }

            try
            {
                var file = System.IO.File.ReadAllText(args[0]);
                var processedFile = DCPUB.Preprocessor.Parser.Preprocess(file, (str) =>
                    { return System.IO.File.ReadAllText(str); });
                if (useStandardOut)
                    Console.Out.Write(processedFile);
                else
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
