using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class BlockRequest : Request
    {
        public uint StartBlock { get; set; }
        public uint BlockNumber { get; set; }
        public BlockRequest()
        {
            RequestType = RequestType.Request;
            Operation = NetOperationType.GetBlocks;
            RequestId = 2;
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using(BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true)) {
                DataLength = 8;
                bw.Write(DataLength);
                bw.Write(StartBlock);
                bw.Write(BlockNumber+StartBlock-1);
            }
        }
    }
}
