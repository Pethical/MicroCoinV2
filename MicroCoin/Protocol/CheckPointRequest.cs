using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Protocol
{
    public class CheckPointRequest : MessageHeader
    {
        public uint checkPointBlockCount { get; set; }
        public ByteString CheckPointHash { get; set; }
        public uint StartBlock { get; set; }
        public uint EndBlock { get; set; }
        public CheckPointRequest(Stream s, MessageHeader rp) : base(rp)
        {
            using(BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                checkPointBlockCount = br.ReadUInt32();
                CheckPointHash = ByteString.ReadFromStream(br);
                StartBlock = br.ReadUInt32();
                EndBlock = br.ReadUInt32();
            }
        }
        public CheckPointRequest()
        {

        }
        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using(BinaryWriter bw = new BinaryWriter(s,Encoding.Default, true))
            {
                DataLength = 4 + 2 + 32 + 4 + 4;
                bw.Write(DataLength);
                bw.Write(checkPointBlockCount);
                CheckPointHash.SaveToStream(bw);
                bw.Write(StartBlock);
                bw.Write(EndBlock);
            }
        }
    }
}
