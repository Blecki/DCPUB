using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DCPUCIDE
{
    static class Program
    {
        private static int fib(int n)
        {
            if (n == 0) return 0;
            if (n == 1) return 1;
            return fib(n - 1) + fib(n - 2);
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var x = fib(6);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
