using System;
using System.IO;

namespace GeekBoy
{
    internal enum Mbc //Enum names are inconsistantly named IMO but since it somehow helps the readability I left them as is
    {
        ROM_NONE = 0x00,
        ROM_MBC1 = 0x01,
        ROM_MBC1_RAM = 0x02,
        ROM_MBC1_RAM_BATT = 0x03,
        ROM_MBC2 = 0x05,
        ROM_MBC2_BATT = 0x06,
        ROM_RAM = 0x08,
        ROM_RAM_BATT = 0x09,
        ROM_MMM01 = 0x0B,
        ROM_MMM01_RAM = 0x0C,
        ROM_MMM01_RAM_BATT = 0x0D,
        ROM_MBC3_TIMER_BATT = 0x0F,
        ROM_MBC3_TIMER_RAM_BATT = 0x10,
        ROM_MBC3 = 0x11,
        ROM_MBC3_RAM = 0x12,
        ROM_MBC3_RAM_BATT = 0x13,
        ROM_MBC4 = 0x15,
        ROM_MBC4_RAM = 0x16,
        ROM_MBC4_BATT = 0x17,
        ROM_MBC5 = 0x19,
        ROM_MBC5_RAM = 0x1A,
        ROM_MBC5_RAM_BATT = 0x1B,
        ROM_MBC5_RUMBLE = 0x1C,
        ROM_MBC5_RUMBLE_RAM = 0x1D,
        ROM_MBC5_RUMBLE_RAM_BATT = 0x1E,
        ROM_POCKET_CAMERA = 0xFC,
        ROM_BANDAI_TAMA5 = 0xFD,
        ROM_HUC3 = 0xFE,
        ROM_HUC1_RAM_BATT = 0xFF
    }

    internal class Rom
    {
        public Mbc CartridgeType = Mbc.ROM_NONE;
        public bool Cgb = false;
        public bool CgbOnly = false;
        public bool Japanese = false;

        public IMemoryDevice Memory;
        public int RamSize = 0;
        public int RomSize = 0;
        public bool SgbSupport = false;
        public string Title = string.Empty;

        public Rom(string path, string save = "")
        {
            byte[] data = File.ReadAllBytes(path);
            byte[] save_data; //Not used -> redundant

            if (save != string.Empty)
                save_data = File.ReadAllBytes(save);

            Console.WriteLine("DEBUG: Loading ROM");

            // Read ROM name
            for (int i = 0; i < 16; i++)
                Title += (char) data[0x134 + i];

            Console.WriteLine("       ROMNAME={0}", Title);

            // Game does support CGB features?
            Cgb = data[0x143] == 0x80 || data[0x143] == 0xC0;

            // Is the cartridge CGB only?
            CgbOnly = data[0x143] == 0xC0;

            Console.WriteLine("       CGB_SUPPORT={0}", Cgb);
            Console.WriteLine("       CGB_ONLY={0}", Cgb);

            // Does the game support SGB features?
            SgbSupport = data[0x146] == 0x03;

            Console.WriteLine("       SGB_SUPPORT={0}", SgbSupport);

            // Cartridge Type
            CartridgeType = (Mbc) data[0x147];

            Console.WriteLine("       MBC={0}", CartridgeType);

            // ROM size
            RomSize = 32000 << data[0x148];

            Console.WriteLine("       ROMSIZE={0}KB", RomSize/1000);

            // RAM size
            switch (data[0x149])
            {
                case 0x0:
                    RamSize = 0;
                    break;
                case 0x1:
                    RamSize = 2048;
                    break;
                case 0x2:
                    RamSize = 8192;
                    break;
                case 0x03:
                    RamSize = 32768;
                    break;
            }

            Console.WriteLine("       RAMSIZE={0}KB", RamSize/1000);

            Japanese = data[0x14A] == 0x00;

            Console.WriteLine("       JAPANESE={0}", Japanese);

            // Init MBC
            Console.WriteLine("DEBUG: Initialize MBC");
            switch (CartridgeType)
            {
                case Mbc.ROM_MBC3_RAM:
                case Mbc.ROM_MBC3_RAM_BATT:
                case Mbc.ROM_MBC3_TIMER_BATT:
                case Mbc.ROM_MBC3_TIMER_RAM_BATT:
                    Memory = new Mbc3(data, CartridgeType, RomSize);
                    break;
                default:
                    throw new Exception("Unsupported cartridge type " + CartridgeType);  //May be replaced with internally defined Exception derived object.
            }
        }
    }
}