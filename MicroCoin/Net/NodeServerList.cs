﻿// This file is part of MicroCoin.
// 
// Copyright (c) 2018 Peter Nemeth
//
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.


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
        public string IPAddress => Encoding.ASCII.GetString(IP);

        public IPEndPoint EndPoint => new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port);

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