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


using MicroCoin.BlockChain;
using MicroCoin.Cryptography;
using MicroCoin.Net;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class HelloResponse : Response
    {
        public ushort ServerPort { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public int Timestamp { get; set; }
        public TransactionBlock TransactionBlock { get; set; }
        public NodeServerList NodeServers { get; set; }
        public string Version { get; set; }
        public Int64 WorkSum { get; set; }

        public void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ServerPort = br.ReadUInt16();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream);
                Timestamp = br.ReadInt32();
                TransactionBlock = new TransactionBlock(stream);
                NodeServers = NodeServerList.LoadFromStream(stream);
                ushort vlen = br.ReadUInt16();
                byte[] vb = br.ReadBytes(vlen);
                Version = Encoding.ASCII.GetString(vb);
                WorkSum = br.ReadInt64();
            }

        }

        public HelloResponse(Stream stream) : base(stream)
        {
            LoadFromStream(stream);
        }

        public HelloResponse(Stream stream, Response rp) : base(rp)
        {
            LoadFromStream(stream);
        }
    }
}
