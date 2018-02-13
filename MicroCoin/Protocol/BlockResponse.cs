using MicroCoin.BlockChain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class BlockResponse : Response
    {
        public List<BlockTransactionList> OperationBlocks { get; set; }
        public uint OpCount { get; set; }

        public BlockResponse(Stream stream) : base(stream)
        {
            BlockTransactionList op = new BlockTransactionList(stream);
        }

        public BlockResponse(Stream stream, Response rp) :base(rp)
        {
            OperationBlocks = new List<BlockTransactionList>();
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                OpCount = br.ReadUInt32();
                for(int i = 0; i < OpCount; i++)
                {
                    if (stream.Position >= stream.Length - 1) {
                        Console.WriteLine("Position");
                        break;
                    }                    
                    BlockTransactionList op = new BlockTransactionList(stream);
                    OperationBlocks.Add(op);
                }
            }
        }
    }
}
