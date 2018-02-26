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
//-------------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using log4net;
using MicroCoin.Util;
using MicroCoin.Chain;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MicroCoin.Transactions;
using MicroCoin.Protocol;

namespace MicroCoin.Net
{
    public class NodeServer
    {

        public ByteString IP { get; set; }

        public ushort Port { get; set; }

        public Timestamp LastConnection { get; set; }

        public string IPAddress => Encoding.ASCII.GetString(IP);

        public IPEndPoint EndPoint => new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port);

        public TcpClient TcpClient { get; set; }

        public bool Connected { get; set; } = false;

        public MicroCoinClient MicroCoinClient { get; set; }

        internal static void LoadFromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return IP +":"+ Port.ToString();
        }

        private object clientLock = new object();

        public MicroCoinClient Connect()
        {
            lock (clientLock)
            {
                if (!Connected)
                {
                    MicroCoinClient = new MicroCoinClient();
                    if(!MicroCoinClient.Connect(IPAddress, Port))
                    {
                        return null;
                    }
                    MicroCoinClient.Start();
                    if (MicroCoinClient.Connected)
                    {
                        Connected = true;
                        return MicroCoinClient;
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
        }
    }

    public class NodeServerList : ConcurrentDictionary<string, NodeServer>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConcurrentDictionary<string, NodeServer> BlackList { get; set; } = new ConcurrentDictionary<string, NodeServer>();

        private object addLock = new object();

        private object tLock = new object();

        public void SaveToStream(Stream s)
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
        private List<string> transmitted = new List<string>();

        public void TryAddNew(string key, NodeServer nodeServer)
        {
            //lock (addLock)
            {
                if (BlackList.ContainsKey(key)) return;
                if (ContainsKey(key)) return;
                if (TryAdd(key, nodeServer))
                {
                    log.Debug($"{this.Count} nodes registered");
                    new Thread(() =>
                    {
                        var MicroCoinClient = nodeServer.Connect();
                        if (MicroCoinClient != null && nodeServer.Connected)
                        {
                            MicroCoinClient.HelloResponse += (o, e) =>
                            {
                                log.DebugFormat("Network CheckPointBlock height: {0}. My CheckPointBlock height: {1}", e.HelloResponse.Block.BlockNumber, BlockChain.Instance.BlockHeight());
                                if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                                {
                                    MicroCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                                }
                            };
                            MicroCoinClient.BlockResponse += (ob, eb) => {
                                log.DebugFormat("Received {0} CheckPointBlock from blockchain. BlockChain size: {1}. CheckPointBlock height: {2}", eb.BlockResponse.Blocks.Count, BlockChain.Instance.Count, eb.BlockResponse.Blocks.Last().BlockNumber);
                                BlockChain.Instance.AppendAll(eb.BlockResponse.Blocks);
                            };
                            MicroCoinClient.NewTransaction += (o, e) =>
                            {
                                string hash = e.Transaction.GetHash();
                                if (transmitted.Contains(hash))
                                {
                                    log.Info("Transaction already sent. Skipping.");
                                    return;
                                }
                                var client = (MicroCoinClient)o;
                                var ip = ((IPEndPoint)client.TcpClient.Client.RemoteEndPoint).Address.ToString();
                                transmitted.Add(hash);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    foreach (var c in this)
                                    {
                                        if (c.Value.IPAddress != ip)
                                        {
                                            ms.Position = 0;
                                            c.Value.MicroCoinClient.SendRaw(ms);
                                            log.Info($"Sent incoming transaction to {c.Value.IPAddress}");
                                        }
                                    }
                                }
                            };
                            MicroCoinClient.SendHello();
                        }
                        else
                        {
                            log.Debug($"{nodeServer} dead");
                            BlackList.TryAdd(key, nodeServer);
                            TryRemove(key, out NodeServer outs);
                            var cnt = this.Count(p => p.Value.Connected);
                            log.Debug($"{this.Count} nodes registered. {cnt} connected. {BlackList.Count} dead");
                        }

                    })
                    {
                        Name = nodeServer.ToString()
                    }
                    .Start();
                }
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
                    ns.TryAdd(server.ToString(), server);
                }
            }
            return ns;
        }

        public void UpdateNodeServers(NodeServerList nodeServers)
        {
            foreach (var n in nodeServers)
            {
                if (!ContainsKey(n.Value.ToString()))
                {
                    TryAddNew(n.Value.ToString(), n.Value);
                    log.Debug($"New node server: {n.Value}");
                }
            }
            if (Count > 100)
            {
                var list = this.OrderByDescending(p => p.Value.LastConnection).Take(100 - Count);
                foreach (var l in nodeServers)
                {
                    TryRemove(l.Key, out NodeServer n);
                    log.Debug($"Removed connection: {n}");
                }
            }
        }

        internal void Dispose()
        {
            foreach(var n in this)
            {
                n.Value.MicroCoinClient.Dispose();
            }
        }
    }
}