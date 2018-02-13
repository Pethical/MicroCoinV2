using System;
using System.IO;
using System.Text;

namespace MCC
{
    public class HelloResponse : Response
    {
        public ushort ServerPort { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public int Timestamp { get; set; }
        public OperationBlock OperationBlock { get; set; }
        public NodeServerList NodeServers { get; set; }
        public string Version { get; set; }
        public Int64 WorkSum { get; set; }

        public void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ServerPort = br.ReadUInt16();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream);
                Timestamp = br.ReadInt32();
                OperationBlock = new OperationBlock(stream);
                NodeServers = NodeServerList.LoadFromStream(stream);
                ushort vlen = br.ReadUInt16();
                byte[] vb = br.ReadBytes(vlen);
                Version = Encoding.ASCII.GetString(vb);
                WorkSum = br.ReadInt64();
            }

        }

        public HelloResponse(Stream stream) : base(stream)
        {
            LoadFromStream(stream);
        }

        public HelloResponse(Stream stream, Response rp) : base(rp)
        {
            LoadFromStream(stream);
        }
    }
}
