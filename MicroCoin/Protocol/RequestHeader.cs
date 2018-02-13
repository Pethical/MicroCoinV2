using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class RequestHeader
    {
        public uint Magic { get; set; }
        public RequestType RequestType { get; set; }
        public NetOperationType Operation { get; set; }
        public ushort Error { get; set; }
        public uint RequestId { get; set; }
        public ushort ProtocolVersion { get; set; } = 2;
        public ushort AvailableProtocol { get; set; } = 2;
        public int DataLength { get; set; }

        public RequestHeader()
        {
            Magic = 0x0A043580;
            RequestType = RequestType.Request;
            Operation = NetOperationType.Hello;
            ProtocolVersion = 2;
            AvailableProtocol = 2;
            Error = 0;
        }

        public virtual void SaveToStream(Stream s)
        {
            using (BinaryWriter br = new BinaryWriter(s, Encoding.ASCII, true))
            {
                br.Write(Magic);
                br.Write((ushort)RequestType);
                br.Write((ushort)Operation);
                br.Write(Error);
                br.Write(RequestId);
                br.Write(ProtocolVersion);
                br.Write(AvailableProtocol);
            }
        }
    }
}
