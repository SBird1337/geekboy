using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBoy
{
    class GbBootUp
    {
        public void InitCPU(CPU cpu)
        {
            cpu.A = 0x01;
            cpu.B = 0x00;
            cpu.C = 0x13;
            cpu.D = 0x00;
            cpu.E = 0xD8;
            cpu.H = 0x01;
            cpu.L = 0x4D;
            cpu.SP = 0xFFFE;
            cpu.PC = 0x0100;

            cpu.FLAG_Z = true;
            cpu.FLAG_N = false;
            cpu.FLAG_HC = true;
            cpu.FLAG_C = true;

            cpu.Memory.WriteByte(0xFF05, 0x00);
            cpu.Memory.WriteByte(0xFF06, 0x00);
            cpu.Memory.WriteByte(0xFF07, 0x00);
            cpu.Memory.WriteByte(0xFF10, 0x80);
            cpu.Memory.WriteByte(0xFF11, 0xBF);
            cpu.Memory.WriteByte(0xFF12, 0xF3);
            cpu.Memory.WriteByte(0xFF14, 0xBF);
            cpu.Memory.WriteByte(0xFF16, 0x3F);
            cpu.Memory.WriteByte(0xFF17, 0x00);
            cpu.Memory.WriteByte(0xFF19, 0xBF);
            cpu.Memory.WriteByte(0xFF1A, 0x7F);
            cpu.Memory.WriteByte(0xFF1B, 0xFF);
            cpu.Memory.WriteByte(0xFF1C, 0x9F);
            cpu.Memory.WriteByte(0xFF1E, 0xBF);
            cpu.Memory.WriteByte(0xFF20, 0xFF);
            cpu.Memory.WriteByte(0xFF21, 0x00);
            cpu.Memory.WriteByte(0xFF22, 0x00);
            cpu.Memory.WriteByte(0xFF23, 0xBF);
            cpu.Memory.WriteByte(0xFF24, 0x77);
            cpu.Memory.WriteByte(0xFF25, 0xF3);
            cpu.Memory.WriteByte(0xFF26, 0xF1);
            cpu.Memory.WriteByte(0xFF40, 0x91);
            cpu.Memory.WriteByte(0xFF42, 0x00);
            cpu.Memory.WriteByte(0xFF43, 0x00);
            cpu.Memory.WriteByte(0xFF45, 0x00);
            cpu.Memory.WriteByte(0xFF47, 0xFC);
            cpu.Memory.WriteByte(0xFF48, 0xFF);
            cpu.Memory.WriteByte(0xFF49, 0xFF);
            cpu.Memory.WriteByte(0xFF4A, 0x00);
            cpu.Memory.WriteByte(0xFF4B, 0x00);
            cpu.Memory.WriteByte(0xFFFF, 0x00);
        }
    }
}
