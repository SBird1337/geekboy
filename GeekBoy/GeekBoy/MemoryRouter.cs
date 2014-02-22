using System;

namespace GeekBoy
{
    class MemoryRouter : IMemoryDevice
    {
        public Cpu Cpu { get; set; }
        public IMemoryDevice Mbc { get; set; }
        private byte[]  _wram = new byte[0x2000];
        private byte[] _hram = new byte[0x7F];

        public MemoryRouter(IMemoryDevice mbc)
        {
            Mbc = mbc;
        }

        public byte ReadByte(int address)
        {
            if (address <= 0x7FFF)
            {
                return Mbc.ReadByte(address);
            }
            if (address >= 0x8000 && address <= 0x9FFF) { //The first ">=" is redundant and may be removed safely(applys to whole file as far as I can see)
                // HANDLE 8KB VRAM
                return 0;
            }
            if (address >= 0xA000 && address <= 0xBFFF) {
                return Mbc.ReadByte(address);
            }
            if (address >= 0xC000 && address <= 0xDFFF) {
                return _wram[address - 0xC000];
            }
            if (address >= 0xE000 && address <= 0xFDFF) {
                return _wram[address - 0xE000];
            }
            if (address >= 0xFE00 && address <= 0xFE9F) {
                // HANDLE OAM
                return 0;
            }
            if (address >= 0xFEA0 && address <= 0xFEFF) {
                // NOT USABLE 
                return 0;
            }
            if (address >= 0xFF00 && address <= 0xFF7F) { 
                // IO
                Console.WriteLine("DEBUG: IO REQUEST AT 0x{0} (on read)", address.ToString("X"));
                switch (address - 0xFF00)
                {
                    case 0x44:                                  //Switch may be resolved to an if expression
                        return 0x91;
                    default:
                        return 0;
                }
            }
            if (address >= 0xFF80 && address <= 0xFFFE) {
                return _hram[address - 0xFF80];
            }
            if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                return (byte)Cpu.Ie;
            }
            throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on read)");
        }

        public void WriteByte(int address, byte value)
        {
            if (address <= 0x7FFF)
            {
                Mbc.WriteByte(address, value);
            } else if (address >= 0x8000 && address <= 0x9FFF) {
                // HANDLE 8KB VRAM
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                Mbc.WriteByte(address, value);
            } else if (address >= 0xC000 && address <= 0xDFFF) {
                _wram[address - 0xC000] = value;
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
                    default:                                        //Redundant
                        break;
                }
            } else if (address >= 0xFF80 && address <= 0xFFFE) {
                _hram[address - 0xFF80] = value;
            } else if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                Console.WriteLine("IE now set to 0x{0}", value.ToString("X"));
                Cpu.Ie = value;
            } else {
                throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on write)");
            }
        }

    }
}
