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
    public class HelloRequest : Request
    {
        public ushort ServerPort { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public int Timestamp { get; set; }
        public TransactionBlock OperationBlock { get; set; }
        public NodeServerList NodeServers { get; set; }
        public string Version { get; set; }
        public Int64 WorkSum { get; set; }
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
                    OperationBlock.SaveToStream(ms);
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
    }
}
