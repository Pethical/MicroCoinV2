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


using MicroCoin.Chain;
using MicroCoin.Cryptography;
using MicroCoin.Net;
using MicroCoin.Util;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class HelloRequest : Request
    {

        public ushort ServerPort { get; set; }

        public ECKeyPair AccountKey { get; set; }

        public Timestamp Timestamp { get; set; }

        public TransactionBlock TransactionBlock { get; set; }

        public NodeServerList NodeServers { get; set; }

        public string Version { get; set; }

        public Int64 WorkSum { get; set; }

        public HelloRequest() : base() { }

        public HelloRequest(Stream stream, MessageHeader rp) : base(rp)
        {
            LoadFromStream(stream);
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    bw.Write(ServerPort);
                    AccountKey.SaveToStream(ms);
                    bw.Write(Timestamp);
                    TransactionBlock.SaveToStream(ms);
                    NodeServers.SaveToStream(ms);
                    byte[] vb = Encoding.ASCII.GetBytes(Version);
                    bw.Write((ushort)vb.Length);
                    bw.Write(vb);
                    bw.Write(WorkSum);
                }
                DataLength = (int)ms.Length;
                using (BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
                {
                    bw.Write(DataLength);
                }
                ms.Position = 0;
                ms.CopyTo(s);                
           }
        }

        public void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ServerPort = br.ReadUInt16();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream);
                Timestamp = br.ReadUInt32();
                TransactionBlock = new TransactionBlock(stream);
                NodeServers = NodeServerList.LoadFromStream(stream);
                ushort vlen = br.ReadUInt16();
                byte[] vb = br.ReadBytes(vlen);
                Version = Encoding.ASCII.GetString(vb);
                WorkSum = br.ReadInt64();
            }

        }
    }
}
