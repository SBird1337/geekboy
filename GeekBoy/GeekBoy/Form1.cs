using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeekBoy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("DEBUG: MainForm launched.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ROM rom;
            MemoryRouter mr;
            GbBootUp boot;
            CPU cpu;

            try
            {
                // Load ROM
                rom = new ROM(@"I:\rom.gb", "");

                // Setup MemoryRouter
                mr = new MemoryRouter(rom.Memory);

                // Setup CPU
                cpu = new CPU(mr);

                // Give the MemoryRouter access to the CPU;
                mr.CPU = cpu;

                // BOOT!
                boot = new GbBootUp();
                boot.InitCPU(cpu);

                int time1 = Environment.TickCount;
                int ops = 0;

                while (cpu.Running)
                {
                    //Console.WriteLine("AF: 0x{0} BC: 0x{1} DE: 0x{2}, HL: 0x{3} PC: 0x{4} SP: 0x{5}, Z: {6} N: {7} H: {8} C: {9}",
                    //                   ((cpu.A << 8) + cpu.F).ToString("X"),
                    //                   ((cpu.B << 8) + cpu.C).ToString("X"),
                    //                   ((cpu.D << 8) + cpu.E).ToString("X"),
                    //                   ((cpu.H << 8) + cpu.L).ToString("X"),
                    //                   cpu.PC.ToString("X"),
                    //                   cpu.SP.ToString("X"),
                    //                   cpu.FLAG_Z, cpu.FLAG_N, cpu.FLAG_HC, cpu.FLAG_C);
                    cpu.ExecuteOP();
                    ops++;
                
                    if (Environment.TickCount > time1 + 1000)
                    {
                        Console.Title = ops.ToString();
                        ops = 0;
                        time1 = Environment.TickCount;
                        //Application.DoEvents();
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nException: {0}", ex.Message);
            }
        }
    }
}
