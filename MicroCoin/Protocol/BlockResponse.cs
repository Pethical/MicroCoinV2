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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class BlockResponse : MessageHeader
    {
        public List<BlockTransactionList> BlockTransactions { get; set; }
        public uint TransactionCount { get; set; }

        public BlockResponse(Stream stream) : base(stream)
        {
        }

        public BlockResponse(Stream stream, MessageHeader rp) :base(rp)
        {
            BlockTransactions = new List<BlockTransactionList>();
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                TransactionCount = br.ReadUInt32();
                for(int i = 0; i < TransactionCount; i++)
                {
                    if (stream.Position >= stream.Length - 1) {
                        Console.WriteLine("Position");
                        break;
                    }                    
                    BlockTransactionList op = new BlockTransactionList(stream);
                    BlockTransactions.Add(op);
                }
            }
        }
    }
}
