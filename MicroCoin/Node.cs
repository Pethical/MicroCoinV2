//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// Node.cs - Copyright (c) 2018 Németh Péter
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
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MicroCoin.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroCoin.Chain;
using System.IO;
using MicroCoin.Protocol;
using MicroCoin.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MicroCoin.Util;

namespace MicroCoin
{
    public class Node
    {        
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Node s_instance;
        public ECKeyPair AccountKey { get; } = ECKeyPair.CreateNew(false);
        private static MicroCoinClient MicroCoinClient { get; set; }
        public NodeServerList NodeServers { get; set; } = new NodeServerList();        
        public BlockChain BlockChain { get; set; } = BlockChain.Instance;
        public static Node Instance
        {
            get
            {
                if (s_instance == null) s_instance = new Node();
                return s_instance;
            }
            set => s_instance = value;
        }
        public Thread listenerThread { get; set; }
        public List<MicroCoinClient> Clients { get; set; } = new List<MicroCoinClient>();
        public Node()
        {
            
        }
        public static async Task<Node> StartNode(int port=4004)
        {

            MicroCoinClient = new MicroCoinClient();
            //MicroCoinClient.Connect("micro-225.microbyte.cloud", 4004);
            try
            {
                MicroCoinClient.Connect("127.0.0.1", port);
                MicroCoinClient.ServerPort = (ushort)((IPEndPoint)MicroCoinClient.TcpClient.Client.LocalEndPoint).Port;
                CheckPoints.Init();
                HelloResponse response = await MicroCoinClient.SendHelloAsync();

                uint start = (response.Block.BlockNumber / 100) * 100;
                int bl = BlockChain.Instance.BlockHeight();
                while (bl <= response.Block.BlockNumber)
                {
                    var blocks = await MicroCoinClient.RequestBlocksAsync((uint)bl, 1000); //response.TransactionBlock.BlockNumber);
                    log.Info($"BlockChain downloading {bl} => {bl + 999}");
                    log.Info(blocks.Blocks.First().BlockNumber.ToString() + " " + blocks.Blocks.Last().BlockNumber.ToString());
                    log.Info(blocks.Blocks.Count.ToString());
                    BlockChain.Instance.AppendAll(blocks.Blocks, true);
                    bl += 1000;
                    using (FileStream fs = File.Open(BlockChain.Instance.BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        BlockChain.Instance.SaveToStorage(fs);
                    }
                }
		        log.Info("BlockChain OK");
                if (!File.Exists(CheckPoints.checkPointFileName))
                {
                    var cpList = CheckPoint.BuildFromBlockChain(BlockChain.Instance);
                    FileStream cpFile = File.Create(CheckPoints.checkPointFileName);
                    FileStream indexFile = File.Create(CheckPoints.checkPointIndexName);
                    CheckPoint.SaveList(cpList, cpFile, indexFile);
                    Hash hash = CheckPoint.CheckPointHash(cpList);
                    cpFile.Dispose();
                    indexFile.Dispose();
                }
                if (File.Exists(CheckPoints.checkPointFileName))
                {
                    CheckPoints.Init();
                    uint blocks = CheckPoints.GetLastBlock().BlockNumber;
                    uint need = BlockChain.Instance.GetLastBlock().BlockNumber;
                    for (uint i = blocks; i <= need; i++)
                    {
                        CheckPoints.AppendBlock(BlockChain.Instance.Get((int)i));
                    }
                }
                else
                {
                    throw new FileNotFoundException("Checkpoint file not found", CheckPoints.checkPointFileName);
                }
                GC.Collect();
                MicroCoinClient.HelloResponse += (o, e) =>
                {
                    log.DebugFormat("Network CheckPointBlock height: {0}. My CheckPointBlock height: {1}", e.HelloResponse.Block.BlockNumber, BlockChain.Instance.BlockHeight());
                    if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                    {
                        MicroCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                    }
                };
                MicroCoinClient.BlockResponse += (ob, eb) =>
                {
                    log.DebugFormat("Received {0} CheckPointBlock from blockchain. BlockChain size: {1}. CheckPointBlock height: {2}", eb.BlockResponse.Blocks.Count, BlockChain.Instance.Count, eb.BlockResponse.Blocks.Last().BlockNumber);
                    BlockChain.Instance.AppendAll(eb.BlockResponse.Blocks);
                };
                MicroCoinClient.Dispose();
                Instance.NodeServers.TryAddNew("127.0.0.1:4004", new NodeServer
                {
                    IP = "127.0.0.1",
                    LastConnection = DateTime.Now,
                    Port=4004                    
                });
                //MicroCoinClient.Start();
                //MicroCoinClient.SendHello();
            }
            catch(Exception e)
            {
                log.Error(e.Message, e);
                throw e;
            }
            Instance.Listen();
            return Instance;
        }
        protected void Listen()
        {
            listenerThread = new Thread(() =>
            {
                try
                {
                    try
                    {
                        log.Warn("starting listener");
                        TcpListener tcpListener = new TcpListener(IPAddress.Any, 4041); //
                        log.Warn("started listener");
                        try
                        {
                            // TcpListener tcpListener = new TcpListener((IPEndPoint)MicroCoinClient.TcpClient.Client.LocalEndPoint);
                            MicroCoinClient.ServerPort = 4040;
                            //tcpListener.AllowNatTraversal(true);
                            tcpListener.Start();
                            ManualResetEvent connected = new ManualResetEvent(false);
                            while (true)
                            {
                                connected.Reset();
                                var asyncResult = tcpListener.BeginAcceptTcpClient((state) =>
                                {
                                    try
                                    {
                                        var client = tcpListener.EndAcceptTcpClient(state);
                                        log.Warn($"New client {client.Client.RemoteEndPoint}");
                                        MicroCoinClient mClient = new MicroCoinClient();
                                        mClient.Disconnected += (o, e)=>{
                                            Clients.Remove((MicroCoinClient)o);                                            
                                        };
                                        Clients.Add(mClient);
                                        mClient.Handle(client);
                                        connected.Set();                                        
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                        return;
                                    }
                                }, null);
                                while (!connected.WaitOne(1)) ;
                            }
                        }
                        catch (ThreadAbortException ta)
                        {
                            tcpListener.Stop();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        //log.Error(e.Message, e);
                        return;
                    }
                }
                finally
                {
                    log.Warn("Listener exited");
                }
            });
            listenerThread.Start();
        }
        public void Dispose()
        {
            foreach (var c in Clients)
            {
                if (!c.IsDisposed)
                {
                    c.Dispose();
                    Clients.Remove(c);
                }
            }
            if(listenerThread!=null) listenerThread.Abort();
            MicroCoinClient.Dispose();
            NodeServers.Dispose();
        }
    }
}
