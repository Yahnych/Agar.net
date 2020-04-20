using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agar.Utils
{
    public unsafe class DataReader
    {
        public readonly byte[] Data;
        public readonly int Length;
        private int _offset;
        public int Offset { get; }
        public DataReader(byte[] data)
        {
            Data = data;
            Length = data.Length;
        }
        public byte ReadByte()
            => Data[_offset++];
        public T Read<T>() where T : unmanaged
        {

            int size = sizeof(T);
            T ret;
            byte* t = (byte*)&ret;
            for (int i = 0; i < size; i++)
                t[i] = Data[_offset++];

            return ret;
        }
        public void Advance(int count) => _offset += count;
        public string ReadUTF16String()
        {
            int start = this._offset, index = this._offset;
            while (index + 2 < this.Length && this.Read<ushort>() != 0)
                index += 2;
            return Encoding.Unicode.GetString(Data, start, index - start);
        }
        public string ReadUTF8String()
        {
            int start = this._offset, index = this._offset;
            while (index + 1 < this.Length && this.ReadByte() != 0)
                index++;
            return Encoding.UTF8.GetString(Data, start, index - start);
        }
    }
}
