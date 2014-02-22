namespace GeekBoy
{
    interface IMemoryDevice
    {
        byte ReadByte(int address);
        void WriteByte(int address, byte value);
    }
}
