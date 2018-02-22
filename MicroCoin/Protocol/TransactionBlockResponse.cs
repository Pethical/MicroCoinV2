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

        public TransactionBlockResponse()
        {
            RequestType = Net.RequestType.Response;
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    bw.Write((uint)List.Count);
                    foreach (var c in List)
                    {
                        (c as TransactionBlock).SaveToStream(ms);
                    }
                }                
                using(BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
                {
                    DataLength = (int)ms.Length;
                    bw.Write(DataLength);
                }
                ms.CopyTo(s);
            }
        }

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
