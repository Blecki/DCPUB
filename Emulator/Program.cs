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
                emu.devices.Add(lem);

                bool running = true;
                var emulationThread = new System.Threading.Thread(
                    () =>
                    {
                        try
                        {
                            while (true)
                            {
                                //Console.ReadKey(true);
                                //var _pc = emu.registers[(int)DCPUC.Emulator.Registers.PC];
                                //Console.WriteLine(emu.Disassemble(ref _pc));
                                emu.Step();
                                //Console.WriteLine(emu.debug);
                            }
                        }
                        catch (Exception e)
                        {
                            //Console.Write(emu.debug);
                            Console.WriteLine(e.Message);
                        }

                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey(true);
                        running = false;
                    });

                emulationThread.Start();

                while (running) Application.DoEvents();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }


        }
    }
}
