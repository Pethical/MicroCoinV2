using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class Response : RequestHeader
    {
        public Response(Stream stream)
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                Magic = br.ReadUInt32();
                RequestType = (RequestType)br.ReadUInt16();
                Operation = (NetOperationType)br.ReadUInt16();
                Error = br.ReadUInt16();
                RequestId = br.ReadUInt32();
                ProtocolVersion = br.ReadUInt16();
                AvailableProtocol = br.ReadUInt16();
                DataLength = br.ReadInt32();
                if(Operation == NetOperationType.GetBlocks)
                {
                }
            }
        }

        public Response(Response rp)
        {
            Magic = rp.Magic;
            RequestType = rp.RequestType;
            Operation = rp.Operation;
            Error = rp.Error;
            RequestId = rp.RequestId;
            ProtocolVersion = rp.ProtocolVersion;
            AvailableProtocol = rp.AvailableProtocol;
            DataLength = rp.DataLength;
        }
    }
}
