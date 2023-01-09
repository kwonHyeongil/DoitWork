using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ma3012sock
{
    public class Packet
    {
        // Fields
        private byte[] packetData;

        // Methods
        public Packet(uint size)
        {
            this.packetData = new byte[size];
        }

        public Packet(byte[] bytes)
        {
            this.packetData = bytes;
        }

        public byte GetByte(uint index)
        {
            return this.packetData[index];
        }

        public byte[] GetBytes()
        {
            return this.GetBytes(0);
        }

        public byte[] GetBytes(uint index)
        {
            return this.GetBytes(index, ((uint)this.packetData.Length) - index);
        }

        public byte[] GetBytes(uint index, uint length)
        {
            byte[] destinationArray = new byte[length];
            Array.Copy(this.packetData, (long)index, destinationArray, 0L, (long)length);
            return destinationArray;
        }
        public string GetString(uint index)
        {
            return System.Convert.ToChar(this.packetData[index]).ToString();
        }
        public string GetString(uint index, uint length)
        {
            byte[] stream = new byte[length];
            Array.Copy(this.packetData, (long)index, stream, 0L, (long)length);
            string result = Encoding.ASCII.GetString(stream);
            return result;
        }
        public float GetFloat(uint index)
        {
            return Endian.Swap(BitConverter.ToSingle(this.packetData, (int)index));
        }

        public float GetFloat_NONESwap(uint index)
        {
            return BitConverter.ToSingle(this.packetData, (int)index);
        }
        public float GetFloat_NONESwap(uint index, uint length)
        {
            byte[] stream = new byte[length];
            Array.Copy(this.packetData, (long)index, stream, 0L, (long)length);
            return BitConverter.ToSingle(stream, (int)index);
        }

        public int GetInt(int index)
        {
            return Endian.Swap(BitConverter.ToInt32(this.packetData, index));
        }

        public long GetLong(uint index)
        {
            return Endian.Swap(BitConverter.ToInt64(this.packetData, (int)index));
        }

        public short GetShort(uint index)
        {
            return Endian.Swap(BitConverter.ToInt16(this.packetData, (int)index));
        }

        public uint GetUInt(uint index)
        {
            return Endian.Swap(BitConverter.ToUInt32(this.packetData, (int)index));
        }

        public ulong GetULong(uint index)
        {
            return Endian.Swap(BitConverter.ToUInt64(this.packetData, (int)index));
        }

        public ushort GetUShort(uint index)
        {
            return Endian.Swap(BitConverter.ToUInt16(this.packetData, (int)index));
        }

        public ushort GetUShort_NONESwap(uint index)
        {
            return BitConverter.ToUInt16(this.packetData, (int)index);
        }



        public void Resize(uint newSize)
        {
            Array.Resize<byte>(ref this.packetData, (int)newSize);
        }

        public void SetByte(uint index, byte byteToSet)
        {
            this.packetData[index] = byteToSet;
        }

        public void SetBytes(uint index, byte[] bytesToSet)
        {
            Array.Copy(bytesToSet, 0L, this.packetData, (long)index, (long)bytesToSet.Length);
        }

        public void SetFloat(uint index, float floatToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(floatToSet));
            this.SetBytes(index, bytes);
        }

        public void SetInt(uint index, int intToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(intToSet));
            this.SetBytes(index, bytes);
        }

        public void SetLong(uint index, long longToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(longToSet));
            this.SetBytes(index, bytes);
        }

        public void SetUInt(uint index, uint uintToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(uintToSet));
            this.SetBytes(index, bytes);
        }

        public void SetULong(uint index, ulong ulongToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(ulongToSet));
            this.SetBytes(index, bytes);
        }

        public void SetUShort(uint index, ushort ushortToSet)
        {
            byte[] bytes = BitConverter.GetBytes(Endian.Swap(ushortToSet));
            this.SetBytes(index, bytes);
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.packetData.Length; i++)
            {
                builder.AppendFormat("0x{0:X02}", this.packetData[i]);
                if (i < (this.packetData.Length - 1))
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        // Properties
        public byte[] Data
        {
            get
            {
                return this.packetData;
            }
        }

        public uint Length
        {
            get
            {
                return (uint)this.packetData.Length;
            }
        }
    }
}
