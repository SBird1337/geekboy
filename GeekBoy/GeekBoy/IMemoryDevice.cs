using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBoy
{
    interface IMemoryDevice
    {
        byte ReadByte(int address);
        void WriteByte(int address, byte value);
    }
}
