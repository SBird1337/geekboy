using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GeekBoy
{

    class MBC3 : IMemoryDevice
    {
        private MBC romType;
        private int selectedRomBank = 1;
        private int selectedRamBank = 0;
        private byte[,] ram; //new byte[4, 0x4000];
        private byte[,] rom;
        private bool ram_timer_enable = false;

        public MBC3(byte[] fileData, MBC romType, int romSize)
        {
            this.romType = romType;
            int bankSize = 0x4000;
            int romBanks = romSize / bankSize;
            rom = new byte[romBanks, bankSize];
            this.ram = this.ReadSave();
            for (int i = 0, k = 0; i < romBanks; i++)
            {
                for (int j = 0; j < bankSize; j++, k++)
                {
                    rom[i, j] = fileData[k];
                }
            }
            Console.WriteLine("DEBUG: Init OK");
        }

        public byte ReadByte(int address)
        {
            if (address >= 0 && address <= 0x3FFF)
            {
                return rom[0, address];
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                return rom[selectedRomBank, address - 0x4000];
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (ram_timer_enable && selectedRamBank < 4) return ram[selectedRamBank, address - 0xA000];
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
                ram_timer_enable = value == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                selectedRomBank = value & 0x7F;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                selectedRamBank = value;
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                // IMPLEMENT RTC HERE
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (ram_timer_enable && selectedRamBank < 4) ram[selectedRamBank, address - 0xA000] = (byte)value;
                WriteByteToSave(selectedRamBank * 0x4000 + (address - 0xA000), value);
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
