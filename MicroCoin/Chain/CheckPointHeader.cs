// This file is part of MicroCoin.
// 
// Copyright (c) 2018 Peter Nemeth
//
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.


using MicroCoin.Util;
using System.IO;
using System.Text;

namespace MicroCoin.Chain
{
    public class CheckPointHeader
    {
        public ByteString Magic { get; set; }
        public ushort Protocol { get; set; }
        public ushort Version { get; set; }
        public uint BlockCount { get; set; }
        public uint StartBlock { get; set; }
        public uint EndBlock { get; set; }
        public Hash Hash { get; set; }
        public long HeaderEnd { get; set; }
        public string MagicString
        {
            get => Encoding.ASCII.GetString(Magic);

        }

        public uint[] offsets { get; set; }

        public uint BlockOffset(uint blockNumber)
        {
            if (blockNumber > offsets.Length) return uint.MaxValue;

            return offsets[blockNumber];// + (int)HeaderEnd;

        }
        public CheckPointHeader() { }
        public CheckPointHeader(Stream s)
        {
            LoadFromStream(s);            
        }

        public void SaveToStream(BinaryWriter bw)
        {                    
            Magic.SaveToStream(bw);
            bw.Write(Protocol);
            bw.Write(Version);
            bw.Write(BlockCount);
            bw.Write(StartBlock);
            bw.Write(EndBlock);            
            if (offsets != null)
            {
                foreach (var b in offsets)
                {
                    if (b > 27)
                    {
                        bw.Write((uint)(b-27));
                    }
                    else
                    {
                        bw.Write((uint)(b));
                    }
                }
            }
        }

        public void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
            {
                SaveToStream(bw);
            }
        }

        public void LoadFromStream(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                long position = s.Position;
                s.Position = s.Length - 34;
                Hash = ByteString.ReadFromStream(br);
                s.Position = position;
                ushort len = br.ReadUInt16();
                Magic = br.ReadBytes(len);
                Protocol = br.ReadUInt16();
                Version = br.ReadUInt16();
                BlockCount = br.ReadUInt32();
                StartBlock = br.ReadUInt32();
                EndBlock = br.ReadUInt32();
                long pos = s.Position;
                HeaderEnd = pos;
                offsets = new uint[(EndBlock-StartBlock+2)];
                for (int i = 0; i < offsets.Length; i++)
                {
                    offsets[i] = (uint)(br.ReadUInt32() + pos);
                }
            }
        }
    }
}