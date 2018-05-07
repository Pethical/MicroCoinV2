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
//-----------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MicroCoin.Chain;
using MicroCoin.Cryptography;
using MicroCoin.Mining;
using MicroCoin.Net;
using MicroCoin.Protocol;
using MicroCoin.Transactions;
using MicroCoin.Util;

namespace MicroCoin
{
    public class BlocksDownloadProgressEventArgs : EventArgs
    {
        public int BlocksToDownload { get; set; }
        public int DownloadedBlocks { get; set; }
    }

    public class Node : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Node _sInstance;
        private static object nodeLock = new object();
        public event EventHandler<BlocksDownloadProgressEventArgs> BlockDownloadProgress;
        protected Node()
        {
        }

        internal ECKeyPair NodeKey { get; } = ECKeyPair.CreateNew(false);
        public static IList<ECKeyPair> Keys { get; set; }
        private static MicroCoinClient MicroCoinClient { get; set; }
        public NodeServerList NodeServers { get; set; } = new NodeServerList();
        public BlockChain BlockChain { get; set; } = BlockChain.Instance;
        public List<Account> Accounts => CheckPoints.Accounts;
        public static Node Instance
        {
            get => _sInstance ?? (_sInstance = new Node());
            set => _sInstance = value;
        }
        protected Thread ListenerThread { get; set; }
        private static readonly List<string> Transmitted = new List<string>();
        public MinerServer MinerServer { get; set; }

        protected List<MicroCoinClient> Clients { get; set; } = new List<MicroCoinClient>();
        public async Task<Node> StartNode( object parameters)
        {
            return await StartNode(MainParams.Port);
        }
        public async Task<Node> StartNode(int port = 4004, IList<ECKeyPair> keys = null)
        {
            Keys = keys;
            MicroCoinClient = new MicroCoinClient();
            //MicroCoinClient.Connect("micro-225.microbyte.cloud", 4004);
            try
            {
                P2PClient.ServerPort = MainParams.Port;
                var bl = BlockChain.Instance.BlockHeight();
                 MicroCoinClient.Connect(MainParams.FixSeeds[0], port);
                //MicroCoinClient.Connect("micro-225.microbyte.cloud", 4004);
                var response = await MicroCoinClient.SendHelloAsync();
                 while (bl <= response.Block.BlockNumber)
                 {
                    try
                    {
                        var blocks = await MicroCoinClient.RequestBlocksAsync((uint)bl, 1); //response.TransactionBlock.BlockNumber);
                        BlockChain.Instance.AppendAll(blocks.Blocks, true);
                        bl += 1;
                        BlockDownloadProgress?.Invoke(this, new BlocksDownloadProgressEventArgs
                        {
                            BlocksToDownload = (int)response.Block.BlockNumber,
                            DownloadedBlocks = bl
                        });
                    }
                    catch
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                 }
                CheckPoints.Init();
                Log.Info("BlockChain OK");
                 if (!File.Exists(CheckPoints.CheckPointFileName))
                 {
                     var cpList = CheckPoints.BuildFromBlockChain(BlockChain.Instance);
                     var cpFile = File.Create(CheckPoints.CheckPointFileName);
                     var indexFile = File.Create(CheckPoints.CheckPointIndexName);
                     CheckPoints.SaveList(cpList, cpFile, indexFile);
                     cpFile.Dispose();
                     indexFile.Dispose();
                 }

                 if (File.Exists(CheckPoints.CheckPointFileName))
                 {
                     CheckPoints.Init();
                     var blocks = CheckPoints.GetLastBlock().BlockNumber;
                     var need = BlockChain.Instance.GetLastBlock().BlockNumber;
                     for (var i = blocks; i <= need; i++) CheckPoints.AppendBlock(BlockChain.Instance.Get((int) i));
                 }
                 else
                 {
                     throw new FileNotFoundException("Checkpoint file not found", CheckPoints.CheckPointFileName);
                 }

                 GC.Collect();
                 MicroCoinClient.HelloResponse += (o, e) =>
                 {
                     Log.DebugFormat("Network CheckPointBlock height: {0}. My CheckPointBlock height: {1}",
                         e.HelloResponse.Block.BlockNumber, BlockChain.Instance.BlockHeight());
                     if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                     {
                         MicroCoinClient?.RequestBlockChain((uint) BlockChain.Instance.BlockHeight(), 100);
                     }
                 };
                 MicroCoinClient.BlockResponse += (ob, eb) =>
                 {
                     Log.DebugFormat(
                         "Received {0} CheckPointBlock from blockchain. BlockChain size: {1}. CheckPointBlock height: {2}",
                         eb.BlockResponse.Blocks.Count, BlockChain.Instance.Count,
                         eb.BlockResponse.Blocks.Last().BlockNumber);
                     BlockChain.Instance.AppendAll(eb.BlockResponse.Blocks);
                 };
                 MicroCoinClient.Dispose();
                 MicroCoinClient = null;

                Instance.NodeServers.NewNode += (sender, ev) =>
                {                    
                    var microCoinClient = ev.Node.MicroCoinClient;
                    microCoinClient.HelloResponse += (o, e) =>
                    {
                        if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                        {
                            bl = BlockChain.Instance.BlockHeight();
                            if (bl <= e.HelloResponse.Block.BlockNumber)
                            {
                                microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                            }
                        }
                    };
                    microCoinClient.BlockResponse += (ob, eb) =>
                    {
                        BlockChain.Instance.AppendAll(eb.BlockResponse.Blocks);
                        microCoinClient.SendHello();
                    };
                    microCoinClient.NewTransaction += (o, e) =>
                    {
                        string hash = e.Transaction.GetHash();
                        if (Transmitted.Contains(hash))
                        {
                            return;
                        }
                        var client = (MicroCoinClient)o;
                        var ip = ((IPEndPoint)client.TcpClient.Client.RemoteEndPoint).Address.ToString();
                        Transmitted.Add(hash);
                        if (e.Transaction.Transactions[0] is TransferTransaction t)
                        {
                            if (!t.IsValid() || !t.SignatureValid())
                            {
                                return;
                            }
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            foreach (var c in Instance.NodeServers)
                            {
                                if (c.Value.IP == ip) continue;
                                ms.Position = 0;
                                try
                                {
                                    c.Value.MicroCoinClient.SendRaw(ms);
                                }
                                catch
                                {

                                }
                            }
                        }
                    };
                    microCoinClient.NewBlock += (o, e) =>
                    {
                        BlockChain.Instance.Append(e.Block);
                    };
                };
                Instance.NodeServers.TryAddNew(MainParams.FixSeeds[0]+":"+MainParams.Port.ToString(), new NodeServer
                {
                    IP = MainParams.FixSeeds[0],
                    LastConnection = DateTime.Now,
                    Port = MainParams.Port
                });
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }

            Instance.Listen();
            Instance.MinerServer = new MinerServer();
            Instance.MinerServer.Start();
            return Instance;
        }
        protected void Listen()
        {
            ListenerThread = new Thread(() =>
            {
                try
                {
                    try
                    {
                        var tcpListener = new TcpListener(IPAddress.Any, MainParams.Port); //
                        try
                        {
                            P2PClient.ServerPort = MainParams.Port;
                            tcpListener.Start();
                            var connected = new ManualResetEvent(false);
                            while (true)
                            {
                                connected.Reset();
                                var asyncResult = tcpListener.BeginAcceptTcpClient(state =>
                                {
                                    try
                                    {
                                        var client = tcpListener.EndAcceptTcpClient(state);
                                        var mClient = new MicroCoinClient();
                                        mClient.Disconnected += (o, e) => { Clients.Remove((MicroCoinClient) o); };
                                        Clients.Add(mClient);
                                        mClient.Handle(client);
                                        connected.Set();
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                    }
                                }, null);
                                while (!connected.WaitOne(1)) ;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            tcpListener.Stop();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                finally
                {
                    Log.Warn("Listener exited");
                }
            });
            ListenerThread.Name = "Node Server";
            ListenerThread.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            foreach (var c in Clients)
            {
                if (c.IsDisposed) continue;
                Clients.Remove(c);
                c.Dispose();
            }

            if (MinerServer != null)
            {
                MinerServer.Stop = true;
                MinerServer = null;                
            }

            ListenerThread?.Abort();
            MicroCoinClient?.Dispose();
            NodeServers?.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendTransaction(Transaction transaction)
        {
            if (!transaction.IsValid()) throw new InvalidOperationException("Transaction is invalid");
            NewTransactionMessage message = new NewTransactionMessage
            {
                Operation = NetOperationType.AddOperations,
                RequestType = RequestType.AutoSend,
                TransactionCount = 1,
                Transactions = new ITransaction[] { transaction }
            };
            using (Stream s = new MemoryStream())
            {
                message.SaveToStream(s);
                s.Position = 0;
                NodeServers.BroadCastMessage(s);
            }
        }

        public bool SendCoin(Account sender, Account target, decimal amount, decimal fee, ECKeyPair key, string payload)
        {
            var transaction = new TransferTransaction
            {
                Amount = (ulong)(amount * 10000M),
                Fee = (ulong)(fee * 10000M),
                Payload = payload,
                SignerAccount = sender.AccountNumber,
                TargetAccount = target.AccountNumber,
                TransactionStyle = TransferTransaction.TransferType.Transaction,
                TransactionType = TransactionType.Transaction,
                AccountKey = key
            };
            CheckPoints.Account(transaction.SignerAccount).NumberOfOperations++;
            transaction.NumberOfOperations = CheckPoints.Account(transaction.SignerAccount).NumberOfOperations;
            transaction.Signature = transaction.GetSignature();
            sender.Balance -= transaction.Amount + transaction.Fee;
            target.Balance += transaction.Amount;
            sender.Saved = false;
            target.Saved = false;
            if (!transaction.SignatureValid()) throw new InvalidDataException();
            Instance.SendTransaction(transaction);
            return true;
        }
        public bool ChangeAccountInfo(Account account, decimal fee, string payload, ECKeyPair key)
        {
            var transaction = new ChangeAccountInfoTransaction
            {
                NewName = account.Name,
                Payload = payload,
                Fee = (ulong)(fee * 10000M),
                TargetAccount = account.AccountNumber,
                ChangeType = (byte)ChangeAccountInfoTransaction.AccountInfoChangeType.AccountName,
                SignerAccount = account.AccountNumber,
                NumberOfOperations = ++account.NumberOfOperations,
                AccountKey = account.AccountInfo.AccountKey,
                NewAccountKey = account.AccountInfo.NewPublicKey
            };
            transaction.AccountKey = key;
            transaction.Signature = transaction.GetSignature();
            account.Saved = false;
            if (transaction.NewAccountKey.CurveType != CurveType.Empty)
                transaction.ChangeType |= (byte) ChangeAccountInfoTransaction.AccountInfoChangeType.PublicKey;
            Instance.SendTransaction(transaction);
            return true;
        }
        public bool SellAccount(Account account, decimal price, decimal fee, AccountNumber seller, ECKeyPair key)
        {
            var transaction = new ListAccountTransaction
            {
                AccountPrice = (ulong) (price * 10000M),
                AccountToPay = seller,
                Fee = (ulong) (fee * 10000M) + 1,
                NumberOfOperations = ++account.NumberOfOperations,
                SignerAccount = account.AccountNumber,
                TargetAccount = account.AccountNumber,
                Payload = "",
                TransactionType = TransactionType.ListAccountForSale,
                AccountKey = key,
                LockedUntilBlock = 0
            };
            transaction.Signature = transaction.GetSignature();
            Instance.SendTransaction(transaction);
            return true;
        }
        public bool ChangeAccountKey(Account account, decimal fee, string payload, Account signer, ECKeyPair key,
            string newKey)
        {
            var transaction = new ChangeKeyTransaction
            {
                AccountKey = key,
                Fee = (ulong)(fee * 10000M),
                NewAccountKey = ECKeyPair.FromEncodedString(newKey),
                NumberOfOperations = ++signer.NumberOfOperations,
                Payload = payload,
                SignerAccount = signer.AccountNumber,
                TargetAccount = account.AccountNumber,
                TransactionType = TransactionType.ChangeKeySigned
            };
            transaction.Signature = transaction.GetSignature();
            Instance.SendTransaction(transaction);
            signer.Saved = account.Saved = false;
            return true;
        }
        public bool BuyAccount(Account account, decimal fee, string payload, Account buyer, ECKeyPair key)
        {
            var transaction = new TransferTransaction
            {
                Amount = account.AccountInfo.Price,
                Fee = (ulong)(fee * 10000M),
                Payload = payload,
                SignerAccount = buyer.AccountNumber,
                TargetAccount = account.AccountNumber,
                TransactionStyle = TransferTransaction.TransferType.BuyAccount,
                TransactionType = TransactionType.BuyAccount,
                AccountKey = key,
                AccountPrice = account.AccountInfo.Price,
                NewAccountKey = key
            };
            CheckPoints.Account(transaction.SignerAccount).NumberOfOperations++;
            transaction.NumberOfOperations = CheckPoints.Account(transaction.SignerAccount).NumberOfOperations;
            transaction.Signature = transaction.GetSignature();
            var seller =
                CheckPoints.Accounts.FirstOrDefault(p => p.AccountNumber == account.AccountInfo.AccountToPayPrice);
            if(seller==null) throw new NullReferenceException("No seller");
            seller.Balance += transaction.Amount;
            buyer.Balance -= transaction.Amount + transaction.Fee;
            transaction.SellerAccount = seller.AccountNumber;
            buyer.Saved = false;
            seller.Saved = false;
            account.Saved = false;
            Instance.SendTransaction(transaction);
            return true;
        }
    }
}