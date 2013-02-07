using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FontDumper
{
    class Program
    {
        public static string btoa(ushort b)
        {
            var s = "";
            for (int i = 0; i < 16; ++i)
            {
                s = (char)('0' + (b % 2)) + s;
                b >>= 1;
            }
            return s;
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fontdumper font-file out-file");
                    return;
                }

                bool bigEndian = false;
                if (args.Length >= 3 && args[2] == "-be") bigEndian = true;

                var inFile = System.IO.File.OpenRead(args[0]);
                var outFile = System.IO.File.CreateText(args[1]);
                var buffer = new byte[2];
                bool extraLine = false;

                while (inFile.Read(buffer, 0, 2) == 2)
                {
                    var b = (ushort)((buffer[bigEndian ? 0 : 1] << 8) + buffer[bigEndian ? 1 : 0]);
                    var c = btoa(b);
                    outFile.WriteLine(c.Substring(0, 8));
                    outFile.WriteLine(c.Substring(8, 8));
                    if (extraLine)
                    {
                        outFile.WriteLine("");
                        extraLine = false;
                    }
                    else
                        extraLine = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
