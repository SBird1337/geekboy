using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeekBoy
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Console Introdution
            Console.Title = "GeekBoy GameBoy Emulator [DEBUG]";
            Console.WriteLine("Welcome to GeekBoy the GameBoy emulator written by geeks for geeks.\n");
            Console.WriteLine("This is the debug console. You will find helpful debugging information here.\n");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
