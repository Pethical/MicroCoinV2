using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCoin.Net.Discovery
{
    public enum DiscoveryCommand { HelloRequest, HelloResponse, NodeListResponse, NodeListRequest };
    public class DiscoveryMessage
    {
        public DiscoveryCommand Command { get; set; }
        public ushort PayloadLength
        {
            get
            {
                if(Payload!=null)
                    return (ushort) Payload.Length;
                return 0;
            }
        }
        public byte[] Payload { get; set; }
        public int Length
        {
            get
            {
                return PayloadLength + sizeof(int) +sizeof(ushort);
            }
        }

        public DiscoveryMessage() { }

        public DiscoveryMessage(byte[] data)
        {
            int dt = (data[3] << 24 | data[2] << 16 | data[1] << 8 | data[0]);
            Command = (DiscoveryCommand)dt;
            ushort PayloadLength =  (ushort)((data[4] << 8) | (data[2]));
            Payload = data.Skip(6).ToArray();
        }

        public byte[] ToByteArray()
        {            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((int)Command);
                    bw.Write(PayloadLength);
                    if (PayloadLength > 0)
                    {
                        bw.Write(Payload, 0, Payload.Length);
                    }
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }
        public override string ToString() => Payload==null?"":Encoding.ASCII.GetString(Payload);
        public static DiscoveryMessage FromString(DiscoveryCommand Command, string message)
        {
            return new DiscoveryMessage
            {
                Payload = Encoding.UTF8.GetBytes(message),
                Command = Command
            };
        }
    }
}
