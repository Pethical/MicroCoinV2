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

namespace MicroCoin
{
    public class Node
    {        
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Node s_instance;
        public ECKeyPair AccountKey { get; } = ECKeyPair.CreateNew(false);
        private static MicroCoinClient MicroCoinClient { get; set; }
        public NodeServerList NodeServers { get; set; } = new NodeServerList();
        public Snapshot Snapshot { get; set; } = new Snapshot();
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
        public Node()
        {
            
        }
        public static async Task<Node> StartNode()
        {
            MicroCoinClient = new MicroCoinClient();
            MicroCoinClient.Connect("micro-225.microbyte.cloud", 4004);
            HelloResponse response = await MicroCoinClient.SendHelloAsync();
            uint start = (response.TransactionBlock.BlockNumber / 100) * 100;
            var blocks = await MicroCoinClient.RequestBlocksAsync(start, response.TransactionBlock.BlockNumber);
            BlockChain.Instance.AddRange(blocks.BlockTransactions);
            using (FileStream fs = File.Create(BlockChain.Instance.BlockChainFileName))
            {
                BlockChain.Instance.SaveToStorage(fs);
            }
            await MicroCoinClient.DownloadSnaphostAsync(response.TransactionBlock.BlockNumber);
            FileStream file = File.Create($"snaphot");
            Instance.Snapshot.SaveToStream(file);            
            file.Dispose();
            Instance.Snapshot.LoadFromFile("snaphot");
            GC.Collect();
            MicroCoinClient.HelloResponse += (o, e) =>
            {
                log.DebugFormat("Network Block height: {0}. My Block height: {1}", e.HelloResponse.TransactionBlock.BlockNumber, BlockChain.Instance.BlockHeight());
                if (BlockChain.Instance.BlockHeight() < e.HelloResponse.TransactionBlock.BlockNumber)
                {
                    MicroCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                }
            };
            MicroCoinClient.BlockResponse += (ob, eb) => {
                log.DebugFormat("Received {0} Block from blockchain. BlockChain size: {1}. Block height: {2}", eb.BlockResponse.BlockTransactions.Count, BlockChain.Instance.Count, eb.BlockResponse.BlockTransactions.Last().BlockNumber);
                BlockChain.Instance.AppendAll(eb.BlockResponse.BlockTransactions);
            };
            MicroCoinClient.Start();
            MicroCoinClient.SendHello();
            return Instance;
        }

        internal void Dispose()
        {
            MicroCoinClient.Dispose();
            NodeServers.Dispose();            
            Snapshot.Dispose();
        }
    }
}
