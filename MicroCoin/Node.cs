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
        private static Node s_instance;
        public ECKeyPair AccountKey { get; } = ECKeyPair.CreateNew(false);
        private MicroCoinClient MicroCoinClient { get; set; }
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
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.Connect("127.0.0.1", 4004);
            HelloResponse response = await microCoinClient.SendHelloAsync();
            uint start = (response.TransactionBlock.BlockNumber / 100) * 100;
            var blocks = await microCoinClient.RequestBlocksAsync(start, response.TransactionBlock.BlockNumber);
            BlockChain.Instance.AddRange(blocks.BlockTransactions);
            using (FileStream fs = File.Create(BlockChain.Instance.BlockChainFileName))
            {
                BlockChain.Instance.SaveToStorage(fs);
            }
            await microCoinClient.DownloadSnaphostAsync(response.TransactionBlock.BlockNumber);
            FileStream file = File.Create($"snaphot");
            Instance.Snapshot.SaveToStream(file);            
            file.Dispose();
            Instance.Snapshot.LoadFromFile("snaphot");
            GC.Collect();
            microCoinClient.Start();
            microCoinClient.SendHello();
            return Instance;
        }

    }
}
