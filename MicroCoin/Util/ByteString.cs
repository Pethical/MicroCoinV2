using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroCoin.Util
{
    public struct ByteString
    {

        private byte[] value;

        public ByteString(byte[] b)
        {
            value = b;
        }

        public int Length { get
            {
                return value.Length;
            }
        }

        public static implicit operator ByteString(string s)
        {
            return new ByteString(Encoding.Default.GetBytes(s));
        }

        public static implicit operator string(ByteString s)
        {
            return Encoding.Default.GetString(s.value);
        }

        public static implicit operator byte[](ByteString s)
        {
            return s.value;
        }

        public static implicit operator ByteString(byte[] s)
        {
            return new ByteString(s);
        }

        public static ByteString ReadFromStream(BinaryReader br)
        {
            ByteString bs = new ByteString();
            ushort len = br.ReadUInt16();
            bs = br.ReadBytes(len);
            return bs;
        }

        public void SaveToStream(BinaryWriter bw)
        {
            value.SaveToStream(bw);
        }

        override public string ToString()
        {
            return this;
        }

        internal void SaveToStream(BinaryWriter bw, bool writeLengths)
        {
            bw.Write(value);
        }
    }



    public struct Hash
    {

        private byte[] value;

        public Hash(byte[] b)
        {
            value = b;
        }

        public int Length
        {
            get
            {
                return value.Length;
            }        
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        public static implicit operator Hash(string s)
        {
            return new Hash(StringToByteArray(s));
        }

        public static implicit operator string(Hash s)
        {
            return BitConverter.ToString(s).Replace("-", "");
        }

        public static implicit operator ByteString(Hash s)
        {
            return new ByteString(s);
        }


        public static implicit operator byte[] (Hash s)
        {
            return s.value;
        }

        public static implicit operator Hash(byte[] s)
        {
            return new Hash(s);
        }

        public static implicit operator Hash(ByteString s)
        {
            return new Hash(s);
        }

        public static Hash ReadFromStream(BinaryReader br)
        {
            Hash bs = new Hash();
            ushort len = br.ReadUInt16();
            bs = br.ReadBytes(len);
            return bs;
        }

        public void SaveToStream(BinaryWriter bw)
        {
            value.SaveToStream(bw);
        }

        override public string ToString()
        {
            return this;
        }

        internal void SaveToStream(BinaryWriter bw, bool writeLengths)
        {
            bw.Write(value);
        }
    }


}