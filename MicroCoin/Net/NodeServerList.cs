//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// NodeServerList.cs - Copyright (c) 2018 Németh Péter
//-----------------------------------------------------------------------
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//-----------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using log4net;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace MicroCoin.Net
{

    public class NewConnectionEventArgs : EventArgs
    {
        public NodeServer Node { get; set; }

        public NewConnectionEventArgs(NodeServer node)
        {
            Node = node;
        }
    }

    public class NodeServerList : ConcurrentDictionary<string, NodeServer>, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<NewConnectionEventArgs> NewNode;

        public ConcurrentDictionary<string, NodeServer> BlackList { get; set; } = new ConcurrentDictionary<string, NodeServer>();

        internal void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write((uint)Count);
                foreach(var item in this)
                {
                    item.Value.IP.SaveToStream(bw);
                    bw.Write(item.Value.Port);
                    bw.Write(item.Value.LastConnection);
                }
            }
        }

        protected void OnNewConnection(NodeServer newNode)
        {
            NewNode?.Invoke(this, new NewConnectionEventArgs(newNode));
        }

        public void BroadCastMessage(Stream message)
        {
            foreach (var item in this)
            {
                item.Value.MicroCoinClient.SendRaw(message);
            }
        }

        internal void TryAddNew(string key, NodeServer nodeServer)
        {
            if (BlackList.ContainsKey(key)) return;
            if (ContainsKey(key)) return;
            Log.Debug($"{Count} nodes registered");
            new Thread(() =>
            {
                var microCoinClient = nodeServer.Connect();
                if (microCoinClient != null && nodeServer.Connected)
                {
                    TryAdd(key, nodeServer);
                    OnNewConnection(nodeServer);
                }
                else
                {
                    Log.Debug($"{nodeServer} dead");
                    BlackList.TryAdd(key, nodeServer);
                    TryRemove(key, out NodeServer outs);
                    var cnt = this.Count(p => p.Value.Connected);
                    Log.Debug($"{Count} nodes registered. {cnt} connected. {BlackList.Count} dead");
                }

            })
            {
                Name = nodeServer.ToString()
            }.Start();
        }

        internal static NodeServerList LoadFromStream(Stream stream)
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
                    ns.TryAdd(server.ToString(), server);
                }
            }
            return ns;
        }

        internal void UpdateNodeServers(NodeServerList nodeServers)
        {
            foreach (var n in nodeServers)
            {
                if (ContainsKey(n.Value.ToString())) continue;
                TryAddNew(n.Value.ToString(), n.Value);
                Log.Debug($"New node server: {n.Value}");
            }

            if (Count <= 100) return;
            {
                foreach (var l in nodeServers)
                {
                    TryRemove(l.Key, out NodeServer n);
                    Log.Debug($"Removed connection: {n}");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            foreach (var n in this)
            {
                n.Value.MicroCoinClient.Dispose();
            }
        }
    }
    }