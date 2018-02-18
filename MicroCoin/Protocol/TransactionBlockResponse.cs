using MicroCoin.Chain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Protocol
{
    public class TransactionBlockResponse : MessageHeader
    {
        public List<TransactionBlock> List { get; set; }
        public TransactionBlockResponse(Stream stream) : base(stream)
        {
        }

        public TransactionBlockResponse(Stream stream, MessageHeader rp) : base(rp)
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                List = new List<TransactionBlock>(br.ReadInt32());
            }
            
            for(int i = 0; i < List.Capacity; i++)
            {
                List.Add(new TransactionBlock(stream));
            }
        }

    }
}
