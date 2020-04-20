using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agar.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Int24
    {
        private byte _0;
        private byte _1;
        private byte _2;
        public Int24(int value)
        {
            _0 = (byte)(value & 0xff);
            _1 = (byte)((value >> 8) & 0xff);
            _2 = (byte)((value >> 16) & 0xff);
        }
        public int Value
        {
            get
            {
                int val = _0;
                val |= _1;
                val |= _2;
                return val;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt24
    {
        private byte _0;
        private byte _1;
        private byte _2;
        public UInt24(uint value)
        {
            _0 = (byte)(value & 0xff);
            _1 = (byte)((value >> 8) & 0xff);
            _2 = (byte)((value >> 16) & 0xff);
        }
        public uint Value
        {
            get
            {
                uint val = _0;
                val |= _1;
                val |= _2;
                return val;
            }
        }
    }
}
