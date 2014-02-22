using System;

namespace GeekBoy
{
    internal class Cpu
    {
        // Registers
        private int _a, _b, _c, _d, _e, _f, _h, _l;

        public int A
        {
            get { return _a; }
            set { _a = value; }
        }
        public int B
        {
            get { return _b; }
            set { _b = value; }
        }
        public int C
        {
            get { return _c; }
            set { _c = value; }
        }
        public int D
        {
            get { return _d; }
            set { _d = value; }
        }
        public int E
        {
            get { return _e; }
            set { _e = value; }
        }
        public int F
        {
            get { return _f; }
            set { _f = value; }
        }
        public bool FlagC { get; set; } // Zero, Substract, HalfCarry, Carry
        public bool FlagHc { get; set; } // Zero, Substract, HalfCarry, Carry
        public bool FlagN { get; set; } // Zero, Substract, HalfCarry, Carry
        public bool FlagZ { get; set; } // Zero, Substract, HalfCarry, Carry
        public int H
        {
            get { return _h; }
            set { _h = value; }
        }
        public int Ie { get; set; }
        public bool Ime { get; set; } // Interrupt Enable
        public int L
        {
            get { return _l; }
            set { _l = value; }
        }
        public IMemoryDevice Memory { get; set; }
        public int Pc { get; set; }

        // Flags
        public bool Running = true;
        public int Sp;

        // Tables
        //private int[] CycleTable = { 4, 12, 8, 8, 4, 4, 8, 4, 20, 8, 8, 8, 4, 4, 8, 4,
        //                             4, 12, 8, 8, 4, 4, 8, 4, 12, 8, 8, 8, 4, 4, 8, 4};

        private bool _waitForInterrupt;

        public Cpu(IMemoryDevice memory)
        {
            Memory = memory;
            Ie = 0;
            Ime = false;
        }

        public void ExecuteOp()
        {
            byte op = ReadByte(Pc);

            if (!Running || _waitForInterrupt) return;

            _f = (FlagZ ? 0x80 : 0) + (FlagN ? 0x40 : 0) + (FlagHc ? 0x20 : 0) + (FlagC ? 0x10 : 0);

            switch (op)
            {
                case 0x00: // NOP
                    Pc++;
                    break;
                case 0x01: // LD BC, d16
                    LD_R16_D16(ref _b, ref _c);
                    break;
                case 0x02: // LD (BC), A
                    LD_PR16_R8(_b, _c, _a);
                    break;
                case 0x03: // INC BC
                    INC_R16(ref _b, ref _c);
                    break;
                case 0x04: // INC B
                    INC_R8(ref _b);
                    break;
                case 0x05: // DEC B
                    DEC_R8(ref _b);
                    break;
                case 0x06: // LD B, D8
                    LD_R8_D8(ref _b);
                    break;
                case 0x07: // RLCA
                    Rlca();
                    break;
                case 0x08: // LD (A16), SP
                    LD_A16_R16((Sp >> 8) & 0xFF, Sp & 0xFF);
                    break;
                case 0x09: // ADD HL, BC
                    ADD_R16_R16(ref _h, ref _l, _b, _c);
                    break;
                case 0x0A: // LD A, (BC)
                    LD_R8_PR16(ref _a, _b, _c);
                    break;
                case 0x0B: // DEC BC
                    DEC_R16(ref _b, ref _c);
                    break;
                case 0x0C: // INC C
                    INC_R8(ref _c);
                    break;
                case 0x0D: // DEC C
                    DEC_R8(ref _c);
                    break;
                case 0x0E: // LD C, D8
                    LD_R8_D8(ref _c);
                    break;
                case 0x0F: // RRCA
                    Rrca();
                    break;
                case 0x10: // STOP 0
                    Running = false;
                    Pc += 2;
                    break;
                case 0x11: // LD DE, D16
                    LD_R16_D16(ref _d, ref _e);
                    break;
                case 0x12: // LD (DE), A
                    LD_PR16_R8(_d, _e, _a);
                    break;
                case 0x13: // INC DE
                    INC_R16(ref _d, ref _e);
                    break;
                case 0x14: // INC D
                    INC_R8(ref _d);
                    break;
                case 0x15: // DEC D
                    DEC_R8(ref _d);
                    break;
                case 0x16: // LD D, D8
                    LD_R8_D8(ref _d);
                    break;
                case 0x17: // RLA 
                    Rla();
                    break;
                case 0x18: // JR S8
                    JR_S8();
                    break;
                case 0x19: // ADD HL, DE
                    ADD_R16_R16(ref _h, ref _l, _d, _e);
                    break;
                case 0x1A: // LD A, (DE)
                    LD_R8_PR16(ref _a, _d, _e);
                    break;
                case 0x1B: // DEC DE
                    DEC_R16(ref _d, ref _e);
                    break;
                case 0x1C: // INC E
                    INC_R8(ref _e);
                    break;
                case 0x1D: // DEC E
                    DEC_R8(ref _e);
                    break;
                case 0x1E: // LD E, D8
                    LD_R8_D8(ref _e);
                    break;
                case 0x1F: // RRA
                    Rra();
                    break;
                case 0x20: // JR NZ, S8
                    JRNZ_S8();
                    break;
                case 0x21: // LD HL, D16
                    LD_R16_D16(ref _h, ref _l);
                    break;
                case 0x22: // LD (HL+), A
                    LDI_PR16_R8(ref _h, ref _l, _a);
                    break;
                case 0x23: // INC HL
                    INC_R16(ref _h, ref _l);
                    break;
                case 0x24: // INC H
                    INC_R8(ref _h);
                    break;
                case 0x25: // DEC H
                    DEC_R8(ref _h);
                    break;
                case 0x26: // LD D, D8
                    LD_R8_D8(ref _d);
                    break;
                case 0x27: // DAA
                    Daa();
                    break;
                case 0x28: // JR Z, S8
                    JRZ_S8();
                    break;
                case 0x29: // ADD HL, HL
                    ADD_R16_R16(ref _h, ref _l, _h, _l);
                    break;
                case 0x2A: // LD A, (HL+)
                    LDI_R8_PR16(ref _a, ref _h, ref _l);
                    break;
                case 0x2B: // DEC HL
                    DEC_R16(ref _h, ref _l);
                    break;
                case 0x2C: // INC _l
                    INC_R8(ref _l);
                    break;
                case 0x2D: // DEC _l
                    DEC_R8(ref _l);
                    break;
                case 0x2E: // LD _l, D8
                    LD_R8_D8(ref _l);
                    break;
                case 0x2F: // CPL
                    Cpl();
                    break;
                case 0x30: // JR NC, S8
                    JRNC_S8();
                    break;
                case 0x31: // LD SP, D16
                    LD_SP_D16();
                    break;
                case 0x32: // LD (HL-), A
                    LDD_PR16_R8(ref _h, ref _l, _a);
                    break;
                case 0x33: // INC SP
                    Sp++;
                    Sp &= 0xFFFF;
                    Pc++;
                    break;
                case 0x34: // INC (HL)
                    INC_PR16(_h, _l);
                    break;
                case 0x35: // DEC (HL)
                    DEC_PR16(_h, _l);
                    break;
                case 0x36: // LD (HL), D8
                    LD_PR16_D8(_h, _l);
                    break;
                case 0x37: // SCF
                    Scf();
                    break;
                case 0x38: // JR C, S8
                    JRC_S8();
                    break;
                case 0x39: // ADD HL, SP
                    ADD_R16_R16(ref _h, ref _l, Sp >> 8, Sp & 0xFF);
                    break;
                case 0x3A: // LD A, (HL-)
                    LDD_R8_PR16(ref _a, ref _h, ref _l);
                    break;
                case 0x3B: // DEC SP
                    Sp--;
                    Sp &= 0xFF;
                    Pc++;
                    break;
                case 0x3C: // INC A
                    INC_R8(ref _a);
                    break;
                case 0x3D: // DEC A
                    DEC_R8(ref _a);
                    break;
                case 0x3E: // LD A, D8
                    LD_R8_D8(ref _a);
                    break;
                case 0x3F: // CCF
                    Ccf();
                    break;
                case 0x40: // LD B, B
                    LD_R8_R8(ref _b, _b);
                    break;
                case 0x41: // LD B, C
                    LD_R8_R8(ref _b, _c);
                    break;
                case 0x42: // LD B, D
                    LD_R8_R8(ref _b, _d);
                    break;
                case 0x43: // LD B, E
                    LD_R8_R8(ref _b, _e);
                    break;
                case 0x44: // LD B, H
                    LD_R8_R8(ref _b, _h);
                    break;
                case 0x45: // LD B, _l
                    LD_R8_R8(ref _b, _l);
                    break;
                case 0x46: // LD B, (HL)
                    LD_R8_PR16(ref _b, _h, _l);
                    break;
                case 0x47: // LD B, A
                    LD_R8_R8(ref _b, _a);
                    break;
                case 0x48: // LD C, B
                    LD_R8_R8(ref _c, _b);
                    break;
                case 0x49: // LD C, C
                    LD_R8_R8(ref _c, _c);
                    break;
                case 0x4A: // LD C, D
                    LD_R8_R8(ref _c, _d);
                    break;
                case 0x4B: // LD C, E
                    LD_R8_R8(ref _c, _e);
                    break;
                case 0x4C: // LD C, H
                    LD_R8_R8(ref _c, _h);
                    break;
                case 0x4D: // LD C, _l
                    LD_R8_R8(ref _c, _l);
                    break;
                case 0x4E: // LD C, (HL)
                    LD_R8_PR16(ref _c, _h, _l);
                    break;
                case 0x4F: // LD C, A
                    LD_R8_R8(ref _c, _a);
                    break;
                case 0x50: // LD D, B
                    LD_R8_R8(ref _d, _b);
                    break;
                case 0x51: // LD D, C
                    LD_R8_R8(ref _d, _c);
                    break;
                case 0x52: // LD D, D
                    LD_R8_R8(ref _d, _d);
                    break;
                case 0x53: // LD D, E
                    LD_R8_R8(ref _d, _e);
                    break;
                case 0x54: // LD D, H
                    LD_R8_R8(ref _d, _h);
                    break;
                case 0x55: // LD D, _l
                    LD_R8_R8(ref _d, _l);
                    break;
                case 0x56: // LD D, (HL)
                    LD_R8_PR16(ref _d, _h, _l);
                    break;
                case 0x57: // LD D, A
                    LD_R8_R8(ref _d, _a);
                    break;
                case 0x58: // LD E, B
                    LD_R8_R8(ref _e, _b);
                    break;
                case 0x59: // LD E, C
                    LD_R8_R8(ref _e, _c);
                    break;
                case 0x5A: // LD E, D
                    LD_R8_R8(ref _e, _d);
                    break;
                case 0x5B: // LD E, E
                    LD_R8_R8(ref _e, _e);
                    break;
                case 0x5C: // LD E, H
                    LD_R8_R8(ref _e, _h);
                    break;
                case 0x5D: // LD E, _l
                    LD_R8_R8(ref _e, _l);
                    break;
                case 0x5E: // LD E, (HL)
                    LD_R8_PR16(ref _e, _h, _l);
                    break;
                case 0x5F: // LD E, A
                    LD_R8_R8(ref _e, _a);
                    break;
                case 0x60: // LD H, B
                    LD_R8_R8(ref _h, _b);
                    break;
                case 0x61: // LD H, C
                    LD_R8_R8(ref _h, _c);
                    break;
                case 0x62: // LD H, D
                    LD_R8_R8(ref _h, _d);
                    break;
                case 0x63: // LD H, E
                    LD_R8_R8(ref _h, _e);
                    break;
                case 0x64: // LD H, H
                    LD_R8_R8(ref _h, _h);
                    break;
                case 0x65: // LD H, _l
                    LD_R8_R8(ref _h, _l);
                    break;
                case 0x66: // LD H, (HL)
                    LD_R8_PR16(ref _h, _h, _l);
                    break;
                case 0x67: // LD H, A
                    LD_R8_R8(ref _h, _a);
                    break;
                case 0x68: // LD _l, B
                    LD_R8_R8(ref _l, _b);
                    break;
                case 0x69: // LD _l, C
                    LD_R8_R8(ref _l, _c);
                    break;
                case 0x6A: // LD _l, D
                    LD_R8_R8(ref _l, _d);
                    break;
                case 0x6B: // LD _l, E
                    LD_R8_R8(ref _l, _e);
                    break;
                case 0x6C: // LD _l, H
                    LD_R8_R8(ref _l, _h);
                    break;
                case 0x6D: // LD _l, _l
                    LD_R8_R8(ref _l, _l);
                    break;
                case 0x6E: // LD _l, (HL)
                    LD_R8_PR16(ref _l, _h, _l);
                    break;
                case 0x6F: // LD _l, A
                    LD_R8_R8(ref _l, _a);
                    break;
                case 0x70: // LD (HL), B
                    LD_PR16_R8(_h, _l, _b);
                    break;
                case 0x71: // LD (HL), C
                    LD_PR16_R8(_h, _l, _c);
                    break;
                case 0x72: // LD (HL), D
                    LD_PR16_R8(_h, _l, _d);
                    break;
                case 0x73: // LD (HL), E
                    LD_PR16_R8(_h, _l, _e);
                    break;
                case 0x74: // LD (HL), H
                    LD_PR16_R8(_h, _l, _h);
                    break;
                case 0x75: // LD (HL), _l
                    LD_PR16_R8(_h, _l, _l);
                    break;
                case 0x76: // HALT
                    Halt();
                    break;
                case 0x77: // LD (HL), A
                    LD_PR16_R8(_h, _l, _a);
                    break;
                case 0x78: // LD A, B
                    LD_R8_R8(ref _a, _b);
                    break;
                case 0x79: // LD A, C
                    LD_R8_R8(ref _a, _c);
                    break;
                case 0x7A: // LD A, D
                    LD_R8_R8(ref _a, _d);
                    break;
                case 0x7B: // LD A, E
                    LD_R8_R8(ref _a, _e);
                    break;
                case 0x7C: // LD A, H
                    LD_R8_R8(ref _a, _h);
                    break;
                case 0x7D: // LD A, _l
                    LD_R8_R8(ref _a, _l);
                    break;
                case 0x7E: // LD A, (HL)
                    LD_R8_PR16(ref _a, _h, _l);
                    break;
                case 0x7F: // LD A, A
                    LD_R8_R8(ref _a, _a);
                    break;
                case 0x80: // ADD A, B
                    ADD_R8_R8(ref _a, _b);
                    break;
                case 0x81: // ADD A, C
                    ADD_R8_R8(ref _a, _c);
                    break;
                case 0x82: // ADD A, D
                    ADD_R8_R8(ref _a, _d);
                    break;
                case 0x83: // ADD A, E
                    ADD_R8_R8(ref _a, _e);
                    break;
                case 0x84: // ADD A, H
                    ADD_R8_R8(ref _a, _h);
                    break;
                case 0x85: // ADD A, _l
                    ADD_R8_R8(ref _a, _l);
                    break;
                case 0x86: // ADD A, (HL)
                    ADD_R8_PR16(ref _a, _h, _l);
                    break;
                case 0x87: // ADD A, A
                    ADD_R8_R8(ref _a, _a);
                    break;
                case 0x88: // ADC A, B
                    ADC_R8_R8(ref _a, _b);
                    break;
                case 0x89: // ADC A, C
                    ADC_R8_R8(ref _a, _c);
                    break;
                case 0x8A: // ADC A, D
                    ADC_R8_R8(ref _a, _d);
                    break;
                case 0x8B: // ADC A, E
                    ADC_R8_R8(ref _a, _e);
                    break;
                case 0x8C: // ADC A, H
                    ADC_R8_R8(ref _a, _h);
                    break;
                case 0x8D: // ADC A, _l
                    ADC_R8_R8(ref _a, _l);
                    break;
                case 0x8E: // ADC A, (HL)
                    ADC_R8_PR16(ref _a, _h, _l);
                    break;
                case 0x8F: // ADC A, A
                    ADC_R8_R8(ref _a, _a);
                    break;
                case 0x90: // SUB B
                    SUB_R8(_b);
                    break;
                case 0x91: // SUB C
                    SUB_R8(_c);
                    break;
                case 0x92: // SUB D
                    SUB_R8(_d);
                    break;
                case 0x93: // SUB E
                    SUB_R8(_e);
                    break;
                case 0x94: // SUB H
                    SUB_R8(_h);
                    break;
                case 0x95: // SUB _l
                    SUB_R8(_l);
                    break;
                case 0x96: // SUB (HL)
                    SUB_PR16(_h, _l);
                    break;
                case 0x97: // SUB A
                    SUB_R8(_a);
                    break;
                case 0x98: // SBC A, B
                    SBC_R8_R8(ref _a, _b);
                    break;
                case 0x99: // SBC A, C
                    SBC_R8_R8(ref _a, _c);
                    break;
                case 0x9A: // SBC A, D
                    SBC_R8_R8(ref _a, _d);
                    break;
                case 0x9B: // SBC A, E
                    SBC_R8_R8(ref _a, _e);
                    break;
                case 0x9C: // SBC A, H
                    SBC_R8_R8(ref _a, _h);
                    break;
                case 0x9D: // SBC A, _l
                    SBC_R8_R8(ref _a, _l);
                    break;
                case 0x9E: // SBC A, (HL)
                    SBC_R8_PR16(ref _a, _h, _l);
                    break;
                case 0x9F: // SBC A, A
                    SBC_R8_R8(ref _a, _a);
                    break;
                case 0xA0: // AND B
                    AND_R8(_b);
                    break;
                case 0xA1: // AND C
                    AND_R8(_c);
                    break;
                case 0xA2: // AND D
                    AND_R8(_d);
                    break;
                case 0xA3: // AND E
                    AND_R8(_e);
                    break;
                case 0xA4: // AND H
                    AND_R8(_h);
                    break;
                case 0xA5: // AND _l
                    AND_R8(_l);
                    break;
                case 0xA6: // AND B
                    AND_PR16(_h, _l);
                    break;
                case 0xA7: // AND A
                    AND_R8(_a);
                    break;
                case 0xA8: // XOR B
                    XOR_R8(_b);
                    break;
                case 0xA9: // XOR C
                    XOR_R8(_c);
                    break;
                case 0xAA: // XOR D
                    XOR_R8(_d);
                    break;
                case 0xAB: // XOR E
                    XOR_R8(_e);
                    break;
                case 0xAC: // XOR H
                    XOR_R8(_h);
                    break;
                case 0xAD: // XOR _l
                    XOR_R8(_l);
                    break;
                case 0xAE: // XOR (HL)
                    XOR_PR16(_h, _l);
                    break;
                case 0xAF: // XOR A
                    XOR_R8(_a);
                    break;
                case 0xB0: // OR B
                    OR_R8(_b);
                    break;
                case 0xB1: // OR C
                    OR_R8(_c);
                    break;
                case 0xB2: // OR D
                    OR_R8(_d);
                    break;
                case 0xB3: // OR E
                    OR_R8(_e);
                    break;
                case 0xB4: // OR H
                    OR_R8(_h);
                    break;
                case 0xB5: // OR _l
                    OR_R8(_l);
                    break;
                case 0xB6: // OR (HL)
                    OR_PR16(_h, _l);
                    break;
                case 0xB7: // OR A
                    OR_R8(_a);
                    break;
                case 0xB8: // CP B
                    CP_R8(_b);
                    break;
                case 0xB9: // CP C
                    CP_R8(_c);
                    break;
                case 0xBA: // CP D
                    CP_R8(_d);
                    break;
                case 0xBB: // CP E
                    CP_R8(_e);
                    break;
                case 0xBC: // CP H
                    CP_R8(_h);
                    break;
                case 0xBD: // CP _l
                    CP_R8(_l);
                    break;
                case 0xBE: // CP (HL)
                    CP_PR16(_h, _l);
                    break;
                case 0xBF: // CP A
                    CP_R8(_a);
                    break;
                case 0xC0: // RET NZ
                    Retnz();
                    break;
                case 0xC1: // POP BC
                    Pop(ref _b, ref _c);
                    break;
                case 0xC2: // JP NZ, A16
                    JPNZ_A16();
                    break;
                case 0xC3: // JP A16
                    JP_A16();
                    break;
                case 0xC4: // CALL NZ, A16
                    CALLNZ_A16();
                    break;
                case 0xC5: // PUSH BC
                    Push(_b, _c);
                    break;
                case 0xC6: // ADD A, D8
                    ADD_R8_D8(ref _a);
                    break;
                case 0xC7: // RST 0x00
                    Rst(0x00);
                    break;
                case 0xC8: // RET Z
                    Retz();
                    break;
                case 0xC9: // RET
                    Ret();
                    break;
                case 0xCA: // JP Z, A16
                    JPZ_A16();
                    break;
                case 0xCB: // PREFIX CB
                    Pc++;
                    ExecutePrefixCb();
                    break;
                case 0xCC: // CALL Z, A16
                    CALLZ_A16();
                    break;
                case 0xCD: // CALL A16
                    CALL_A16();
                    break;
                case 0xCE: // ADC A, D8
                    ADC_R8_D8(ref _a);
                    break;
                case 0xCF: // RST 0x08
                    Rst(0x08);
                    break;
                case 0xD0: // RET NC
                    Retnc();
                    break;
                case 0xD1: // POP DE
                    Pop(ref _d, ref _e);
                    break;
                case 0xD2: // JP NC, A16
                    JPNC_A16();
                    break;
                case 0xD4: // CALL NC, A16
                    CALLNC_A16();
                    break;
                case 0xD5: // PUSH DE
                    Push(_d, _e);
                    break;
                case 0xD6: // SUB D8
                    SUB_D8();
                    break;
                case 0xD7: // RST 0x10
                    Rst(0x10);
                    break;
                case 0xD8: // RET C
                    Retc();
                    break;
                case 0xD9: // RETI
                    Reti();
                    break;
                case 0xDA: // JP C, A16
                    JPC_A16();
                    break;
                case 0xDC: // CALL C, A16
                    CALLC_A16();
                    break;
                case 0xDE: // SBC A, D8
                    SBC_R8_D8(ref _a);
                    break;
                case 0xDF: // RST 0x18
                    Rst(0x18);
                    break;
                case 0xE0: // LD (A8), A
                    LD_A8_R8(_a);
                    break;
                case 0xE1: // POP HL
                    Pop(ref _h, ref _l);
                    break;
                case 0xE2: // LD (C), A
                    LD_PR8_R8(_c, _a);
                    break;
                case 0xE5: // PUSH HL
                    Push(_h, _l);
                    break;
                case 0xE6: // AND D8
                    AND_D8();
                    break;
                case 0xE7: // RST 0x20
                    Rst(0x20);
                    break;
                case 0xE8: // ADD SP, S8
                    ADD_SP_S8();
                    break;
                case 0xE9: // JP (HL)
                    JP_PR16(_h, _l);
                    break;
                case 0xEA: // LD (A16), A
                    LD_A16_R8(_a);
                    break;
                case 0xEE: // XOR D8
                    XOR_D8();
                    break;
                case 0xEF: // RST 0x28
                    Rst(0x28);
                    break;
                case 0xF0: // LD A, (A8)
                    LD_R8_A8(ref _a);
                    break;
                case 0xF1: // POP AF
                    Pop(ref _a, ref _f);
                    break;
                case 0xF2: // LD A, (C)
                    LD_R8_PR8(ref _a, _c);
                    break;
                case 0xF3: // DI
                    Di();
                    break;
                case 0xF5: // PUSH AF
                    Push(_a, _f);
                    break;
                case 0xF6: // OR D8
                    OR_D8();
                    break;
                case 0xF7: // RST 0x30
                    Rst(0x30);
                    break;
                case 0xF8: // LD HL, SP+S8
                    LD_R16_SP_S8(ref _h, ref _l);
                    break;
                case 0xF9: // LD SP, HL
                    LD_SP_R16(_h, _l);
                    break;
                case 0xFA: // LD A, (A16)
                    LD_R8_A16(ref _a);
                    break;
                case 0xFB: // EI
                    Ei();
                    break;
                case 0xFE: // CP D8:
                    CP_D8();
                    break;
                case 0xFF: // RST 0x38
                    Rst(0x38);
                    break;
                default:
                    throw new Exception("Unable to handle opcode 0x" + op.ToString("X"));
                //May be replaced with a derived exception object e.g. "InvalidOpCodeException"
            }
        }

        private void ExecutePrefixCb()
        {
            int op = ReadByte(Pc);

            switch (op)
            {
                case 0x00: // RLC B
                    RLC_R8(ref _b);
                    break;
                case 0x01: // RLC C
                    RLC_R8(ref _c);
                    break;
                case 0x02: // RLC D
                    RLC_R8(ref _d);
                    break;
                case 0x03: // RLC E
                    RLC_R8(ref _e);
                    break;
                case 0x04: // RLC H
                    RLC_R8(ref _h);
                    break;
                case 0x05: // RLC _l
                    RLC_R8(ref _l);
                    break;
                case 0x06: // RLC (HL)
                    RLC_PR16(_h, _l);
                    break;
                case 0x07: // RLC A
                    RLC_R8(ref _a);
                    break;
                case 0x08: // RRC B
                    RRC_R8(ref _b);
                    break;
                case 0x09: // RRC C
                    RRC_R8(ref _c);
                    break;
                case 0x0A: // RRC D
                    RRC_R8(ref _d);
                    break;
                case 0x0B: // RRC E
                    RRC_R8(ref _e);
                    break;
                case 0x0C: // RRC H
                    RRC_R8(ref _h);
                    break;
                case 0x0D: // RRC _l
                    RRC_R8(ref _l);
                    break;
                case 0x0E: // RRC (HL)
                    RRC_PR16(_h, _l);
                    break;
                case 0x0F: // RRC A
                    RRC_R8(ref _b);
                    break;
                case 0x10: // rl B
                    RL_R8(ref _b);
                    break;
                case 0x11: // rl C
                    RL_R8(ref _c);
                    break;
                case 0x12: // rl D
                    RL_R8(ref _d);
                    break;
                case 0x13: // rl E
                    RL_R8(ref _e);
                    break;
                case 0x14: // rl H
                    RL_R8(ref _h);
                    break;
                case 0x15: // rl _l
                    RL_R8(ref _l);
                    break;
                case 0x16: // rl (HL)
                    RL_PR16(_h, _l);
                    break;
                case 0x17: // rl A
                    RL_R8(ref _a);
                    break;
                case 0x18: // RR B
                    RR_R8(ref _b);
                    break;
                case 0x19: // RR C
                    RR_R8(ref _c);
                    break;
                case 0x1A: // RR D
                    RR_R8(ref _d);
                    break;
                case 0x1B: // RR E
                    RR_R8(ref _e);
                    break;
                case 0x1C: // RR H
                    RR_R8(ref _h);
                    break;
                case 0x1D: // RR _l
                    RR_R8(ref _l);
                    break;
                case 0x1E: // RR (HL)
                    RR_PR16(_h, _l);
                    break;
                case 0x1F: // RR A
                    RR_R8(ref _b);
                    break;
                case 0x20: // SLA B
                    SLA_R8(ref _b);
                    break;
                case 0x21: // SLA C
                    SLA_R8(ref _c);
                    break;
                case 0x22: // SLA D
                    SLA_R8(ref _d);
                    break;
                case 0x23: // SLA E
                    SLA_R8(ref _e);
                    break;
                case 0x24: // SLA H
                    SLA_R8(ref _h);
                    break;
                case 0x25: // SLA _l
                    SLA_R8(ref _l);
                    break;
                case 0x26: // SLA (HL)
                    SLA_PR16(_h, _l);
                    break;
                case 0x27: // SLA A
                    SLA_R8(ref _a);
                    break;
                case 0x28: // SRA B
                    SRA_R8(ref _b);
                    break;
                case 0x29: // SRA C
                    SRA_R8(ref _c);
                    break;
                case 0x2A: // SRA D
                    SRA_R8(ref _d);
                    break;
                case 0x2B: // SRA E
                    SRA_R8(ref _e);
                    break;
                case 0x2C: // SRA H
                    SRA_R8(ref _h);
                    break;
                case 0x2D: // SRA _l
                    SRA_R8(ref _l);
                    break;
                case 0x2E: // SRA (HL)
                    SRA_PR16(_h, _l);
                    break;
                case 0x2F: // SRA A
                    SRA_R8(ref _a);
                    break;
                case 0x30: // SWAP B
                    SWAP_R8(ref _b);
                    break;
                case 0x31: // SWAP C
                    SWAP_R8(ref _c);
                    break;
                case 0x32: // SWAP D
                    SWAP_R8(ref _d);
                    break;
                case 0x33: // SWAP E
                    SWAP_R8(ref _e);
                    break;
                case 0x34: // SWAP H
                    SWAP_R8(ref _h);
                    break;
                case 0x35: // SWAP _l
                    SWAP_R8(ref _l);
                    break;
                case 0x36: // SWAP (HL)
                    SWAP_PR16(_h, _l);
                    break;
                case 0x37: // SWAP A
                    SWAP_R8(ref _a);
                    break;
                case 0x38: // SRL B
                    SRL_R8(ref _b);
                    break;
                case 0x39: // SRL C
                    SRL_R8(ref _c);
                    break;
                case 0x3A: // SRL D
                    SRL_R8(ref _d);
                    break;
                case 0x3B: // SRL E
                    SRL_R8(ref _e);
                    break;
                case 0x3C: // SRL H
                    SRL_R8(ref _h);
                    break;
                case 0x3D: // SRL _l
                    SRL_R8(ref _l);
                    break;
                case 0x3E: // SRL (HL)
                    SRL_PR16(_h, _l);
                    break;
                case 0x3F: // SRL A
                    SRL_R8(ref _a);
                    break;
                case 0x40: // BIT 0, B
                    BIT_R8(0, _b);
                    break;
                case 0x41: // BIT 0, C
                    BIT_R8(0, _c);
                    break;
                case 0x42: // BIT 0, D
                    BIT_R8(0, _d);
                    break;
                case 0x43: // BIT 0, E
                    BIT_R8(0, _e);
                    break;
                case 0x44: // BIT 0, H
                    BIT_R8(0, _h);
                    break;
                case 0x45: // BIT 0, _l
                    BIT_R8(0, _l);
                    break;
                case 0x46: // BIT 0, (HL)
                    BIT_PR16(0, _h, _l);
                    break;
                case 0x47: // BIT 0, A
                    BIT_R8(0, _a);
                    break;
                case 0x48: // BIT 1, B
                    BIT_R8(1, _b);
                    break;
                case 0x49: // BIT 1, C
                    BIT_R8(1, _c);
                    break;
                case 0x4A: // BIT 1, D
                    BIT_R8(1, _d);
                    break;
                case 0x4B: // BIT 1, E
                    BIT_R8(1, _e);
                    break;
                case 0x4C: // BIT 1, H
                    BIT_R8(1, _h);
                    break;
                case 0x4D: // BIT 1, _l
                    BIT_R8(1, _l);
                    break;
                case 0x4E: // BIT 1, (HL)
                    BIT_PR16(1, _h, _l);
                    break;
                case 0x4F: // BIT 1, A
                    BIT_R8(1, _a);
                    break;
                case 0x50: // BIT 2, B
                    BIT_R8(2, _b);
                    break;
                case 0x51: // BIT 2, C
                    BIT_R8(2, _c);
                    break;
                case 0x52: // BIT 2, D
                    BIT_R8(2, _d);
                    break;
                case 0x53: // BIT 2, E
                    BIT_R8(2, _e);
                    break;
                case 0x54: // BIT 2, H
                    BIT_R8(2, _h);
                    break;
                case 0x55: // BIT 2, _l
                    BIT_R8(2, _l);
                    break;
                case 0x56: // BIT 2, (HL)
                    BIT_PR16(2, _h, _l);
                    break;
                case 0x57: // BIT 2, A
                    BIT_R8(2, _a);
                    break;
                case 0x58: // BIT 3, B
                    BIT_R8(3, _b);
                    break;
                case 0x59: // BIT 3, C
                    BIT_R8(3, _c);
                    break;
                case 0x5A: // BIT 3, D
                    BIT_R8(3, _d);
                    break;
                case 0x5B: // BIT 3, E
                    BIT_R8(3, _e);
                    break;
                case 0x5C: // BIT 3, H
                    BIT_R8(3, _h);
                    break;
                case 0x5D: // BIT 3, _l
                    BIT_R8(3, _l);
                    break;
                case 0x5E: // BIT 3, (HL)
                    BIT_PR16(3, _h, _l);
                    break;
                case 0x5F: // BIT 3, A
                    BIT_R8(3, _a);
                    break;
                case 0x60: // BIT 4, B
                    BIT_R8(4, _b);
                    break;
                case 0x61: // BIT 4, C
                    BIT_R8(4, _c);
                    break;
                case 0x62: // BIT 4, D
                    BIT_R8(4, _d);
                    break;
                case 0x63: // BIT 4, E
                    BIT_R8(4, _e);
                    break;
                case 0x64: // BIT 4, H
                    BIT_R8(4, _h);
                    break;
                case 0x65: // BIT 4, _l
                    BIT_R8(4, _l);
                    break;
                case 0x66: // BIT 4, (HL)
                    BIT_PR16(4, _h, _l);
                    break;
                case 0x67: // BIT 4, A
                    BIT_R8(4, _a);
                    break;
                case 0x68: // BIT 5, B
                    BIT_R8(5, _b);
                    break;
                case 0x69: // BIT 5, C
                    BIT_R8(5, _c);
                    break;
                case 0x6A: // BIT 5, D
                    BIT_R8(5, _d);
                    break;
                case 0x6B: // BIT 5, E
                    BIT_R8(5, _e);
                    break;
                case 0x6C: // BIT 5, H
                    BIT_R8(5, _h);
                    break;
                case 0x6D: // BIT 5, _l
                    BIT_R8(5, _l);
                    break;
                case 0x6E: // BIT 5, (HL)
                    BIT_PR16(5, _h, _l);
                    break;
                case 0x6F: // BIT 5, A
                    BIT_R8(5, _a);
                    break;
                case 0x70: // BIT 6, B
                    BIT_R8(6, _b);
                    break;
                case 0x71: // BIT 6, C
                    BIT_R8(6, _c);
                    break;
                case 0x72: // BIT 6, D
                    BIT_R8(6, _d);
                    break;
                case 0x73: // BIT 6, E
                    BIT_R8(6, _e);
                    break;
                case 0x74: // BIT 6, H
                    BIT_R8(6, _h);
                    break;
                case 0x75: // BIT 6, _l
                    BIT_R8(6, _l);
                    break;
                case 0x76: // BIT 6, (HL)
                    BIT_PR16(6, _h, _l);
                    break;
                case 0x77: // BIT 6, A
                    BIT_R8(6, _a);
                    break;
                case 0x78: // BIT 7, B
                    BIT_R8(7, _b);
                    break;
                case 0x79: // BIT 7, C
                    BIT_R8(7, _c);
                    break;
                case 0x7A: // BIT 7, D
                    BIT_R8(7, _d);
                    break;
                case 0x7B: // BIT 7, E
                    BIT_R8(7, _e);
                    break;
                case 0x7C: // BIT 7, H
                    BIT_R8(7, _h);
                    break;
                case 0x7D: // BIT 7, _l
                    BIT_R8(7, _l);
                    break;
                case 0x7E: // BIT 7, (HL)
                    BIT_PR16(7, _h, _l);
                    break;
                case 0x7F: // BIT 7, A
                    BIT_R8(7, _a);
                    break;
                case 0x80: // RES 0, B
                    RES_R8(0, ref _b);
                    break;
                case 0x81: // RES 0, C
                    RES_R8(0, ref _c);
                    break;
                case 0x82: // RES 0, D
                    RES_R8(0, ref _d);
                    break;
                case 0x83: // RES 0, E
                    RES_R8(0, ref _e);
                    break;
                case 0x84: // RES 0, H
                    RES_R8(0, ref _h);
                    break;
                case 0x85: // RES 0, _l
                    RES_R8(0, ref _l);
                    break;
                case 0x86: // RES 0, (HL)
                    RES_PR16(0, _h, _l);
                    break;
                case 0x87: // RES 0, A
                    RES_R8(0, ref _a);
                    break;
                case 0x88: // RES 1, B
                    RES_R8(1, ref _b);
                    break;
                case 0x89: // RES 1, C
                    RES_R8(1, ref _c);
                    break;
                case 0x8A: // RES 1, D
                    RES_R8(1, ref _d);
                    break;
                case 0x8B: // RES 1, E
                    RES_R8(1, ref _e);
                    break;
                case 0x8C: // RES 1, H
                    RES_R8(1, ref _h);
                    break;
                case 0x8D: // RES 1, _l
                    RES_R8(1, ref _l);
                    break;
                case 0x8E: // RES 1, (HL)
                    RES_PR16(1, _h, _l);
                    break;
                case 0x8F: // RES 1, A
                    RES_R8(1, ref _a);
                    break;
                case 0x90: // RES 2, B
                    RES_R8(2, ref _b);
                    break;
                case 0x91: // RES 2, C
                    RES_R8(2, ref _c);
                    break;
                case 0x92: // RES 2, D
                    RES_R8(2, ref _d);
                    break;
                case 0x93: // RES 2, E
                    RES_R8(2, ref _e);
                    break;
                case 0x94: // RES 2, H
                    RES_R8(2, ref _h);
                    break;
                case 0x95: // RES 2, _l
                    RES_R8(2, ref _l);
                    break;
                case 0x96: // RES 2, (HL)
                    RES_PR16(2, _h, _l);
                    break;
                case 0x97: // RES 2, A
                    RES_R8(2, ref _a);
                    break;
                case 0x98: // RES 3, B
                    RES_R8(3, ref _b);
                    break;
                case 0x99: // RES 3, C
                    RES_R8(3, ref _c);
                    break;
                case 0x9A: // RES 3, D
                    RES_R8(3, ref _d);
                    break;
                case 0x9B: // RES 3, E
                    RES_R8(3, ref _e);
                    break;
                case 0x9C: // RES 3, H
                    RES_R8(3, ref _h);
                    break;
                case 0x9D: // RES 3, _l
                    RES_R8(3, ref _l);
                    break;
                case 0x9E: // RES 3, (HL)
                    RES_PR16(3, _h, _l);
                    break;
                case 0x9F: // RES 3, A
                    RES_R8(3, ref _a);
                    break;
                case 0xA0: // RES 4, B
                    RES_R8(4, ref _b);
                    break;
                case 0xA1: // RES 4, C
                    RES_R8(4, ref _c);
                    break;
                case 0xA2: // RES 4, D
                    RES_R8(4, ref _d);
                    break;
                case 0xA3: // RES 4, E
                    RES_R8(4, ref _e);
                    break;
                case 0xA4: // RES 4, H
                    RES_R8(4, ref _h);
                    break;
                case 0xA5: // RES 4, _l
                    RES_R8(4, ref _l);
                    break;
                case 0xA6: // RES 4, (HL)
                    RES_PR16(4, _h, _l);
                    break;
                case 0xA7: // RES 4, A
                    RES_R8(4, ref _a);
                    break;
                case 0xA8: // RES 5, B
                    RES_R8(5, ref _b);
                    break;
                case 0xA9: // RES 5, C
                    RES_R8(5, ref _c);
                    break;
                case 0xAA: // RES 5, D
                    RES_R8(5, ref _d);
                    break;
                case 0xAB: // RES 5, E
                    RES_R8(5, ref _e);
                    break;
                case 0xAC: // RES 5, H
                    RES_R8(5, ref _h);
                    break;
                case 0xAD: // RES 5, _l
                    RES_R8(5, ref _l);
                    break;
                case 0xAE: // RES 5, (HL)
                    RES_PR16(5, _h, _l);
                    break;
                case 0xAF: // RES 5, A
                    RES_R8(5, ref _a);
                    break;
                case 0xB0: // RES 6, B
                    RES_R8(6, ref _b);
                    break;
                case 0xB1: // RES 6, C
                    RES_R8(6, ref _c);
                    break;
                case 0xB2: // RES 6, D
                    RES_R8(6, ref _d);
                    break;
                case 0xB3: // RES 6, E
                    RES_R8(6, ref _e);
                    break;
                case 0xB4: // RES 6, H
                    RES_R8(6, ref _h);
                    break;
                case 0xB5: // RES 6, _l
                    RES_R8(6, ref _l);
                    break;
                case 0xB6: // RES 6, (HL)
                    RES_PR16(4, _h, _l);
                    break;
                case 0xB7: // RES 6, A
                    RES_R8(6, ref _a);
                    break;
                case 0xB8: // RES 7, B
                    RES_R8(7, ref _b);
                    break;
                case 0xB9: // RES 7, C
                    RES_R8(7, ref _c);
                    break;
                case 0xBA: // RES 7, D
                    RES_R8(7, ref _d);
                    break;
                case 0xBB: // RES 7, E
                    RES_R8(7, ref _e);
                    break;
                case 0xBC: // RES 7, H
                    RES_R8(7, ref _h);
                    break;
                case 0xBD: // RES 7, _l
                    RES_R8(7, ref _l);
                    break;
                case 0xBE: // RES 7, (HL)
                    RES_PR16(7, _h, _l);
                    break;
                case 0xBF: // RES 7, A
                    RES_R8(7, ref _a);
                    break;
                case 0xC0: // SET 0, B
                    SET_R8(0, ref _b);
                    break;
                case 0xC1: // SET 0, C
                    SET_R8(0, ref _c);
                    break;
                case 0xC2: // SET 0, D
                    SET_R8(0, ref _d);
                    break;
                case 0xC3: // SET 0, E
                    SET_R8(0, ref _e);
                    break;
                case 0xC4: // SET 0, H
                    SET_R8(0, ref _h);
                    break;
                case 0xC5: // SET 0, _l
                    SET_R8(0, ref _l);
                    break;
                case 0xC6: // SET 0, (HL)
                    SET_PR16(0, _h, _l);
                    break;
                case 0xC7: // SET 0, A
                    SET_R8(0, ref _a);
                    break;
                case 0xC8: // SET 1, B
                    SET_R8(1, ref _b);
                    break;
                case 0xC9: // SET 1, C
                    SET_R8(1, ref _c);
                    break;
                case 0xCA: // SET 1, D
                    SET_R8(1, ref _d);
                    break;
                case 0xCB: // SET 1, E
                    SET_R8(1, ref _e);
                    break;
                case 0xCC: // SET 1, H
                    SET_R8(1, ref _h);
                    break;
                case 0xCD: // SET 1, _l
                    SET_R8(1, ref _l);
                    break;
                case 0xCE: // SET 1, (HL)
                    SET_PR16(1, _h, _l);
                    break;
                case 0xCF: // SET 1, A
                    SET_R8(1, ref _a);
                    break;
                case 0xD0: // SET 2, B
                    SET_R8(2, ref _b);
                    break;
                case 0xD1: // SET 2, C
                    SET_R8(2, ref _c);
                    break;
                case 0xD2: // SET 2, D
                    SET_R8(2, ref _d);
                    break;
                case 0xD3: // SET 2, E
                    SET_R8(2, ref _e);
                    break;
                case 0xD4: // SET 2, H
                    SET_R8(2, ref _h);
                    break;
                case 0xD5: // SET 2, _l
                    SET_R8(2, ref _l);
                    break;
                case 0xD6: // SET 2, (HL)
                    SET_PR16(2, _h, _l);
                    break;
                case 0xD7: // SET 2, A
                    SET_R8(2, ref _a);
                    break;
                case 0xD8: // SET 3, B
                    SET_R8(3, ref _b);
                    break;
                case 0xD9: // SET 3, C
                    SET_R8(3, ref _c);
                    break;
                case 0xDA: // SET 3, D
                    SET_R8(3, ref _d);
                    break;
                case 0xDB: // SET 3, E
                    SET_R8(3, ref _e);
                    break;
                case 0xDC: // SET 3, H
                    SET_R8(3, ref _h);
                    break;
                case 0xDD: // SET 3, _l
                    SET_R8(3, ref _l);
                    break;
                case 0xDE: // SET 3, (HL)
                    SET_PR16(3, _h, _l);
                    break;
                case 0xDF: // SET 3, A
                    SET_R8(3, ref _a);
                    break;
                case 0xE0: // SET 4, B
                    SET_R8(4, ref _b);
                    break;
                case 0xE1: // SET 4, C
                    SET_R8(4, ref _c);
                    break;
                case 0xE2: // SET 4, D
                    SET_R8(4, ref _d);
                    break;
                case 0xE3: // SET 4, E
                    SET_R8(4, ref _e);
                    break;
                case 0xE4: // SET 4, H
                    SET_R8(4, ref _h);
                    break;
                case 0xE5: // SET 4, _l
                    SET_R8(4, ref _l);
                    break;
                case 0xE6: // SET 4, (HL)
                    SET_PR16(4, _h, _l);
                    break;
                case 0xE7: // SET 4, A
                    SET_R8(4, ref _a);
                    break;
                case 0xE8: // SET 5, B
                    SET_R8(5, ref _b);
                    break;
                case 0xE9: // SET 5, C
                    SET_R8(5, ref _c);
                    break;
                case 0xEA: // SET 5, D
                    SET_R8(5, ref _d);
                    break;
                case 0xEB: // SET 5, E
                    SET_R8(5, ref _e);
                    break;
                case 0xEC: // SET 5, H
                    SET_R8(5, ref _h);
                    break;
                case 0xED: // SET 5, _l
                    SET_R8(5, ref _l);
                    break;
                case 0xEE: // SET 5, (HL)
                    SET_PR16(5, _h, _l);
                    break;
                case 0xEF: // SET 5, A
                    SET_R8(5, ref _a);
                    break;
                case 0xF0: // SET 6, B
                    SET_R8(6, ref _b);
                    break;
                case 0xF1: // SET 6, C
                    SET_R8(6, ref _c);
                    break;
                case 0xF2: // SET 6, D
                    SET_R8(6, ref _d);
                    break;
                case 0xF3: // SET 6, E
                    SET_R8(6, ref _e);
                    break;
                case 0xF4: // SET 6, H
                    SET_R8(6, ref _h);
                    break;
                case 0xF5: // SET 6, _l
                    SET_R8(6, ref _l);
                    break;
                case 0xF6: // SET 6, (HL)
                    SET_PR16(6, _h, _l);
                    break;
                case 0xF7: // SET 6, A
                    SET_R8(6, ref _a);
                    break;
                case 0xF8: // SET 7, B
                    SET_R8(7, ref _b);
                    break;
                case 0xF9: // SET 7, C
                    SET_R8(7, ref _c);
                    break;
                case 0xFA: // SET 7, D
                    SET_R8(7, ref _d);
                    break;
                case 0xFB: // SET 7, E
                    SET_R8(7, ref _e);
                    break;
                case 0xFC: // SET 7, H
                    SET_R8(7, ref _h);
                    break;
                case 0xFD: // SET 7, _l
                    SET_R8(7, ref _l);
                    break;
                case 0xFE: // SET 7, (HL)
                    SET_PR16(7, _h, _l);
                    break;
                case 0xFF: // SET 7, A
                    SET_R8(7, ref _a);
                    break;
                default:
                    throw new Exception("Unable to handle opcode 0xCB" + op.ToString("X"));
            }

            Pc++;
        }

        // Stack
        private void push_internal(int v)
        {
            WriteShort(Sp, v);
            Sp -= 2;
        }

        private int pop_internal()
        {
            int v = ReadShort(Sp);
            Sp += 2;
            return v;
        }

        // Interrupting
        public void Interrupt(int address)
        {
            Ime = false;
            push_internal(Pc);
            Pc = address;
        }

        // Memory Helpers
        private byte ReadByte(int address)
        {
            return Memory.ReadByte(address);
        }

        private ushort ReadShort(int address)
        {
            return (ushort)((Memory.ReadByte(address + 1) << 8) + Memory.ReadByte(address));
        }

        private void WriteByte(int address, int value)
        {
            Memory.WriteByte(address, (byte)value);
        }

        private void WriteShort(int address, int value)
        {
            Memory.WriteByte(address, (byte)(value & 0xFF));
            Memory.WriteByte(address + 1, (byte)(value >> 8));
        }

        #region Opcode implementation

        #region Interrupts

        private void Reti()
        {
            Ime = true;
            _waitForInterrupt = false;
            Pc = pop_internal();
        }

        private void Ei()
        {
            Ime = true;
            Pc++;
        }

        private void Di()
        {
            Ime = false;
            Pc++;
        }

        #endregion

        #region Stack

        private void Push(int rh, int rl)
        {
            push_internal((rh << 8) + rl);
            Pc++;
        }

        private void Pop(ref int rh, ref int rl) //May be realized with out statements
        {
            int v = pop_internal();
            rh = v >> 8;
            rl = v & 0xFF;
            Pc++;
        }

        #endregion

        #region Compare

        private void CP_R8(int ra)
        {
            FlagN = true;
            FlagHc = (_a & 0xF) < (ra & 0xF);
            FlagC = ra > _a;
            FlagZ = ((_a - ra) & 0xFF) == 0;
            Pc++;
        }

        private void CP_PR16(int rah, int ral)
        {
            int v = ReadByte((rah << 8) + ral);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            FlagZ = ((_a - v) & 0xFF) == 0;
            Pc++;
        }

        private void CP_D8()
        {
            int v = ReadByte(Pc + 1);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            FlagZ = ((_a - v) & 0xFF) == 0;
            Pc += 2;
        }

        #endregion

        #region Program control (Jumps, etc.)

        private void JR_S8()
        {
            int relAddr = ReadByte(Pc + 1);
            if (relAddr > 0x7F)
            {
                relAddr -= 0xFF;
            }
            Pc += relAddr + 1;
        }

        private void JRNZ_S8()
        {
            if (!FlagZ)
            {
                JR_S8();
            }
            else
            {
                Pc += 2;
            }
        }

        private void JRZ_S8()
        {
            if (FlagZ)
            {
                JR_S8();
            }
            else
            {
                Pc += 2;
            }
        }

        private void JRNC_S8()
        {
            if (!FlagC)
            {
                JR_S8();
            }
            else
            {
                Pc += 2;
            }
        }

        private void JRC_S8()
        {
            if (FlagC)
            {
                JR_S8();
            }
            else
            {
                Pc += 2;
            }
        }

        private void JP_A16()
        {
            Pc = ReadShort(Pc + 1);
        }

        private void JPNZ_A16()
        {
            if (!FlagZ)
            {
                JP_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void JPZ_A16()
        {
            if (FlagZ)
            {
                JP_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void JPNC_A16()
        {
            if (!FlagC)
            {
                JP_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void JPC_A16()
        {
            if (FlagC)
            {
                JP_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void JP_PR16(int rh, int rl)
        {
            Pc = ReadShort((rh << 8) + rl);
        }

        private void CALL_A16()
        {
            push_internal(Pc + 3);
            Pc = ReadShort(Pc + 1);
        }

        private void CALLNZ_A16()
        {
            if (!FlagZ)
            {
                CALL_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void CALLZ_A16()
        {
            if (FlagZ)
            {
                CALL_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void CALLNC_A16()
        {
            if (!FlagC)
            {
                CALL_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void CALLC_A16()
        {
            if (FlagC)
            {
                CALL_A16();
            }
            else
            {
                Pc += 3;
            }
        }

        private void Halt()
        {
            _waitForInterrupt = true;
            Pc++;
        }

        private void Ret()
        {
            Pc = pop_internal();
        }

        private void Retnz()
        {
            if (!FlagZ)
            {
                Ret();
            }
            else
            {
                Pc++;
            }
        }

        private void Retz()
        {
            if (FlagZ)
            {
                Ret();
            }
            else
            {
                Pc++;
            }
        }

        private void Retnc()
        {
            if (!FlagC)
            {
                Ret();
            }
            else
            {
                Pc++;
            }
        }

        private void Retc()
        {
            if (FlagC)
            {
                Ret();
            }
            else
            {
                Pc++;
            }
        }

        private void Rst(int address)
        {
            push_internal(Pc);
            Pc = address;
        }

        #endregion

        #region LOAD

        private void LD_R8_R8(ref int ra, int rb) //All of the out Statements 
        {
            ra = rb;
            Pc++;
        }

        private void LD_R16_D16(ref int rh, ref int rl)
        {
            rh = ReadByte(Pc + 2);
            rl = ReadByte(Pc + 1);
            Pc += 3;
        }

        private void LD_PR16_R8(int rah, int ral, int rb)
        {
            WriteByte(rah << 8 + ral, rb);
            Pc++;
        }

        private void LD_R8_D8(ref int r)
        {
            r = ReadByte(Pc + 1);
            Pc += 2;
        }

        private void LD_A16_R8(int r)
        {
            WriteShort((ReadByte(Pc + 2) << 8) + ReadByte(Pc + 1), r);
            Pc += 3;
        }

        private void LD_A16_R16(int rh, int rl)
        {
            WriteShort(ReadShort(Pc + 1), (rh << 8) + rl);
            Pc += 3;
        }

        private void LD_R8_A16(ref int r)
        {
            r = ReadByte(ReadShort(Pc + 1));
            Pc += 3;
        }

        private void LD_R8_PR16(ref int ra, int rbh, int rbl)
        {
            ra = ReadByte((rbh << 8) + rbl);
            Pc++;
        }

        private void LD_PR16_D8(int rah, int ral)
        {
            WriteByte((rah << 8) + ral, ReadByte(Pc + 1));
            Pc += 2;
        }

        private void LD_SP_D16()
        {
            Sp = ReadShort(Pc + 1);
            Pc += 3;
        }

        private void LD_SP_R16(int rh, int rl)
        {
            Sp = (rh << 8) + rl;
            Pc++;
        }

        private void LD_R16_SP_S8(ref int rh, ref int rl)
        {
            int v = ReadByte(Pc + 1);
            if (v > 0x7F)
            {
                v -= 255;
            }
            rl = (Sp & 0xFF) + v;
            FlagHc = rl > 0xFF;
            rl &= 0xFF;
            rh = (Sp >> 8) + (FlagHc ? 1 : 0);
            FlagC = rh > 0xFF;
            rh &= 0xFF;
            Pc += 2;
        }

        private void LDI_PR16_R8(ref int rah, ref int ral, int rb)
        {
            LDI_PR16_R8(ref rah, ref ral, rb); //Attention infinite recursive method call ; rb is only recursively used / not used
            INC_R16(ref rah, ref ral);
            Pc -= 1;
        }

        private void LDI_R8_PR16(ref int ra, ref int rbh, ref int rbl)
        {
            LD_R8_PR16(ref ra, rbh, rbl);
            INC_R16(ref rbh, ref rbl);
            Pc -= 1;
        }

        private void LDD_PR16_R8(ref int rah, ref int ral, int rb)
        {
            LDI_PR16_R8(ref rah, ref ral, rb);
            DEC_R16(ref rah, ref ral);
            Pc -= 1;
        }

        private void LDD_R8_PR16(ref int ra, ref int rbh, ref int rbl)
        {
            LD_R8_PR16(ref ra, rbh, rbl);
            DEC_R16(ref rbh, ref rbl);
            Pc -= 1;
        }

        private void LD_A8_R8(int ra)
        {
            WriteByte(0xFF00 + ReadByte(Pc + 1), ra);
            Pc += 2;
        }

        private void LD_R8_A8(ref int ra)
        {
            ra = ReadByte(0xFF00 + ReadByte(Pc + 1));
            Pc += 2;
        }

        private void LD_PR8_R8(int ra, int rb)
        {
            WriteByte(0xFF00 + ra, rb);
            Pc++;
        }

        private void LD_R8_PR8(ref int ra, int rb)
        {
            ra = ReadByte(0xFF00 + rb);
            Pc++;
        }

        #endregion

        #region MATH

        private void INC_R16(ref int rh, ref int rl)
        {
            if (rl == 0xFF)
            {
                rh = (rh + 1) & 0xFF;
                rl = 0;
            }
            else
            {
                rl++;
            }
            Pc++;
        }

        private void INC_R8(ref int r)
        {
            FlagN = false;
            FlagHc = (r & 0xF) == 0xF;
            r++;
            r &= 0xFF;
            FlagZ = r == 0;
            Pc++;
        }

        private void INC_PR16(int rh, int rl)
        {
            int addr = (rh << 8) + rl;
            int vOrig = ReadByte(addr);
            int v = (vOrig + 1) & 0xFF;
            WriteByte(addr, v);
            FlagZ = v == 0;
            FlagN = false;
            FlagHc = (vOrig & 0xF) == 0xF;
            Pc++;
        }

        private void DEC_R16(ref int RH, ref int RL)
        {
            if (RL == 0)
            {
                RH = (RH - 1) & 0xFF;
                RL = 0xFF;
            }
            else
            {
                RL--;
            }
            Pc++;
        }

        private void DEC_R8(ref int R)
        {
            FlagN = true;
            FlagHc = (R & 0xF) == 0x0;
            R--;
            R &= 0xFF;
            FlagZ = R == 0;
            Pc++;
        }

        private void DEC_PR16(int rh, int rl)
        {
            int addr = (rh << 8) + rl;
            int vOrig = ReadByte(addr);
            int v = (vOrig - 1) & 0xFF;
            WriteByte(addr, v);
            FlagZ = v == 0;
            FlagN = true;
            FlagHc = (vOrig & 0xF) == 0x0;
            Pc++;
        }

        private void ADD_R16_R16(ref int rah, ref int ral, int rbh, int rbl)
        {
            FlagN = false;
            ral += rbl;
            int carry = (ral > 0xFF) ? 1 : 0; //Moved declaration to assignment
            ral &= 0xFF;
            FlagHc = carry + (rah & 0xF) + (rbh & 0xF) > 0xF;
            rah += rbh + carry;
            FlagC = rah > 0xFF;
            rah &= 0xFF;
            Pc++;
        }

        private void ADD_R8_R8(ref int ra, int rb)
        {
            FlagN = false;
            FlagHc = (ra & 0xF) + (rb & 0xF) > 0xF;
            ra += rb;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void ADD_R8_PR16(ref int ra, int rbh, int rbl)
        {
            int v = ReadByte((rbh << 8) + rbl);
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) > 0xF;
            ra += v;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void ADD_R8_D8(ref int ra)
        {
            int v = ReadByte(Pc + 1);
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) > 0xF;
            ra += v;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        private void ADD_SP_S8()
        {
            int v = ReadByte(Pc++);
            if (v > 0x7F)
            {
                v -= 256;
            }
            Sp += v;
        }

        private void ADC_R8_R8(ref int ra, int rb)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (rb & 0xF) + carryBit > 0xF;
            ra += rb + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void ADC_R8_PR16(ref int ra, int rbh, int rbl)
        {
            int v = ReadByte((rbh << 8) + rbl);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) + carryBit > 0xF;
            ra += v + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void ADC_R8_D8(ref int ra)
        {
            int v = ReadByte(Pc + 1);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) + carryBit > 0xF;
            ra += v + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        private void SUB_R8(int ra)
        {
            FlagN = true;
            FlagHc = (_a & 0xF) < (ra & 0xF);
            FlagC = ra > _a;
            _a -= ra;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc++;
        }

        private void SUB_PR16(int rah, int ral)
        {
            int v = ReadByte((rah << 8) + ral);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            _a -= v;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc++;
        }

        private void SUB_D8()
        {
            int v = ReadByte(Pc + 1);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            _a -= v;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc += 2;
        }

        private void SBC_R8_R8(ref int ra, int rb)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = true;
            FlagHc = (ra & 0xF) < ((rb + carryBit) & 0xF);
            FlagC = (rb + carryBit) > ra;
            ra -= (rb + carryBit);
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void SBC_R8_PR16(ref int ra, int rah, int ral)
        {
            int carryBit = FlagC ? 1 : 0;
            int v = ReadByte((rah << 8) + ral) + carryBit;
            FlagN = true;
            FlagHc = (ra & 0xF) < (v & 0xF);
            FlagC = v > ra;
            ra -= v;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        private void SBC_R8_D8(ref int ra)
        {
            int carryBit = FlagC ? 1 : 0;
            int v = ReadByte(Pc + 1) + carryBit;
            FlagN = true;
            FlagHc = (ra & 0xF) < (v & 0xF);
            FlagC = v > ra;
            ra -= v;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        #endregion

        #region BIT OPERATIONS

        private void Rlca()
        {
            int highBit = _a >> 7;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            _a = ((_a << 1) & 0xFF) | highBit;
            Pc++;
        }

        private void Rrca()
        {
            int lowBit = _a & 1;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            _a = (_a >> 1) | (lowBit << 7);
            Pc++;
        }

        private void Rla()
        {
            int carryBit = FlagC ? 1 : 0;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = (_a >> 7) == 1;
            _a = ((_a << 1) & 0xFF) | carryBit;
            Pc++;
        }

        private void Rra()
        {
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = (_a & 1) == 1;
            _a = (_a >> 1) | carryBit;
            Pc++;
        }

        private void Cpl()
        {
            FlagHc = true;
            FlagN = true;
            _a ^= 0xFF;
            Pc++;
        }

        private void AND_R8(int ra)
        {
            _a &= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc++;
        }

        private void AND_PR16(int rah, int ral)
        {
            _a &= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc++;
        }

        private void AND_D8()
        {
            _a &= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc += 2;
        }

        private void XOR_R8(int ra)
        {
            _a ^= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        private void XOR_PR16(int rah, int ral)
        {
            _a ^= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        private void XOR_D8()
        {
            _a ^= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc += 2;
        }

        private void OR_R8(int ra)
        {
            _a |= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        private void OR_PR16(int rah, int ral)
        {
            _a |= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        private void OR_D8()
        {
            _a |= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc += 2;
        }

        #endregion

        #region Flags

        private void Scf()
        {
            FlagN = false;
            FlagHc = false;
            FlagC = true;
            Pc++;
        }

        private void Ccf()
        {
            FlagC = !FlagC;
            FlagHc = !FlagHc;
            FlagN = false;
            Pc++;
        }

        #endregion

        #region BCD

        private void Daa()
        {
            if ((_a & 0xF) > 9 || FlagHc) _a += 0x6;
            if ((_a >> 4) > 9 || FlagC) _a += 0x60;
            _a &= 0xFF;
            Pc++;
        }

        #endregion

        #region Prefix CB

        private void RLC_R8(ref int r)
        {
            int highBit = r >> 7;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            r = ((r << 1) & 0xFF) | highBit;
            FlagZ = r == 0;
        }

        private void RLC_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int highBit = v >> 7;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            v = ((v << 1) & 0xFF) | highBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void RRC_R8(ref int r)
        {
            int lowBit = r & 1;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            r = (r >> 1) | (lowBit << 7);
            FlagZ = r == 0;
        }

        private void RRC_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int lowBit = v & 1;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            v = (v >> 1) | (lowBit << 7);
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void RL_R8(ref int r)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = false;
            FlagC = (r >> 7) == 1;
            r = ((r << 1) & 0xFF) | carryBit;
            FlagZ = r == 0;
        }

        private void RL_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = false;
            FlagC = (v >> 7) == 1;
            v = ((v << 1) & 0xFF) | carryBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void RR_R8(ref int r)
        {
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagN = false;
            FlagHc = false;
            FlagC = (r & 1) == 1;
            r = (r >> 1) | carryBit;
            FlagZ = r == 0;
        }

        private void RR_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagN = false;
            FlagHc = false;
            FlagC = (v & 1) == 1;
            v = (v >> 1) | carryBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void SLA_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = (r >> 7) == 1;
            r <<= 1;
            r &= 0xFF;
            FlagZ = r == 0;
        }

        private void SLA_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = (v >> 7) == 1;
            v <<= 1;
            v &= 0xFF;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void SRA_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            r = (r & 0x80) + (r >> 1);
            FlagZ = r == 0;
        }

        private void SRA_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            v = (v & 0x80) + (v >> 1);
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void SRL_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = (r & 1) == 1;
            r >>= 1;
            r &= 0xFF;
            FlagZ = r == 0;
        }

        private void SRL_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = (v & 1) == 1;
            v >>= 1;
            v &= 0xFF;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        private void SWAP_R8(ref int r)
        {
            r = ((r << 4) | (r >> 4)) & 0xFF;
            FlagZ = r == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
        }

        private void SWAP_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = ((v << 4) | (v >> 4)) & 0xFF;
            FlagZ = v == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            WriteByte(address, v);
        }

        private void BIT_R8(int n, int r)
        {
            FlagZ = (r & (1 << n)) == 0;
            FlagN = false;
            FlagHc = true;
        }

        private void BIT_PR16(int n, int rh, int rl)
        {
            FlagZ = (ReadByte((rh << 8) + rl) & (1 << n)) == 0;
            FlagN = false;
            FlagHc = true;
        }

        private void RES_R8(int n, ref int r)
        {
            r = r & (0xFF - (1 << n));
        }

        private void RES_PR16(int n, int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = v & (0xFF - (1 << n));
            WriteByte(address, v);
        }

        private void SET_R8(int n, ref int r)
        {
            r = r | (1 << n);
        }

        private void SET_PR16(int n, int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = v | (1 << n);
            WriteByte(address, v);
        }

        #endregion

        // LEGENDE:
        // ~ R8   : 8-bit Register
        // ~ R16  : 16-bit Register
        // ~ PR8  : 0xFF00 + Pointer aus 8-bit Register
        // ~ PR16 : Pointer aus 16-bit Register
        // ~ D8   : Byte Data
        // ~ D16  : Short Data
        // ~ A8   : 8-bit Pointer (0xFF00 + A8)
        // ~ A16  : 16-bit Pointer
        // ~ S8   : 8-bit signed relative data

        #endregion
    }
}