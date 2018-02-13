using MicroCoin.BlockChain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class BlockResponse : Response
    {
        public List<BlockTransactionList> BlockTransactions { get; set; }
        public uint TransactionCount { get; set; }

        public BlockResponse(Stream stream) : base(stream)
        {
            BlockTransactionList op = new BlockTransactionList(stream);
        }

        public BlockResponse(Stream stream, Response rp) :base(rp)
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
