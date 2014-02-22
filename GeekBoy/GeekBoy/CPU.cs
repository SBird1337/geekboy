using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBoy
{
    class CPU
    {

        // Registers
        public int A, F, B, C, D, E, H, L;
        public int SP, PC;

        // Flags
        public bool Running = true;
        public bool IME = false; // Interrupt Enable
        public bool FLAG_Z, FLAG_N, FLAG_HC, FLAG_C; // Zero, Substract, HalfCarry, Carry
        public int IE = 0;

        // Memory
        public IMemoryDevice Memory;

        // Tables
        //private int[] CycleTable = { 4, 12, 8, 8, 4, 4, 8, 4, 20, 8, 8, 8, 4, 4, 8, 4,
        //                             4, 12, 8, 8, 4, 4, 8, 4, 12, 8, 8, 8, 4, 4, 8, 4};

        private bool WaitForInterrupt = false;

        public CPU(IMemoryDevice Memory)
        {
            this.Memory = Memory;
        }

        public void ExecuteOP()
        {
            byte op = ReadByte(PC);

            if (!Running || WaitForInterrupt) return;

            F = (FLAG_Z ? 0x80 : 0) + (FLAG_N ? 0x40 : 0) + (FLAG_HC ? 0x20 : 0) + (FLAG_C ? 0x10 : 0);

            switch (op)
            {
                case 0x00: // NOP
                    PC++;
                    break;
                case 0x01: // LD BC, d16
                    LD_R16_D16(ref B, ref C);
                    break;
                case 0x02: // LD (BC), A
                    LD_PR16_R8(B, C, A);
                    break;
                case 0x03: // INC BC
                    INC_R16(ref B, ref C);
                    break;
                case 0x04: // INC B
                    INC_R8(ref B);
                    break;
                case 0x05: // DEC B
                    DEC_R8(ref B);
                    break;
                case 0x06: // LD B, D8
                    LD_R8_D8(ref B);
                    break;
                case 0x07: // RLCA
                    RLCA();
                    break;
                case 0x08: // LD (A16), SP
                    LD_A16_R16((SP >> 8) & 0xFF, SP & 0xFF);
                    break;
                case 0x09: // ADD HL, BC
                    ADD_R16_R16(ref H, ref L, B, C);
                    break;
                case 0x0A: // LD A, (BC)
                    LD_R8_PR16(ref A, B, C);
                    break;
                case 0x0B: // DEC BC
                    DEC_R16(ref B, ref C);
                    break;
                case 0x0C: // INC C
                    INC_R8(ref C);
                    break;
                case 0x0D: // DEC C
                    DEC_R8(ref C);
                    break;
                case 0x0E: // LD C, D8
                    LD_R8_D8(ref C);
                    break;
                case 0x0F: // RRCA
                    RRCA();
                    break;
                case 0x10: // STOP 0
                    Running = false;
                    PC += 2;
                    break;
                case 0x11: // LD DE, D16
                    LD_R16_D16(ref D, ref E);
                    break;
                case 0x12: // LD (DE), A
                    LD_PR16_R8(D, E, A);
                    break;
                case 0x13: // INC DE
                    INC_R16(ref D, ref E);
                    break;
                case 0x14: // INC D
                    INC_R8(ref D);
                    break;
                case 0x15: // DEC D
                    DEC_R8(ref D);
                    break;
                case 0x16: // LD D, D8
                    LD_R8_D8(ref D);
                    break;
                case 0x17: // RLA 
                    RLA();
                    break;
                case 0x18: // JR S8
                    JR_S8();
                    break;
                case 0x19: // ADD HL, DE
                    ADD_R16_R16(ref H, ref L, D, E);
                    break;
                case 0x1A: // LD A, (DE)
                    LD_R8_PR16(ref A, D, E);
                    break;
                case 0x1B: // DEC DE
                    DEC_R16(ref D, ref E);
                    break;
                case 0x1C: // INC E
                    INC_R8(ref E);
                    break;
                case 0x1D: // DEC E
                    DEC_R8(ref E);
                    break;
                case 0x1E: // LD E, D8
                    LD_R8_D8(ref E);
                    break;
                case 0x1F: // RRA
                    RRA();
                    break;
                case 0x20: // JR NZ, S8
                    JRNZ_S8();
                    break;
                case 0x21: // LD HL, D16
                    LD_R16_D16(ref H, ref L);
                    break;
                case 0x22: // LD (HL+), A
                    LDI_PR16_R8(ref H, ref L, A);
                    break;
                case 0x23: // INC HL
                    INC_R16(ref H, ref L);
                    break;
                case 0x24: // INC H
                    INC_R8(ref H);
                    break;
                case 0x25: // DEC H
                    DEC_R8(ref H);
                    break;
                case 0x26: // LD D, D8
                    LD_R8_D8(ref D);
                    break;
                case 0x27: // DAA
                    DAA();
                    break;
                case 0x28: // JR Z, S8
                    JRZ_S8();
                    break;
                case 0x29: // ADD HL, HL
                    ADD_R16_R16(ref H, ref L, H, L);
                    break;
                case 0x2A: // LD A, (HL+)
                    LDI_R8_PR16(ref A, ref H, ref L);
                    break;
                case 0x2B: // DEC HL
                    DEC_R16(ref H, ref L);
                    break;
                case 0x2C: // INC L
                    INC_R8(ref L);
                    break;
                case 0x2D: // DEC L
                    DEC_R8(ref L);
                    break;
                case 0x2E: // LD L, D8
                    LD_R8_D8(ref L);
                    break;
                case 0x2F: // CPL
                    CPL();
                    break;
                case 0x30: // JR NC, S8
                    JRNC_S8();
                    break;
                case 0x31: // LD SP, D16
                    LD_SP_D16();
                    break;
                case 0x32: // LD (HL-), A
                    LDD_PR16_R8(ref H, ref L, A);
                    break;
                case 0x33: // INC SP
                    SP++;
                    SP &= 0xFFFF;
                    PC++;
                    break;
                case 0x34: // INC (HL)
                    INC_PR16(H, L);
                    break;
                case 0x35: // DEC (HL)
                    DEC_PR16(H, L);
                    break;
                case 0x36: // LD (HL), D8
                    LD_PR16_D8(H, L);
                    break;
                case 0x37: // SCF
                    SCF();
                    break;
                case 0x38: // JR C, S8
                    JRC_S8();
                    break;
                case 0x39: // ADD HL, SP
                    ADD_R16_R16(ref H, ref L, SP >> 8, SP & 0xFF);
                    break;
                case 0x3A: // LD A, (HL-)
                    LDD_R8_PR16(ref A, ref H, ref L);
                    break;
                case 0x3B: // DEC SP
                    SP--;
                    SP &= 0xFF;
                    PC++;
                    break;
                case 0x3C: // INC A
                    INC_R8(ref A);
                    break;
                case 0x3D: // DEC A
                    DEC_R8(ref A);
                    break;
                case 0x3E: // LD A, D8
                    LD_R8_D8(ref A);
                    break;
                case 0x3F: // CCF
                    CCF();
                    break;
                case 0x40: // LD B, B
                    LD_R8_R8(ref B, B);
                    break;
                case 0x41: // LD B, C
                    LD_R8_R8(ref B, C);
                    break;
                case 0x42: // LD B, D
                    LD_R8_R8(ref B, D);
                    break;
                case 0x43: // LD B, E
                    LD_R8_R8(ref B, E);
                    break;
                case 0x44: // LD B, H
                    LD_R8_R8(ref B, H);
                    break;
                case 0x45: // LD B, L
                    LD_R8_R8(ref B, L);
                    break;
                case 0x46: // LD B, (HL)
                    LD_R8_PR16(ref B, H, L);
                    break;
                case 0x47: // LD B, A
                    LD_R8_R8(ref B, A);
                    break;
                case 0x48: // LD C, B
                    LD_R8_R8(ref C, B);
                    break;
                case 0x49: // LD C, C
                    LD_R8_R8(ref C, C);
                    break;
                case 0x4A: // LD C, D
                    LD_R8_R8(ref C, D);
                    break;
                case 0x4B: // LD C, E
                    LD_R8_R8(ref C, E);
                    break;
                case 0x4C: // LD C, H
                    LD_R8_R8(ref C, H);
                    break;
                case 0x4D: // LD C, L
                    LD_R8_R8(ref C, L);
                    break;
                case 0x4E: // LD C, (HL)
                    LD_R8_PR16(ref C, H, L);
                    break;
                case 0x4F: // LD C, A
                    LD_R8_R8(ref C, A);
                    break;
                case 0x50: // LD D, B
                    LD_R8_R8(ref D, B);
                    break;
                case 0x51: // LD D, C
                    LD_R8_R8(ref D, C);
                    break;
                case 0x52: // LD D, D
                    LD_R8_R8(ref D, D);
                    break;
                case 0x53: // LD D, E
                    LD_R8_R8(ref D, E);
                    break;
                case 0x54: // LD D, H
                    LD_R8_R8(ref D, H);
                    break;
                case 0x55: // LD D, L
                    LD_R8_R8(ref D, L);
                    break;
                case 0x56: // LD D, (HL)
                    LD_R8_PR16(ref D, H, L);
                    break;
                case 0x57: // LD D, A
                    LD_R8_R8(ref D, A);
                    break;
                case 0x58: // LD E, B
                    LD_R8_R8(ref E, B);
                    break;
                case 0x59: // LD E, C
                    LD_R8_R8(ref E, C);
                    break;
                case 0x5A: // LD E, D
                    LD_R8_R8(ref E, D);
                    break;
                case 0x5B: // LD E, E
                    LD_R8_R8(ref E, E);
                    break;
                case 0x5C: // LD E, H
                    LD_R8_R8(ref E, H);
                    break;
                case 0x5D: // LD E, L
                    LD_R8_R8(ref E, L);
                    break;
                case 0x5E: // LD E, (HL)
                    LD_R8_PR16(ref E, H, L);
                    break;
                case 0x5F: // LD E, A
                    LD_R8_R8(ref E, A);
                    break;
                case 0x60: // LD H, B
                    LD_R8_R8(ref H, B);
                    break;
                case 0x61: // LD H, C
                    LD_R8_R8(ref H, C);
                    break;
                case 0x62: // LD H, D
                    LD_R8_R8(ref H, D);
                    break;
                case 0x63: // LD H, E
                    LD_R8_R8(ref H, E);
                    break;
                case 0x64: // LD H, H
                    LD_R8_R8(ref H, H);
                    break;
                case 0x65: // LD H, L
                    LD_R8_R8(ref H, L);
                    break;
                case 0x66: // LD H, (HL)
                    LD_R8_PR16(ref H, H, L);
                    break;
                case 0x67: // LD H, A
                    LD_R8_R8(ref H, A);
                    break;
                case 0x68: // LD L, B
                    LD_R8_R8(ref L, B);
                    break;
                case 0x69: // LD L, C
                    LD_R8_R8(ref L, C);
                    break;
                case 0x6A: // LD L, D
                    LD_R8_R8(ref L, D);
                    break;
                case 0x6B: // LD L, E
                    LD_R8_R8(ref L, E);
                    break;
                case 0x6C: // LD L, H
                    LD_R8_R8(ref L, H);
                    break;
                case 0x6D: // LD L, L
                    LD_R8_R8(ref L, L);
                    break;
                case 0x6E: // LD L, (HL)
                    LD_R8_PR16(ref L, H, L);
                    break;
                case 0x6F: // LD L, A
                    LD_R8_R8(ref L, A);
                    break;
                case 0x70: // LD (HL), B
                    LD_PR16_R8(H, L, B);
                    break;
                case 0x71: // LD (HL), C
                    LD_PR16_R8(H, L, C);
                    break;
                case 0x72: // LD (HL), D
                    LD_PR16_R8(H, L, D);
                    break;
                case 0x73: // LD (HL), E
                    LD_PR16_R8(H, L, E);
                    break;
                case 0x74: // LD (HL), H
                    LD_PR16_R8(H, L, H);
                    break;
                case 0x75: // LD (HL), L
                    LD_PR16_R8(H, L, L);
                    break;
                case 0x76: // HALT
                    HALT();
                    break;
                case 0x77: // LD (HL), A
                    LD_PR16_R8(H, L, A);
                    break;
                case 0x78: // LD A, B
                    LD_R8_R8(ref A, B);
                    break;
                case 0x79: // LD A, C
                    LD_R8_R8(ref A, C);
                    break;
                case 0x7A: // LD A, D
                    LD_R8_R8(ref A, D);
                    break;
                case 0x7B: // LD A, E
                    LD_R8_R8(ref A, E);
                    break;
                case 0x7C: // LD A, H
                    LD_R8_R8(ref A, H);
                    break;
                case 0x7D: // LD A, L
                    LD_R8_R8(ref A, L);
                    break;
                case 0x7E: // LD A, (HL)
                    LD_R8_PR16(ref A, H, L);
                    break;
                case 0x7F: // LD A, A
                    LD_R8_R8(ref A, A);
                    break;
                case 0x80: // ADD A, B
                    ADD_R8_R8(ref A, B);
                    break;
                case 0x81: // ADD A, C
                    ADD_R8_R8(ref A, C);
                    break;
                case 0x82: // ADD A, D
                    ADD_R8_R8(ref A, D);
                    break;
                case 0x83: // ADD A, E
                    ADD_R8_R8(ref A, E);
                    break;
                case 0x84: // ADD A, H
                    ADD_R8_R8(ref A, H);
                    break;
                case 0x85: // ADD A, L
                    ADD_R8_R8(ref A, L);
                    break;
                case 0x86: // ADD A, (HL)
                    ADD_R8_PR16(ref A, H, L);
                    break;
                case 0x87: // ADD A, A
                    ADD_R8_R8(ref A, A);
                    break;
                case 0x88: // ADC A, B
                    ADC_R8_R8(ref A, B);
                    break;
                case 0x89: // ADC A, C
                    ADC_R8_R8(ref A, C);
                    break;
                case 0x8A: // ADC A, D
                    ADC_R8_R8(ref A, D);
                    break;
                case 0x8B: // ADC A, E
                    ADC_R8_R8(ref A, E);
                    break;
                case 0x8C: // ADC A, H
                    ADC_R8_R8(ref A, H);
                    break;
                case 0x8D: // ADC A, L
                    ADC_R8_R8(ref A, L);
                    break;
                case 0x8E: // ADC A, (HL)
                    ADC_R8_PR16(ref A, H, L);
                    break;
                case 0x8F: // ADC A, A
                    ADC_R8_R8(ref A, A);
                    break;
                case 0x90: // SUB B
                    SUB_R8(B);
                    break;
                case 0x91: // SUB C
                    SUB_R8(C);
                    break;
                case 0x92: // SUB D
                    SUB_R8(D);
                    break;
                case 0x93: // SUB E
                    SUB_R8(E);
                    break;
                case 0x94: // SUB H
                    SUB_R8(H);
                    break;
                case 0x95: // SUB L
                    SUB_R8(L);
                    break;
                case 0x96: // SUB (HL)
                    SUB_PR16(H, L);
                    break;
                case 0x97: // SUB A
                    SUB_R8(A);
                    break;
                case 0x98: // SBC A, B
                    SBC_R8_R8(ref A, B);
                    break;
                case 0x99: // SBC A, C
                    SBC_R8_R8(ref A, C);
                    break;
                case 0x9A: // SBC A, D
                    SBC_R8_R8(ref A, D);
                    break;
                case 0x9B: // SBC A, E
                    SBC_R8_R8(ref A, E);
                    break;
                case 0x9C: // SBC A, H
                    SBC_R8_R8(ref A, H);
                    break;
                case 0x9D: // SBC A, L
                    SBC_R8_R8(ref A, L);
                    break;
                case 0x9E: // SBC A, (HL)
                    SBC_R8_PR16(ref A, H, L);
                    break;
                case 0x9F: // SBC A, A
                    SBC_R8_R8(ref A, A);
                    break;
                case 0xA0: // AND B
                    AND_R8(B);
                    break;
                case 0xA1: // AND C
                    AND_R8(C);
                    break;
                case 0xA2: // AND D
                    AND_R8(D);
                    break;
                case 0xA3: // AND E
                    AND_R8(E);
                    break;
                case 0xA4: // AND H
                    AND_R8(H);
                    break;
                case 0xA5: // AND L
                    AND_R8(L);
                    break;
                case 0xA6: // AND B
                    AND_PR16(H, L);
                    break;
                case 0xA7: // AND A
                    AND_R8(A);
                    break;
                case 0xA8: // XOR B
                    XOR_R8(B);
                    break;
                case 0xA9: // XOR C
                    XOR_R8(C);
                    break;
                case 0xAA: // XOR D
                    XOR_R8(D);
                    break;
                case 0xAB: // XOR E
                    XOR_R8(E);
                    break;
                case 0xAC: // XOR H
                    XOR_R8(H);
                    break;
                case 0xAD: // XOR L
                    XOR_R8(L);
                    break;
                case 0xAE: // XOR (HL)
                    XOR_PR16(H, L);
                    break;
                case 0xAF: // XOR A
                    XOR_R8(A);
                    break;
                case 0xB0: // OR B
                    OR_R8(B);
                    break;
                case 0xB1: // OR C
                    OR_R8(C);
                    break;
                case 0xB2: // OR D
                    OR_R8(D);
                    break;
                case 0xB3: // OR E
                    OR_R8(E);
                    break;
                case 0xB4: // OR H
                    OR_R8(H);
                    break;
                case 0xB5: // OR L
                    OR_R8(L);
                    break;
                case 0xB6: // OR (HL)
                    OR_PR16(H, L);
                    break;
                case 0xB7: // OR A
                    OR_R8(A);
                    break;
                case 0xB8: // CP B
                    CP_R8(B);
                    break;
                case 0xB9: // CP C
                    CP_R8(C);
                    break;
                case 0xBA: // CP D
                    CP_R8(D);
                    break;
                case 0xBB: // CP E
                    CP_R8(E);
                    break;
                case 0xBC: // CP H
                    CP_R8(H);
                    break;
                case 0xBD: // CP L
                    CP_R8(L);
                    break;
                case 0xBE: // CP (HL)
                    CP_PR16(H, L);
                    break;
                case 0xBF: // CP A
                    CP_R8(A);
                    break;
                case 0xC0: // RET NZ
                    RETNZ();
                    break;
                case 0xC1: // POP BC
                    POP(ref B, ref C);
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
                    PUSH(B, C);
                    break;
                case 0xC6: // ADD A, D8
                    ADD_R8_D8(ref A);
                    break;
                case 0xC7: // RST 0x00
                    RST(0x00);
                    break;
                case 0xC8: // RET Z
                    RETZ();
                    break;
                case 0xC9: // RET
                    RET();
                    break;
                case 0xCA: // JP Z, A16
                    JPZ_A16();
                    break;
                case 0xCB: // PREFIX CB
                    PC++;
                    ExecutePrefixCB();
                    break;
                case 0xCC: // CALL Z, A16
                    CALLZ_A16();
                    break;
                case 0xCD: // CALL A16
                    CALL_A16();
                    break;
                case 0xCE: // ADC A, D8
                    ADC_R8_D8(ref A);
                    break;
                case 0xCF: // RST 0x08
                    RST(0x08);
                    break;
                case 0xD0: // RET NC
                    RETNC();
                    break;
                case 0xD1: // POP DE
                    POP(ref D, ref E);
                    break;
                case 0xD2: // JP NC, A16
                    JPNC_A16();
                    break;
                case 0xD4: // CALL NC, A16
                    CALLNC_A16();
                    break;
                case 0xD5: // PUSH DE
                    PUSH(D, E);
                    break;
                case 0xD6: // SUB D8
                    SUB_D8();
                    break;
                case 0xD7: // RST 0x10
                    RST(0x10);
                    break;
                case 0xD8: // RET C
                    RETC();
                    break;
                case 0xD9: // RETI
                    RETI();
                    break;
                case 0xDA: // JP C, A16
                    JPC_A16();
                    break;
                case 0xDC: // CALL C, A16
                    CALLC_A16();
                    break;
                case 0xDE: // SBC A, D8
                    SBC_R8_D8(ref A);
                    break;
                case 0xDF: // RST 0x18
                    RST(0x18);
                    break;
                case 0xE0: // LD (A8), A
                    LD_A8_R8(A);
                    break;
                case 0xE1: // POP HL
                    POP(ref H, ref L);
                    break;
                case 0xE2: // LD (C), A
                    LD_PR8_R8(C, A);
                    break;
                case 0xE5: // PUSH HL
                    PUSH(H, L);
                    break;
                case 0xE6: // AND D8
                    AND_D8();
                    break;
                case 0xE7: // RST 0x20
                    RST(0x20);
                    break;
                case 0xE8: // ADD SP, S8
                    ADD_SP_S8();
                    break;
                case 0xE9: // JP (HL)
                    JP_PR16(H, L);
                    break;
                case 0xEA: // LD (A16), A
                    LD_A16_R8(A);
                    break;
                case 0xEE: // XOR D8
                    XOR_D8();
                    break;
                case 0xEF: // RST 0x28
                    RST(0x28);
                    break;
                case 0xF0: // LD A, (A8)
                    LD_R8_A8(ref A);
                    break;
                case 0xF1: // POP AF
                    POP(ref A, ref F);
                    break;
                case 0xF2: // LD A, (C)
                    LD_R8_PR8(ref A, C);
                    break;
                case 0xF3: // DI
                    DI();
                    break;
                case 0xF5: // PUSH AF
                    PUSH(A, F);
                    break;
                case 0xF6: // OR D8
                    OR_D8();
                    break;
                case 0xF7: // RST 0x30
                    RST(0x30);
                    break;
                case 0xF8: // LD HL, SP+S8
                    LD_R16_SP_S8(ref H, ref L);
                    break;
                case 0xF9: // LD SP, HL
                    LD_SP_R16(H, L);
                    break;
                case 0xFA: // LD A, (A16)
                    LD_R8_A16(ref A);
                    break;
                case 0xFB: // EI
                    EI();
                    break;
                case 0xFE: // CP D8:
                    CP_D8();
                    break;
                case 0xFF: // RST 0x38
                    RST(0x38);
                    break;
                default:
                    throw new Exception("Unable to handle opcode 0x" + op.ToString("X"));
            }
        }

        private void ExecutePrefixCB()
        {
            int op = ReadByte(PC);

            switch (op)
            {
                case 0x00: // RLC B
                    RLC_R8(ref B);
                    break;
                case 0x01: // RLC C
                    RLC_R8(ref C);
                    break;
                case 0x02: // RLC D
                    RLC_R8(ref D);
                    break;
                case 0x03: // RLC E
                    RLC_R8(ref E);
                    break;
                case 0x04: // RLC H
                    RLC_R8(ref H);
                    break;
                case 0x05: // RLC L
                    RLC_R8(ref L);
                    break;
                case 0x06: // RLC (HL)
                    RLC_PR16(H, L);
                    break;
                case 0x07: // RLC A
                    RLC_R8(ref A);
                    break;
                case 0x08: // RRC B
                    RRC_R8(ref B);
                    break;
                case 0x09: // RRC C
                    RRC_R8(ref C);
                    break;
                case 0x0A: // RRC D
                    RRC_R8(ref D);
                    break;
                case 0x0B: // RRC E
                    RRC_R8(ref E);
                    break;
                case 0x0C: // RRC H
                    RRC_R8(ref H);
                    break;
                case 0x0D: // RRC L
                    RRC_R8(ref L);
                    break;
                case 0x0E: // RRC (HL)
                    RRC_PR16(H, L);
                    break;
                case 0x0F: // RRC A
                    RRC_R8(ref B);
                    break;
                case 0x10: // RL B
                    RL_R8(ref B);
                    break;
                case 0x11: // RL C
                    RL_R8(ref C);
                    break;
                case 0x12: // RL D
                    RL_R8(ref D);
                    break;
                case 0x13: // RL E
                    RL_R8(ref E);
                    break;
                case 0x14: // RL H
                    RL_R8(ref H);
                    break;
                case 0x15: // RL L
                    RL_R8(ref L);
                    break;
                case 0x16: // RL (HL)
                    RL_PR16(H, L);
                    break;
                case 0x17: // RL A
                    RL_R8(ref A);
                    break;
                case 0x18: // RR B
                    RR_R8(ref B);
                    break;
                case 0x19: // RR C
                    RR_R8(ref C);
                    break;
                case 0x1A: // RR D
                    RR_R8(ref D);
                    break;
                case 0x1B: // RR E
                    RR_R8(ref E);
                    break;
                case 0x1C: // RR H
                    RR_R8(ref H);
                    break;
                case 0x1D: // RR L
                    RR_R8(ref L);
                    break;
                case 0x1E: // RR (HL)
                    RR_PR16(H, L);
                    break;
                case 0x1F: // RR A
                    RR_R8(ref B);
                    break;
                case 0x20: // SLA B
                    SLA_R8(ref B);
                    break;
                case 0x21: // SLA C
                    SLA_R8(ref C);
                    break;
                case 0x22: // SLA D
                    SLA_R8(ref D);
                    break;
                case 0x23: // SLA E
                    SLA_R8(ref E);
                    break;
                case 0x24: // SLA H
                    SLA_R8(ref H);
                    break;
                case 0x25: // SLA L
                    SLA_R8(ref L);
                    break;
                case 0x26: // SLA (HL)
                    SLA_PR16(H, L);
                    break;
                case 0x27: // SLA A
                    SLA_R8(ref A);
                    break;
                case 0x28: // SRA B
                    SRA_R8(ref B);
                    break;
                case 0x29: // SRA C
                    SRA_R8(ref C);
                    break;
                case 0x2A: // SRA D
                    SRA_R8(ref D);
                    break;
                case 0x2B: // SRA E
                    SRA_R8(ref E);
                    break;
                case 0x2C: // SRA H
                    SRA_R8(ref H);
                    break;
                case 0x2D: // SRA L
                    SRA_R8(ref L);
                    break;
                case 0x2E: // SRA (HL)
                    SRA_PR16(H, L);
                    break;
                case 0x2F: // SRA A
                    SRA_R8(ref A);
                    break;
                case 0x30: // SWAP B
                    SWAP_R8(ref B);
                    break;
                case 0x31: // SWAP C
                    SWAP_R8(ref C);
                    break;
                case 0x32: // SWAP D
                    SWAP_R8(ref D);
                    break;
                case 0x33: // SWAP E
                    SWAP_R8(ref E);
                    break;
                case 0x34: // SWAP H
                    SWAP_R8(ref H);
                    break;
                case 0x35: // SWAP L
                    SWAP_R8(ref L);
                    break;
                case 0x36: // SWAP (HL)
                    SWAP_PR16(H, L);
                    break;
                case 0x37: // SWAP A
                    SWAP_R8(ref A);
                    break;
                case 0x38: // SRL B
                    SRL_R8(ref B);
                    break;
                case 0x39: // SRL C
                    SRL_R8(ref C);
                    break;
                case 0x3A: // SRL D
                    SRL_R8(ref D);
                    break;
                case 0x3B: // SRL E
                    SRL_R8(ref E);
                    break;
                case 0x3C: // SRL H
                    SRL_R8(ref H);
                    break;
                case 0x3D: // SRL L
                    SRL_R8(ref L);
                    break;
                case 0x3E: // SRL (HL)
                    SRL_PR16(H, L);
                    break;
                case 0x3F: // SRL A
                    SRL_R8(ref A);
                    break;
                case 0x40: // BIT 0, B
                    BIT_R8(0, B);
                    break;
                case 0x41: // BIT 0, C
                    BIT_R8(0, C);
                    break;
                case 0x42: // BIT 0, D
                    BIT_R8(0, D);
                    break;
                case 0x43: // BIT 0, E
                    BIT_R8(0, E);
                    break;
                case 0x44: // BIT 0, H
                    BIT_R8(0, H);
                    break;
                case 0x45: // BIT 0, L
                    BIT_R8(0, L);
                    break;
                case 0x46: // BIT 0, (HL)
                    BIT_PR16(0, H, L);
                    break;
                case 0x47: // BIT 0, A
                    BIT_R8(0, A);
                    break;
                case 0x48: // BIT 1, B
                    BIT_R8(1, B);
                    break;
                case 0x49: // BIT 1, C
                    BIT_R8(1, C);
                    break;
                case 0x4A: // BIT 1, D
                    BIT_R8(1, D);
                    break;
                case 0x4B: // BIT 1, E
                    BIT_R8(1, E);
                    break;
                case 0x4C: // BIT 1, H
                    BIT_R8(1, H);
                    break;
                case 0x4D: // BIT 1, L
                    BIT_R8(1, L);
                    break;
                case 0x4E: // BIT 1, (HL)
                    BIT_PR16(1, H, L);
                    break;
                case 0x4F: // BIT 1, A
                    BIT_R8(1, A);
                    break;
                case 0x50: // BIT 2, B
                    BIT_R8(2, B);
                    break;
                case 0x51: // BIT 2, C
                    BIT_R8(2, C);
                    break;
                case 0x52: // BIT 2, D
                    BIT_R8(2, D);
                    break;
                case 0x53: // BIT 2, E
                    BIT_R8(2, E);
                    break;
                case 0x54: // BIT 2, H
                    BIT_R8(2, H);
                    break;
                case 0x55: // BIT 2, L
                    BIT_R8(2, L);
                    break;
                case 0x56: // BIT 2, (HL)
                    BIT_PR16(2, H, L);
                    break;
                case 0x57: // BIT 2, A
                    BIT_R8(2, A);
                    break;
                case 0x58: // BIT 3, B
                    BIT_R8(3, B);
                    break;
                case 0x59: // BIT 3, C
                    BIT_R8(3, C);
                    break;
                case 0x5A: // BIT 3, D
                    BIT_R8(3, D);
                    break;
                case 0x5B: // BIT 3, E
                    BIT_R8(3, E);
                    break;
                case 0x5C: // BIT 3, H
                    BIT_R8(3, H);
                    break;
                case 0x5D: // BIT 3, L
                    BIT_R8(3, L);
                    break;
                case 0x5E: // BIT 3, (HL)
                    BIT_PR16(3, H, L);
                    break;
                case 0x5F: // BIT 3, A
                    BIT_R8(3, A);
                    break;
                case 0x60: // BIT 4, B
                    BIT_R8(4, B);
                    break;
                case 0x61: // BIT 4, C
                    BIT_R8(4, C);
                    break;
                case 0x62: // BIT 4, D
                    BIT_R8(4, D);
                    break;
                case 0x63: // BIT 4, E
                    BIT_R8(4, E);
                    break;
                case 0x64: // BIT 4, H
                    BIT_R8(4, H);
                    break;
                case 0x65: // BIT 4, L
                    BIT_R8(4, L);
                    break;
                case 0x66: // BIT 4, (HL)
                    BIT_PR16(4, H, L);
                    break;
                case 0x67: // BIT 4, A
                    BIT_R8(4, A);
                    break;
                case 0x68: // BIT 5, B
                    BIT_R8(5, B);
                    break;
                case 0x69: // BIT 5, C
                    BIT_R8(5, C);
                    break;
                case 0x6A: // BIT 5, D
                    BIT_R8(5, D);
                    break;
                case 0x6B: // BIT 5, E
                    BIT_R8(5, E);
                    break;
                case 0x6C: // BIT 5, H
                    BIT_R8(5, H);
                    break;
                case 0x6D: // BIT 5, L
                    BIT_R8(5, L);
                    break;
                case 0x6E: // BIT 5, (HL)
                    BIT_PR16(5, H, L);
                    break;
                case 0x6F: // BIT 5, A
                    BIT_R8(5, A);
                    break;
                case 0x70: // BIT 6, B
                    BIT_R8(6, B);
                    break;
                case 0x71: // BIT 6, C
                    BIT_R8(6, C);
                    break;
                case 0x72: // BIT 6, D
                    BIT_R8(6, D);
                    break;
                case 0x73: // BIT 6, E
                    BIT_R8(6, E);
                    break;
                case 0x74: // BIT 6, H
                    BIT_R8(6, H);
                    break;
                case 0x75: // BIT 6, L
                    BIT_R8(6, L);
                    break;
                case 0x76: // BIT 6, (HL)
                    BIT_PR16(6, H, L);
                    break;
                case 0x77: // BIT 6, A
                    BIT_R8(6, A);
                    break;
                case 0x78: // BIT 7, B
                    BIT_R8(7, B);
                    break;
                case 0x79: // BIT 7, C
                    BIT_R8(7, C);
                    break;
                case 0x7A: // BIT 7, D
                    BIT_R8(7, D);
                    break;
                case 0x7B: // BIT 7, E
                    BIT_R8(7, E);
                    break;
                case 0x7C: // BIT 7, H
                    BIT_R8(7, H);
                    break;
                case 0x7D: // BIT 7, L
                    BIT_R8(7, L);
                    break;
                case 0x7E: // BIT 7, (HL)
                    BIT_PR16(7, H, L);
                    break;
                case 0x7F: // BIT 7, A
                    BIT_R8(7, A);
                    break;
                case 0x80: // RES 0, B
                    RES_R8(0, ref B);
                    break;
                case 0x81: // RES 0, C
                    RES_R8(0, ref C);
                    break;
                case 0x82: // RES 0, D
                    RES_R8(0, ref D);
                    break;
                case 0x83: // RES 0, E
                    RES_R8(0, ref E);
                    break;
                case 0x84: // RES 0, H
                    RES_R8(0, ref H);
                    break;
                case 0x85: // RES 0, L
                    RES_R8(0, ref L);
                    break;
                case 0x86: // RES 0, (HL)
                    RES_PR16(0, H, L);
                    break;
                case 0x87: // RES 0, A
                    RES_R8(0, ref A);
                    break;
                case 0x88: // RES 1, B
                    RES_R8(1, ref B);
                    break;
                case 0x89: // RES 1, C
                    RES_R8(1, ref C);
                    break;
                case 0x8A: // RES 1, D
                    RES_R8(1, ref D);
                    break;
                case 0x8B: // RES 1, E
                    RES_R8(1, ref E);
                    break;
                case 0x8C: // RES 1, H
                    RES_R8(1, ref H);
                    break;
                case 0x8D: // RES 1, L
                    RES_R8(1, ref L);
                    break;
                case 0x8E: // RES 1, (HL)
                    RES_PR16(1, H, L);
                    break;
                case 0x8F: // RES 1, A
                    RES_R8(1, ref A);
                    break;
                case 0x90: // RES 2, B
                    RES_R8(2, ref B);
                    break;
                case 0x91: // RES 2, C
                    RES_R8(2, ref C);
                    break;
                case 0x92: // RES 2, D
                    RES_R8(2, ref D);
                    break;
                case 0x93: // RES 2, E
                    RES_R8(2, ref E);
                    break;
                case 0x94: // RES 2, H
                    RES_R8(2, ref H);
                    break;
                case 0x95: // RES 2, L
                    RES_R8(2, ref L);
                    break;
                case 0x96: // RES 2, (HL)
                    RES_PR16(2, H, L);
                    break;
                case 0x97: // RES 2, A
                    RES_R8(2, ref A);
                    break;
                case 0x98: // RES 3, B
                    RES_R8(3, ref B);
                    break;
                case 0x99: // RES 3, C
                    RES_R8(3, ref C);
                    break;
                case 0x9A: // RES 3, D
                    RES_R8(3, ref D);
                    break;
                case 0x9B: // RES 3, E
                    RES_R8(3, ref E);
                    break;
                case 0x9C: // RES 3, H
                    RES_R8(3, ref H);
                    break;
                case 0x9D: // RES 3, L
                    RES_R8(3, ref L);
                    break;
                case 0x9E: // RES 3, (HL)
                    RES_PR16(3, H, L);
                    break;
                case 0x9F: // RES 3, A
                    RES_R8(3, ref A);
                    break;
                case 0xA0: // RES 4, B
                    RES_R8(4, ref B);
                    break;
                case 0xA1: // RES 4, C
                    RES_R8(4, ref C);
                    break;
                case 0xA2: // RES 4, D
                    RES_R8(4, ref D);
                    break;
                case 0xA3: // RES 4, E
                    RES_R8(4, ref E);
                    break;
                case 0xA4: // RES 4, H
                    RES_R8(4, ref H);
                    break;
                case 0xA5: // RES 4, L
                    RES_R8(4, ref L);
                    break;
                case 0xA6: // RES 4, (HL)
                    RES_PR16(4, H, L);
                    break;
                case 0xA7: // RES 4, A
                    RES_R8(4, ref A);
                    break;
                case 0xA8: // RES 5, B
                    RES_R8(5, ref B);
                    break;
                case 0xA9: // RES 5, C
                    RES_R8(5, ref C);
                    break;
                case 0xAA: // RES 5, D
                    RES_R8(5, ref D);
                    break;
                case 0xAB: // RES 5, E
                    RES_R8(5, ref E);
                    break;
                case 0xAC: // RES 5, H
                    RES_R8(5, ref H);
                    break;
                case 0xAD: // RES 5, L
                    RES_R8(5, ref L);
                    break;
                case 0xAE: // RES 5, (HL)
                    RES_PR16(5, H, L);
                    break;
                case 0xAF: // RES 5, A
                    RES_R8(5, ref A);
                    break;
                case 0xB0: // RES 6, B
                    RES_R8(6, ref B);
                    break;
                case 0xB1: // RES 6, C
                    RES_R8(6, ref C);
                    break;
                case 0xB2: // RES 6, D
                    RES_R8(6, ref D);
                    break;
                case 0xB3: // RES 6, E
                    RES_R8(6, ref E);
                    break;
                case 0xB4: // RES 6, H
                    RES_R8(6, ref H);
                    break;
                case 0xB5: // RES 6, L
                    RES_R8(6, ref L);
                    break;
                case 0xB6: // RES 6, (HL)
                    RES_PR16(4, H, L);
                    break;
                case 0xB7: // RES 6, A
                    RES_R8(6, ref A);
                    break;
                case 0xB8: // RES 7, B
                    RES_R8(7, ref B);
                    break;
                case 0xB9: // RES 7, C
                    RES_R8(7, ref C);
                    break;
                case 0xBA: // RES 7, D
                    RES_R8(7, ref D);
                    break;
                case 0xBB: // RES 7, E
                    RES_R8(7, ref E);
                    break;
                case 0xBC: // RES 7, H
                    RES_R8(7, ref H);
                    break;
                case 0xBD: // RES 7, L
                    RES_R8(7, ref L);
                    break;
                case 0xBE: // RES 7, (HL)
                    RES_PR16(7, H, L);
                    break;
                case 0xBF: // RES 7, A
                    RES_R8(7, ref A);
                    break;
                case 0xC0: // SET 0, B
                    SET_R8(0, ref B);
                    break;
                case 0xC1: // SET 0, C
                    SET_R8(0, ref C);
                    break;
                case 0xC2: // SET 0, D
                    SET_R8(0, ref D);
                    break;
                case 0xC3: // SET 0, E
                    SET_R8(0, ref E);
                    break;
                case 0xC4: // SET 0, H
                    SET_R8(0, ref H);
                    break;
                case 0xC5: // SET 0, L
                    SET_R8(0, ref L);
                    break;
                case 0xC6: // SET 0, (HL)
                    SET_PR16(0, H, L);
                    break;
                case 0xC7: // SET 0, A
                    SET_R8(0, ref A);
                    break;
                case 0xC8: // SET 1, B
                    SET_R8(1, ref B);
                    break;
                case 0xC9: // SET 1, C
                    SET_R8(1, ref C);
                    break;
                case 0xCA: // SET 1, D
                    SET_R8(1, ref D);
                    break;
                case 0xCB: // SET 1, E
                    SET_R8(1, ref E);
                    break;
                case 0xCC: // SET 1, H
                    SET_R8(1, ref H);
                    break;
                case 0xCD: // SET 1, L
                    SET_R8(1, ref L);
                    break;
                case 0xCE: // SET 1, (HL)
                    SET_PR16(1, H, L);
                    break;
                case 0xCF: // SET 1, A
                    SET_R8(1, ref A);
                    break;
                case 0xD0: // SET 2, B
                    SET_R8(2, ref B);
                    break;
                case 0xD1: // SET 2, C
                    SET_R8(2, ref C);
                    break;
                case 0xD2: // SET 2, D
                    SET_R8(2, ref D);
                    break;
                case 0xD3: // SET 2, E
                    SET_R8(2, ref E);
                    break;
                case 0xD4: // SET 2, H
                    SET_R8(2, ref H);
                    break;
                case 0xD5: // SET 2, L
                    SET_R8(2, ref L);
                    break;
                case 0xD6: // SET 2, (HL)
                    SET_PR16(2, H, L);
                    break;
                case 0xD7: // SET 2, A
                    SET_R8(2, ref A);
                    break;
                case 0xD8: // SET 3, B
                    SET_R8(3, ref B);
                    break;
                case 0xD9: // SET 3, C
                    SET_R8(3, ref C);
                    break;
                case 0xDA: // SET 3, D
                    SET_R8(3, ref D);
                    break;
                case 0xDB: // SET 3, E
                    SET_R8(3, ref E);
                    break;
                case 0xDC: // SET 3, H
                    SET_R8(3, ref H);
                    break;
                case 0xDD: // SET 3, L
                    SET_R8(3, ref L);
                    break;
                case 0xDE: // SET 3, (HL)
                    SET_PR16(3, H, L);
                    break;
                case 0xDF: // SET 3, A
                    SET_R8(3, ref A);
                    break;
                case 0xE0: // SET 4, B
                    SET_R8(4, ref B);
                    break;
                case 0xE1: // SET 4, C
                    SET_R8(4, ref C);
                    break;
                case 0xE2: // SET 4, D
                    SET_R8(4, ref D);
                    break;
                case 0xE3: // SET 4, E
                    SET_R8(4, ref E);
                    break;
                case 0xE4: // SET 4, H
                    SET_R8(4, ref H);
                    break;
                case 0xE5: // SET 4, L
                    SET_R8(4, ref L);
                    break;
                case 0xE6: // SET 4, (HL)
                    SET_PR16(4, H, L);
                    break;
                case 0xE7: // SET 4, A
                    SET_R8(4, ref A);
                    break;
                case 0xE8: // SET 5, B
                    SET_R8(5, ref B);
                    break;
                case 0xE9: // SET 5, C
                    SET_R8(5, ref C);
                    break;
                case 0xEA: // SET 5, D
                    SET_R8(5, ref D);
                    break;
                case 0xEB: // SET 5, E
                    SET_R8(5, ref E);
                    break;
                case 0xEC: // SET 5, H
                    SET_R8(5, ref H);
                    break;
                case 0xED: // SET 5, L
                    SET_R8(5, ref L);
                    break;
                case 0xEE: // SET 5, (HL)
                    SET_PR16(5, H, L);
                    break;
                case 0xEF: // SET 5, A
                    SET_R8(5, ref A);
                    break;
                case 0xF0: // SET 6, B
                    SET_R8(6, ref B);
                    break;
                case 0xF1: // SET 6, C
                    SET_R8(6, ref C);
                    break;
                case 0xF2: // SET 6, D
                    SET_R8(6, ref D);
                    break;
                case 0xF3: // SET 6, E
                    SET_R8(6, ref E);
                    break;
                case 0xF4: // SET 6, H
                    SET_R8(6, ref H);
                    break;
                case 0xF5: // SET 6, L
                    SET_R8(6, ref L);
                    break;
                case 0xF6: // SET 6, (HL)
                    SET_PR16(6, H, L);
                    break;
                case 0xF7: // SET 6, A
                    SET_R8(6, ref A);
                    break;
                case 0xF8: // SET 7, B
                    SET_R8(7, ref B);
                    break;
                case 0xF9: // SET 7, C
                    SET_R8(7, ref C);
                    break;
                case 0xFA: // SET 7, D
                    SET_R8(7, ref D);
                    break;
                case 0xFB: // SET 7, E
                    SET_R8(7, ref E);
                    break;
                case 0xFC: // SET 7, H
                    SET_R8(7, ref H);
                    break;
                case 0xFD: // SET 7, L
                    SET_R8(7, ref L);
                    break;
                case 0xFE: // SET 7, (HL)
                    SET_PR16(7, H, L);
                    break;
                case 0xFF: // SET 7, A
                    SET_R8(7, ref A);
                    break;
                default:
                    throw new Exception("Unable to handle opcode 0xCB" + op.ToString("X"));
            }

            PC++;
        }

        #region Opcode implementation
        
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

        #region Interrupts
        
        private void RETI()
        {
            IME = true;
            WaitForInterrupt = false;
            PC = pop_internal();
        }

        private void EI()
        {
            IME = true;
            PC++;
        }

        private void DI()
        {
            IME = false;
            PC++;
        }

        #endregion

        #region Stack

        private void PUSH(int RH, int RL)
        {
            push_internal((RH << 8) + RL);
            PC++;
        }

        private void POP(ref int RH, ref int RL)
        {
            int v = pop_internal();
            RH = v >> 8;
            RL = v & 0xFF;
            PC++;
        }

        #endregion

        #region Compare

        private void CP_R8(int RA)
        {
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (RA & 0xF);
            FLAG_C = RA > A;
            FLAG_Z = ((A - RA) & 0xFF) == 0;
            PC++;
        }

        private void CP_PR16(int RAH, int RAL)
        {
            int v = ReadByte((RAH << 8) + RAL);
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (v & 0xF);
            FLAG_C = v > A;
            FLAG_Z = ((A - v) & 0xFF) == 0;
            PC++;
        }

        private void CP_D8()
        {
            int v = ReadByte(PC + 1);
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (v & 0xF);
            FLAG_C = v > A;
            FLAG_Z = ((A - v) & 0xFF) == 0;
            PC += 2;
        }

        #endregion

        #region Program control (Jumps, etc.)

        private void JR_S8()
        {
            int relAddr = ReadByte(PC + 1);
            if (relAddr > 0x7F)
            {
                relAddr -= 0xFF;
            }
            PC += relAddr + 1;
        }

        private void JRNZ_S8()
        {
            if (!FLAG_Z)
            {
                JR_S8();
            } else { PC += 2; }
        }

        private void JRZ_S8()
        {
            if (FLAG_Z)
            {
                JR_S8();
            }
            else { PC += 2; }
        }

        private void JRNC_S8()
        {
            if (!FLAG_C)
            {
                JR_S8();
            }
            else { PC += 2; }
        }

        private void JRC_S8()
        {
            if (FLAG_C)
            {
                JR_S8();
            }
            else { PC += 2; }
        }

        private void JP_A16()
        {
            PC = ReadShort(PC + 1);
        }

        private void JPNZ_A16()
        {
            if (!FLAG_Z)
            {
                JP_A16();
            } else { PC += 3; }
        }

        private void JPZ_A16()
        {
            if (FLAG_Z)
            {
                JP_A16();
            } else { PC += 3; }
        }

        private void JPNC_A16()
        {
            if (!FLAG_C)
            {
                JP_A16();
            } else { PC += 3; }
        }

        private void JPC_A16()
        {
            if (FLAG_C)
            {
                JP_A16();
            } else { PC += 3; }
        }

        private void JP_PR16(int RH, int RL)
        {
            PC = ReadShort((RH << 8) + RL);
        }

        private void CALL_A16()
        {
            push_internal(PC + 3);
            PC = ReadShort(PC + 1);
        }

        private void CALLNZ_A16()
        {
            if (!FLAG_Z)
            {
                CALL_A16();
            } else { PC += 3; }
        }

        private void CALLZ_A16()
        {
            if (FLAG_Z)
            {
                CALL_A16();
            } else { PC += 3; }
        }

        private void CALLNC_A16()
        {
            if (!FLAG_C)
            {
                CALL_A16();
            } else { PC += 3; }
        }

        private void CALLC_A16()
        {
            if (FLAG_C)
            {
                CALL_A16();
            } else { PC += 3; }
        }

        private void HALT()
        {
            WaitForInterrupt = true;
            PC++;
        }

        private void RET()
        {
            PC = pop_internal();
        }

        private void RETNZ()
        {
            if (!FLAG_Z)
            {
                RET();
            } else { PC++; }
        }

        private void RETZ()
        {
            if (FLAG_Z)
            {
                RET();
            }
            else { PC++; }
        }

        private void RETNC()
        {
            if (!FLAG_C)
            {
                RET();
            }
            else { PC++; }
        }

        private void RETC()
        {
            if (FLAG_C)
            {
                RET();
            }
            else { PC++; }
        }

        private void RST(int address)
        {
            push_internal(PC);
            PC = address;
        }

        #endregion

        #region LOAD

        private void LD_R8_R8(ref int RA, int RB)
        {
            RA = RB;
            PC++;
        }

        private void LD_R16_D16(ref int RH, ref int RL)
        {
            RH = ReadByte(PC + 2);
            RL = ReadByte(PC + 1);
            PC += 3;
        }

        private void LD_PR16_R8(int RAH, int RAL, int RB)
        {
            WriteByte(RAH << 8 + RAL, RB);
            PC++;
        }

        private void LD_R8_D8(ref int R)
        {
            R = ReadByte(PC + 1);
            PC += 2;
        }

        private void LD_A16_R8(int R)
        {
            WriteShort( (ReadByte(PC + 2) << 8) + ReadByte(PC + 1), R);
            PC += 3;
        }

        private void LD_A16_R16(int RH, int RL)
        {
            WriteShort(ReadShort(PC + 1), (RH << 8) + RL);
            PC += 3;
        }

        private void LD_R8_A16(ref int R)
        {
            R = ReadByte(ReadShort(PC + 1));
            PC += 3;
        }

        private void LD_R8_PR16(ref int RA, int RBH, int RBL)
        {
            RA = ReadByte((RBH << 8) + RBL);
            PC++;
        }

        private void LD_PR16_D8(int RAH, int RAL)
        {
            WriteByte((RAH << 8) + RAL, ReadByte(PC + 1));
            PC += 2;
        }

        private void LD_SP_D16()
        {
            SP = ReadShort(PC + 1);
            PC += 3;
        }

        private void LD_SP_R16(int RH, int RL)
        {
            SP = (RH << 8) + RL;
            PC++;
        }

        private void LD_R16_SP_S8(ref int RH, ref int RL)
        {
            int v = ReadByte(PC + 1);
            if (v > 0x7F)
            {
                v -= 255;
            }
            RL = (SP & 0xFF) + v;
            FLAG_HC = RL > 0xFF;
            RL &= 0xFF;
            RH = (SP >> 8) + (FLAG_HC ? 1 : 0);
            FLAG_C = RH > 0xFF;
            RH &= 0xFF;
            PC += 2;
        }

        private void LDI_PR16_R8(ref int RAH, ref int RAL, int RB)
        {
            LDI_PR16_R8(ref RAH, ref RAL, RB);
            INC_R16(ref RAH, ref RAL);
            PC -= 1;
        }

        private void LDI_R8_PR16(ref int RA, ref int RBH, ref int RBL)
        {
            LD_R8_PR16(ref RA, RBH, RBL);
            INC_R16(ref RBH, ref RBL);
            PC -= 1;
        }

        private void LDD_PR16_R8(ref int RAH, ref int RAL, int RB)
        {
            LDI_PR16_R8(ref RAH, ref RAL, RB);
            DEC_R16(ref RAH, ref RAL);
            PC -= 1;
        }

        private void LDD_R8_PR16(ref int RA, ref int RBH, ref int RBL)
        {
            LD_R8_PR16(ref RA, RBH, RBL);
            DEC_R16(ref RBH, ref RBL);
            PC -= 1;
        }

        private void LD_A8_R8(int RA)
        {
            WriteByte(0xFF00 + ReadByte(PC + 1), RA);
            PC += 2;
        }

        private void LD_R8_A8(ref int RA)
        {
            RA = ReadByte(0xFF00 + ReadByte(PC + 1));
            PC += 2;
        }

        private void LD_PR8_R8(int RA, int RB)
        {
            WriteByte(0xFF00 + RA, RB);
            PC++;
        }

        private void LD_R8_PR8(ref int RA, int RB)
        {
            RA = ReadByte(0xFF00 + RB);
            PC++;
        }

        #endregion

        #region MATH

        private void INC_R16(ref int RH, ref int RL)
        {
            if (RL == 0xFF)
            {
                RH = (RH + 1) & 0xFF;
                RL = 0;
            } else {
                RL++;
            }
            PC++;
        }

        private void INC_R8(ref int R)
        {
            FLAG_N = false;
            FLAG_HC = (R & 0xF) == 0xF;
            R++;
            R &= 0xFF;
            FLAG_Z = R == 0;
            PC++;
        }

        private void INC_PR16(int RH, int RL)
        {
            int addr = (RH << 8) + RL;
            int v_orig = ReadByte(addr);
            int v = (v_orig + 1) & 0xFF;
            WriteByte(addr, v);
            FLAG_Z = v == 0;
            FLAG_N = false;
            FLAG_HC = (v_orig & 0xF) == 0xF;
            PC++;
        }

        private void DEC_R16(ref int RH, ref int RL)
        {
            if (RL == 0)
            {
                RH = (RH - 1) & 0xFF;
                RL = 0xFF;
            } else
            {
                RL--;
            }
            PC++;
        }

        private void DEC_R8(ref int R)
        {
            FLAG_N = true;
            FLAG_HC = (R & 0xF) == 0x0;
            R--;
            R &= 0xFF;
            FLAG_Z = R == 0;
            PC++;
        }

        private void DEC_PR16(int RH, int RL)
        {
            int addr = (RH << 8) + RL;
            int v_orig = ReadByte(addr);
            int v = (v_orig - 1) & 0xFF;
            WriteByte(addr, v);
            FLAG_Z = v == 0;
            FLAG_N = true;
            FLAG_HC = (v_orig & 0xF) == 0x0;
            PC++;
        }

        private void ADD_R16_R16(ref int RAH, ref int RAL, int RBH, int RBL)
        {
            int carry;
            FLAG_N = false;
            RAL += RBL;
            carry = (RAL > 0xFF) ? 1 : 0;
            RAL &= 0xFF;
            FLAG_HC = carry + (RAH & 0xF) + (RBH & 0xF) > 0xF;
            RAH += RBH + carry;
            FLAG_C = RAH > 0xFF;
            RAH &= 0xFF;
            PC++;
        }

        private void ADD_R8_R8(ref int RA, int RB)
        {
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (RB & 0xF) > 0xF;
            RA += RB;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void ADD_R8_PR16(ref int RA, int RBH, int RBL)
        {
            int v = ReadByte((RBH << 8) + RBL);
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (v & 0xF) > 0xF;
            RA += v;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void ADD_R8_D8(ref int RA)
        {
            int v = ReadByte(PC + 1);
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (v & 0xF) > 0xF;
            RA += v;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC += 2;
        }

        private void ADD_SP_S8()
        {
            int v = ReadByte(PC++);
            if (v > 0x7F)
            {
                v -= 256;
            }
            SP += v;
        }

        private void ADC_R8_R8(ref int RA, int RB)
        {
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (RB & 0xF) + carry_bit > 0xF;
            RA += RB + carry_bit;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void ADC_R8_PR16(ref int RA, int RBH, int RBL)
        {
            int v = ReadByte((RBH << 8) + RBL);
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (v & 0xF) + carry_bit > 0xF;
            RA += v + carry_bit;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void ADC_R8_D8(ref int RA)
        {
            int v = ReadByte(PC + 1);
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = false;
            FLAG_HC = (RA & 0xF) + (v & 0xF) + carry_bit > 0xF;
            RA += v + carry_bit;
            FLAG_C = RA > 0xFF;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC += 2;
        }

        private void SUB_R8(int RA)
        {
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (RA & 0xF);
            FLAG_C = RA > A;
            A -= RA;
            A &= 0xFF;
            FLAG_Z = A == 0;
            PC++;
        }

        private void SUB_PR16(int RAH, int RAL)
        {
            int v = ReadByte((RAH << 8) + RAL);
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (v & 0xF);
            FLAG_C = v > A;
            A -= v;
            A &= 0xFF;
            FLAG_Z = A == 0;
            PC++;
        }

        private void SUB_D8()
        {
            int v = ReadByte(PC + 1);
            FLAG_N = true;
            FLAG_HC = (A & 0xF) < (v & 0xF);
            FLAG_C = v > A;
            A -= v;
            A &= 0xFF;
            FLAG_Z = A == 0;
            PC += 2;
        }

        private void SBC_R8_R8(ref int RA, int RB)
        {
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = true;
            FLAG_HC = (RA & 0xF) < ((RB + carry_bit) & 0xF);
            FLAG_C = (RB + carry_bit) > RA;
            RA -= (RB + carry_bit);
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void SBC_R8_PR16(ref int RA, int RAH, int RAL)
        {
            int carry_bit = FLAG_C ? 1 : 0;
            int v = ReadByte((RAH << 8) + RAL) + carry_bit; 
            FLAG_N = true;
            FLAG_HC = (RA & 0xF) < (v & 0xF);
            FLAG_C = v > RA;
            RA -= v;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC++;
        }

        private void SBC_R8_D8(ref int RA)
        {
            int carry_bit = FLAG_C ? 1 : 0;
            int v = ReadByte(PC + 1) + carry_bit;
            FLAG_N = true;
            FLAG_HC = (RA & 0xF) < (v & 0xF);
            FLAG_C = v > RA;
            RA -= v;
            RA &= 0xFF;
            FLAG_Z = RA == 0;
            PC += 2;
        }

        #endregion

        #region BIT OPERATIONS

        private void RLCA()
        {
            int high_bit = A >> 7;
            FLAG_Z = false;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = high_bit == 1;
            A = ((A << 1) & 0xFF) | high_bit;
            PC++;
        }

        private void RRCA()
        {
            int low_bit = A & 1;
            FLAG_Z = false;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = low_bit == 1;
            A = (A >> 1) | (low_bit << 7);
            PC++;
        }

        private void RLA()
        {
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_Z = false;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (A >> 7) == 1;
            A = ((A << 1) & 0xFF) | carry_bit;
            PC++;
        }

        private void RRA()
        {
            int carry_bit = FLAG_C ? 0x80 : 0x00;
            FLAG_Z = false;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (A & 1) == 1;
            A = (A >> 1) | carry_bit;
            PC++;
        }

        private void CPL()
        {
            FLAG_HC = true;
            FLAG_N = true;
            A ^= 0xFF;
            PC++;
        }

        private void AND_R8(int RA)
        {
            A &= RA;
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = true;
            FLAG_C = false;
            PC++;
        }

        private void AND_PR16(int RAH, int RAL)
        {
            A &= ReadByte((RAH << 8) + RAL);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = true;
            FLAG_C = false;
            PC++;
        }

        private void AND_D8()
        {
            A &= ReadByte(PC + 1);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = true;
            FLAG_C = false;
            PC += 2;
        }

        private void XOR_R8(int RA)
        {
            A ^= RA;
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC++;
        }

        private void XOR_PR16(int RAH, int RAL)
        {
            A ^= ReadByte((RAH << 8) + RAL);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC++;
        }

        private void XOR_D8()
        {
            A ^= ReadByte(PC + 1);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC += 2;
        }

        private void OR_R8(int RA)
        {
            A |= RA;
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC++;
        }

        private void OR_PR16(int RAH, int RAL)
        {
            A |= ReadByte((RAH << 8) + RAL);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC++;
        }

        private void OR_D8()
        {
            A |= ReadByte(PC + 1);
            FLAG_Z = A == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            PC += 2;
        }

        #endregion

        #region Flags

        private void SCF()
        {
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = true;
            PC++;
        }

        private void CCF()
        {
            FLAG_C = !FLAG_C;
            FLAG_HC = !FLAG_HC;
            FLAG_N = false;
            PC++;
        }

        #endregion

        #region BCD

        private void DAA()
        {
            if ((A & 0xF) > 9 || FLAG_HC) A += 0x6;
            if ((A >> 4) > 9 || FLAG_C) A += 0x60;
            A &= 0xFF;
            PC++;
        }

        #endregion

        #region Prefix CB

        private void RLC_R8(ref int R)
        {
            int high_bit = R >> 7;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = high_bit == 1;
            R = ((R << 1) & 0xFF) | high_bit;
            FLAG_Z = R == 0;
        }

        private void RLC_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            int high_bit = v >> 7;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = high_bit == 1;
            v = ((v << 1) & 0xFF) | high_bit;
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void RRC_R8(ref int R)
        {
            int low_bit = R & 1;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = low_bit == 1;
            R = (R >> 1) | (low_bit << 7);
            FLAG_Z = R == 0;
        }

        private void RRC_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            int low_bit = v & 1;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = low_bit == 1;
            v = (v >> 1) | (low_bit << 7);
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void RL_R8(ref int R)
        {
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (R >> 7) == 1;
            R = ((R << 1) & 0xFF) | carry_bit;
            FLAG_Z = R == 0;
        }

        private void RL_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            int carry_bit = FLAG_C ? 1 : 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (v >> 7) == 1;
            v = ((v << 1) & 0xFF) | carry_bit;
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void RR_R8(ref int R)
        {
            int carry_bit = FLAG_C ? 0x80 : 0x00;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (R & 1) == 1;
            R = (R >> 1) | carry_bit;
            FLAG_Z = R == 0;
        }

        private void RR_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            int carry_bit = FLAG_C ? 0x80 : 0x00;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (v & 1) == 1;
            v = (v >> 1) | carry_bit;
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void SLA_R8(ref int R)
        {
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (R >> 7) == 1;
            R <<= 1;
            R &= 0xFF;
            FLAG_Z = R == 0;
        }

        private void SLA_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (v >> 7) == 1;
            v <<= 1;
            v &= 0xFF;
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void SRA_R8(ref int R)
        {
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            R = (R & 0x80) + (R >> 1);
            FLAG_Z = R == 0;
        }

        private void SRA_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            v = (v & 0x80) + (v >> 1);
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void SRL_R8(ref int R)
        {
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (R & 1) == 1;
            R >>= 1;
            R &= 0xFF;
            FLAG_Z = R == 0;
        }

        private void SRL_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = (v & 1) == 1;
            v >>= 1;
            v &= 0xFF;
            FLAG_Z = v == 0;
            WriteByte(address, v);
        }

        private void SWAP_R8(ref int R)
        {
            R = ((R << 4) | (R >> 4)) & 0xFF;
            FLAG_Z = R == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
        }

        private void SWAP_PR16(int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            v = ((v << 4) | (v >> 4)) & 0xFF;
            FLAG_Z = v == 0;
            FLAG_N = false;
            FLAG_HC = false;
            FLAG_C = false;
            WriteByte(address, v);
        }

        private void BIT_R8(int n, int R)
        {
            FLAG_Z = (R & (1 << n)) == 0;
            FLAG_N = false;
            FLAG_HC = true;
        }

        private void BIT_PR16(int n, int RH, int RL)
        {
            FLAG_Z = (ReadByte((RH << 8) + RL) & (1 << n)) == 0;
            FLAG_N = false;
            FLAG_HC = true;
        }

        private void RES_R8(int n, ref int R)
        {
            R = R & (0xFF - (1 << n));
        }

        private void RES_PR16(int n, int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            v = v & (0xFF - (1 << n));
            WriteByte(address, v);
        }

        private void SET_R8(int n, ref int R)
        {
            R = R | (1 << n);
        }

        private void SET_PR16(int n, int RH, int RL)
        {
            int address = (RH << 8) + RL;
            int v = ReadByte(address);
            v = v | (1 << n);
            WriteByte(address, v);
        }

        #endregion

        #endregion

        // Stack
        private void push_internal(int v)
        {
            WriteShort(SP, v);
            SP -= 2;
        }

        private int pop_internal()
        {
            int v = (int)ReadShort(SP);
            SP += 2;
            return v;
        }

        // Interrupting
        public void Interrupt(int address)
        {
            IME = false;
            push_internal(PC);
            PC = address;
        }

        // Memory Helpers
        private byte ReadByte(int address)
        {
            return this.Memory.ReadByte(address);
        }

        private ushort ReadShort(int address)
        {
            return (ushort)((this.Memory.ReadByte(address + 1) << 8) + this.Memory.ReadByte(address));
        }

        private void WriteByte(int address, int value)
        {
            this.Memory.WriteByte(address, (byte)value);
        }

        private void WriteShort(int address, int value)
        {
            this.Memory.WriteByte(address, (byte)(value & 0xFF));
            this.Memory.WriteByte(address + 1, (byte)(value >> 8));
        }

    }
}
