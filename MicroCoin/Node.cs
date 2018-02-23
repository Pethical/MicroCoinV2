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

namespace MicroCoin
{
    public class Node
    {        
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Node s_instance;
        public ECKeyPair AccountKey { get; } = ECKeyPair.CreateNew(false);
        private static MicroCoinClient MicroCoinClient { get; set; }
        public NodeServerList NodeServers { get; set; } = new NodeServerList();
        public CheckPoint CheckPoint { get; set; } = new CheckPoint();
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
        public static async Task<Node> StartNode()
        {           
            MicroCoinClient = new MicroCoinClient();
            //MicroCoinClient.Connect("micro-225.microbyte.cloud", 4004);
            try
            {
                MicroCoinClient.Connect("127.0.0.1", 4004);
                MicroCoinClient.ServerPort = (ushort)((IPEndPoint)MicroCoinClient.TcpClient.Client.LocalEndPoint).Port;
                HelloResponse response = await MicroCoinClient.SendHelloAsync();
                uint start = (response.Block.BlockNumber / 100) * 100;
                int bl = BlockChain.Instance.BlockHeight();
		        while(bl < response.Block.BlockNumber) {
            	    var blocks = await MicroCoinClient.RequestBlocksAsync((uint)bl, 1000); //response.TransactionBlock.BlockNumber);
		            log.Info($"BlockChain downloading {bl} => {bl+999}");
		            log.Info(blocks.Blocks.First().BlockNumber.ToString()+" "+blocks.Blocks.Last().BlockNumber.ToString());
		            log.Info(blocks.Blocks.Count.ToString());
            	    BlockChain.Instance.AppendAll(blocks.Blocks);
		            bl+=1000;
                    using (FileStream fs = File.Open(BlockChain.Instance.BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        BlockChain.Instance.SaveToStorage(fs);
                    }
                }
		        log.Info("BlockChain OK");
                if (File.Exists(CheckPoint.CheckPointFileName))
                {
                    Instance.CheckPoint.LoadFromFile(CheckPoint.CheckPointFileName);
                    log.Info($"{Instance.CheckPoint.Header.EndBlock} {start} {response.Block.BlockNumber}");
                }
                else
                {
                    await MicroCoinClient.DownloadCheckPointAsync(response.Block.BlockNumber);
		    log.Info("CheckPoints ok");
                    FileStream file = File.Create(CheckPoint.CheckPointFileName);
                    Instance.CheckPoint.SaveToStream(file);
                    file.Dispose();
                    Instance.CheckPoint.LoadFromFile(CheckPoint.CheckPointFileName);
                }
                if (Instance.CheckPoint.Header.EndBlock < start - 1)
                {
                    Instance.CheckPoint.Dispose();
                    Instance.CheckPoint = new CheckPoint();
                    await MicroCoinClient.DownloadCheckPointAsync(response.Block.BlockNumber);
                    FileStream file = File.Create(CheckPoint.CheckPointFileName);
                    Instance.CheckPoint.SaveToStream(file);
                    file.Dispose();
                    Instance.CheckPoint.LoadFromFile(CheckPoint.CheckPointFileName);
                }
                GC.Collect();
                foreach (Account a in Instance.CheckPoint.Accounts)
                {
                    if (a.Balance != 1000000 && a.Balance != 0)
                        log.Info($"{a.AccountNumber} => {a.Balance}");
                }
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
                MicroCoinClient.Start();
                MicroCoinClient.SendHello();
            }
            catch
            {

            }
//            Instance.Listen();
            return Instance;
        }

        protected void Listen()
        {
            listenerThread = new Thread(() =>
            {
                try
                {
                    TcpListener tcpListener = new TcpListener(IPAddress.Any, 4040); //
                    try
                    {
                        // TcpListener tcpListener = new TcpListener((IPEndPoint)MicroCoinClient.TcpClient.Client.LocalEndPoint);
                        MicroCoinClient.ServerPort = 4040;
                        tcpListener.AllowNatTraversal(true);
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
                                    MicroCoinClient mClient = new MicroCoinClient();
                                    Clients.Add(mClient);
                                    mClient.Handle(client);
                                }
                                catch (ObjectDisposedException)
                                {
                                    return;
                                }
                            }, null);
                            while (true)
                            {
                                connected.WaitOne(1);
                            }
                        }
                    }
                    catch (ThreadAbortException ta)
                    {
                        tcpListener.Stop();
                        return;
                    }
                }
                catch
                {
                    return;
                }

            });
            listenerThread.Start();
        }

        internal void Dispose()
        {
            foreach (var c in Clients)
            {
                if (!c.IsDisposed)
                {
                    c.Dispose();
                    Clients.Remove(c);
                }
            }
            listenerThread.Abort();
            MicroCoinClient.Dispose();
            NodeServers.Dispose();            
            CheckPoint.Dispose();
        }
    }
}
