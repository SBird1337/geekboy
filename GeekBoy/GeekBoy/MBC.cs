using System;
using System.IO;

namespace GeekBoy
{

    class Mbc3 : IMemoryDevice
    {
        private const int BANK_SIZE = 0x4000;

        private Mbc _romType;
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private byte[,] _ram; //new byte[4, 0x4000];
        private byte[,] _rom;
        private bool _ramTimerEnable = false;

        public Mbc3(byte[] fileData, Mbc romType, int romSize)
        {
            _romType = romType;
            int romBanks = romSize / BANK_SIZE;
            _rom = new byte[romBanks, BANK_SIZE];
            _ram = ReadSave();
            for (int i = 0, k = 0; i < romBanks; i++)
            {
                for (int j = 0; j < BANK_SIZE; j++, k++)
                {
                    _rom[i, j] = fileData[k];
                }
            }
            Console.WriteLine("DEBUG: Init OK");
        }

        public byte ReadByte(int address)
        {
            if (address >= 0 && address <= 0x3FFF)
            {
                return _rom[0, address];
            }
            if (address >= 0x4000 && address <= 0x7FFF)
            {
                return _rom[_selectedRomBank, address - 0x4000];
            }
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramTimerEnable && _selectedRamBank < 4) return _ram[_selectedRamBank, address - 0xA000];
                // IMPLEMENT RTC HERE
                return 0;
            }
            Console.WriteLine("MBC3: Read from address 0x{0}", address.ToString("X"));
            throw new Exception(string.Format("Invalid cartridge address: {0}", address));
        }

        public void WriteByte(int address, byte value)
        {
            if (address >= 0x0 && address <= 0x1FFF)
            {
                _ramTimerEnable = value == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                _selectedRomBank = value & 0x7F;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                _selectedRamBank = value;
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                // IMPLEMENT RTC HERE
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramTimerEnable && _selectedRamBank < 4) _ram[_selectedRamBank, address - 0xA000] = (byte)value;
                WriteByteToSave(_selectedRamBank * 0x4000 + (address - 0xA000), value);
                // IMPLEMENT RTC HERE
            }
        }

        private byte[,] ReadSave()
        {
            byte[,] saveGame = new byte[4, 0x2000];
            if (File.Exists("save.sav"))
            {
                byte[] data = File.ReadAllBytes("save.sav");
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 0x2000; j++)
                        saveGame[i, j] = data[i * 0x2000 + j];
                }
            }
            return saveGame;
        }

        private void WriteByteToSave(int address, int value)
        {
            Stream s = File.Open("save.sav", FileMode.OpenOrCreate);

            if (s.Length < 65536)
            {
                for (int i = 0; i < 65536; i++)
                    s.WriteByte(0);
            }

            s.Position = address;
            s.WriteByte((byte)value);
            s.Close();
        }

    }


}
