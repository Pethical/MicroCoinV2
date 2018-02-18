using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Protocol
{
    public class SnapshopRequest : MessageHeader
    {
        public uint SnapshotBlockCount { get; set; }
        public ByteString SnapshotHash { get; set; }
        public uint StartBlock { get; set; }
        public uint EndBlock { get; set; }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using(BinaryWriter bw = new BinaryWriter(s,Encoding.Default, true))
            {
                DataLength = 4 + 2 + 32 + 4 + 4;
                bw.Write(DataLength);
                bw.Write(SnapshotBlockCount);
                SnapshotHash.SaveToStream(bw);
                bw.Write(StartBlock);
                bw.Write(EndBlock);
            }
        }
    }
}
