using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBoy
{
    class MemoryRouter : IMemoryDevice
    {
        public CPU CPU;
        public IMemoryDevice MBC;
        private byte[]  WRAM = new byte[0x2000];
        private byte[] HRAM = new byte[0x7F];

        public MemoryRouter(IMemoryDevice mbc)
        {
            this.MBC = mbc;
        }

        public byte ReadByte(int address)
        {
            if (address <= 0x7FFF)
            {
                return this.MBC.ReadByte(address);
            } else if (address >= 0x8000 && address <= 0x9FFF) {
                // HANDLE 8KB VRAM
                return 0;
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                return this.MBC.ReadByte(address);
            } else if (address >= 0xC000 && address <= 0xDFFF) {
                return this.WRAM[address - 0xC000];
            } else if (address >= 0xE000 && address <= 0xFDFF) {
                return this.WRAM[address - 0xE000];
            } else if (address >= 0xFE00 && address <= 0xFE9F) {
                // HANDLE OAM
                return 0;
            } else if (address >= 0xFEA0 && address <= 0xFEFF) {
                // NOT USABLE 
                return 0;
            } else if (address >= 0xFF00 && address <= 0xFF7F) { 
                // IO
                Console.WriteLine("DEBUG: IO REQUEST AT 0x{0} (on read)", address.ToString("X"));
                switch (address - 0xFF00)
                {
                    case 0x44:
                        return 0x91;
                    default:
                        return 0;
                }
            } else if (address >= 0xFF80 && address <= 0xFFFE) {
                return this.HRAM[address - 0xFF80];
            } else if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                return (byte)this.CPU.IE;
            }
            throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on read)");
        }

        public void WriteByte(int address, byte value)
        {
            if (address <= 0x7FFF)
            {
                this.MBC.WriteByte(address, value);
            } else if (address >= 0x8000 && address <= 0x9FFF) {
                // HANDLE 8KB VRAM
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                this.MBC.WriteByte(address, value);
            } else if (address >= 0xC000 && address <= 0xDFFF) {
                this.WRAM[address - 0xC000] = value;
            } else if (address >= 0xE000 && address <= 0xFDFF) { 
                // NOP
            }else if (address >= 0xFE00 && address <= 0xFE9F) {
                // HANDLE OAM
            } else if (address >= 0xFEA0 && address <= 0xFEFF) {
                // NOT USABLE 
            } else if (address >= 0xFF00 && address <= 0xFF7F) { 
                // IO
                Console.WriteLine("DEBUG: IO REQUEST AT 0x{0} (on write)", address.ToString("X"));
                switch (address - 0xFF00)
                {
                    default:
                        break;
                }
            } else if (address >= 0xFF80 && address <= 0xFFFE) {
                this.HRAM[address - 0xFF80] = value;
            } else if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                Console.WriteLine("IE now set to 0x{0}", value.ToString("X"));
                this.CPU.IE = value;
            } else {
                throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on write)");
            }
        }

    }
}
