using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using System.Windows.Forms;

namespace Emulator
{
    class Options
    {
        public string @in;
        public String dis = null;
    }

     class DisEntry
     {
                    public String str;
                    public int size;
     }

    class Program
    {
        public static bool ParseCommandLine(string[] arguments, Options options, Action<string> onError)
        {
            for (int i = 0; i < arguments.Length; )
            {
                var argName = arguments[i];
                ++i;

                if (argName[0] == '-')
                {
                    argName = argName.Substring(1);
                    var field = typeof(Options).GetField(argName);
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

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            var options = new Options();

            if (args.Length == 0)
            {
                Console.WriteLine(CompileContext.Version);

                return;
            }

            ParseCommandLine(args, options, (s) => { Console.WriteLine(s); });

            if (String.IsNullOrEmpty(options.@in))
            {
                Console.WriteLine("Specify the file to run with -in \"filename\"");
                return;
            }

            try
            {
                var file = System.IO.File.ReadAllBytes(options.@in);
                var shorts = new ushort[file.Length / 2];
                for (int i = 0; i < shorts.Length; ++i)
                    shorts[i] = (ushort)((int)file[i * 2] + (int)(file[(i * 2) + 1] << 8));
                Console.WriteLine(String.Join(" ", shorts.Select((u) => { return DCPUC.Hex.hex(u); })));
                var emu = new DCPUC.Emulator.Emulator();
                emu.Load(shorts);
                var lem = new DCPUC.Emulator.LEM1802(emu);
                emu.AttachDevice(lem);

               
                String[] disassembly = null;
                if (!String.IsNullOrEmpty(options.dis)) 
                {
                    disassembly = new String[shorts.Length];
                    for (int i = 0; i < shorts.Length; ++i)
                        disassembly[i] = "DAT " + Hex.hex(shorts[i]);
                }

                bool running = true;
                var emulationThread = new System.Threading.Thread(
                    () =>
                    {
                        try
                        {
                            bool paused = false;
                            while (!paused)
                            {
                                if (Console.KeyAvailable)
                                {
                                    var key = Console.ReadKey(true);
                                    paused = true;
                                }

                                if (!String.IsNullOrEmpty(options.dis))
                                {
                                    var _pc = emu.registers[(int)DCPUC.Emulator.Registers.PC];
                                    var start = _pc;
                                    var str = emu.Disassemble(ref _pc);

                                    for (int i = start; i < _pc; ++i)
                                        disassembly[i] = null;

                                    disassembly[start] = str.Substring(7);
                                }
                                emu.Step();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey(true);
                        running = false;
                    });

                emulationThread.Start();

                while (running) Application.DoEvents();

                if (!String.IsNullOrEmpty(options.dis))
                {
                    var writer = System.IO.File.OpenWrite(options.dis);
                    var stream = new System.IO.StreamWriter(writer);
                    foreach (var str in disassembly)
                        if (str != null) stream.WriteLine(str);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }


        }
    }
}
