﻿using System;
using System.IO;
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
        public int Length()
        {
            return value.Length;
        }

        public static ByteString ReadFromStream(BinaryReader br)
        {
            ByteString bs = new ByteString();
            ushort len = br.ReadUInt16();
            bs = br.ReadBytes(len);
            return bs;
        }

        override public string ToString()
        {
            return (string)this;
        }

    }
}