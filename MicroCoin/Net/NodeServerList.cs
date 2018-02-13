using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MicroCoin.Net
{
    public class NodeServer
    {
        public byte[] IP { get; set; }
        public ushort Port { get; set; }
        public uint LastConnection { get; set; }
        public string IPAddress
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(IP);
            }
        }
        public IPEndPoint EndPoint
        {
            get
            {
                return new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port);
            }
        }
        internal static void LoadFromStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class NodeServerList : List<NodeServer>
    {
        public void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write((uint)0);
            }
        }

        public static NodeServerList LoadFromStream(Stream stream)
        {
            NodeServerList ns = new NodeServerList();
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                uint serverCount = br.ReadUInt32();
                for(int i = 0; i < serverCount; i++)
                {
                    NodeServer server = new NodeServer();
                    ushort iplen = br.ReadUInt16();
                    server.IP = br.ReadBytes(iplen);
                    server.Port = br.ReadUInt16();
                    server.LastConnection = br.ReadUInt32();
                    ns.Add(server);
                }
            }
            return ns;
        }
    }
}