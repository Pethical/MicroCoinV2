using System;
using System.IO;
using System.Text;

namespace MCC
{
    public class HelloRequest : Request
    {
        public ushort ServerPort { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public int Timestamp { get; set; }
        public OperationBlock OperationBlock { get; set; }
        public NodeServerList NodeServers { get; set; }
        public string Version { get; set; }
        public Int64 WorkSum { get; set; }
        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII, true))
                {
                    bw.Write(ServerPort);
                    AccountKey.SaveToStream(ms);
                    bw.Write(Timestamp);
                    OperationBlock.SaveToStream(ms);
                    NodeServers.SaveToStream(ms);
                    byte[] vb = Encoding.ASCII.GetBytes(Version);
                    bw.Write((ushort)vb.Length);
                    bw.Write(vb);
                    bw.Write(WorkSum);
                }
                DataLength = (int)ms.Length;
                using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
                {
                    bw.Write(DataLength);
                }
                ms.Position = 0;
                ms.CopyTo(s);
            }
        }
    }
}
