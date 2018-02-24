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


using MicroCoin.Chain;
using MicroCoin.Protocol;
using System;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Net;
using MicroCoin.Util;

namespace MicroCoin.Net
{

    public enum RequestType : ushort { None = 0, Request, Response, AutoSend, Unknown };

    public enum NetOperationType : ushort
    {
        Hello = 1,
        Error = 2,
        Message = 3,
        Transactions = 0x05,
        Blocks = 0x10,
        NewBlock = 0x11,
        AddOperations = 0x20,
        CheckPoint = 0x21
    }

    public class HelloRequestEventArgs : EventArgs
    {
        public HelloRequest HelloRequest { get; set; }
        public HelloRequestEventArgs(HelloRequest helloRequest)
        {
            HelloRequest = helloRequest;
        }
    }
    public class HelloResponseEventArgs : EventArgs
    {
        public HelloResponse HelloResponse { get; }
        public HelloResponseEventArgs(HelloResponse helloResponse)
        {
            HelloResponse = helloResponse;
        }
    }
    public class BlockResponseEventArgs : EventArgs
    {
        public BlockResponse BlockResponse { get; }
        public BlockResponseEventArgs(BlockResponse blockResponse)
        {
            BlockResponse = blockResponse;
        }
    }
    /*public class TransactionBlockResponseEventArgs
    {
        public TransactionBlockResponse transactionBlockResponse { get; }

        public TransactionBlockResponseEventArgs(TransactionBlockResponse transactionBlockResponse)
        {
            this.transactionBlockResponse = transactionBlockResponse;
        }
    }
    */
    public class NewTransactionEventArgs
    {
        public NewTransactionMessage Transaction { get; }

        public NewTransactionEventArgs(NewTransactionMessage transaction)
        {
            Transaction = transaction;
        }
    }

    public class MicroCoinClient : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<HelloRequestEventArgs> HelloRequest;
        public event EventHandler<HelloResponseEventArgs> HelloResponse;
        public event EventHandler<BlockResponseEventArgs> BlockResponse;
        //public event EventHandler<BlockResponseEventArgs> TransactionBlockResponse;
        public event EventHandler<NewTransactionEventArgs> NewTransaction;
        public static ushort ServerPort { get; set; }
        public TcpClient TcpClient { get; set; }
        private static object netLock = new object();

        public bool Connected { get; internal set; } = false;
        public bool IsDisposed { get; internal set; } = false;

        public void OnHelloResponse(HelloResponse helloResponse)
        {
            HelloResponse?.Invoke(this, new HelloResponseEventArgs(helloResponse));
        }
        public void OnNewTransaction(NewTransactionMessage newTransaction)
        {
            NewTransaction?.Invoke(this, new NewTransactionEventArgs(newTransaction));
        }
        public void OnGetBlockResponse(BlockResponse blockResponse)
        {
            BlockResponse?.Invoke(this, new BlockResponseEventArgs(blockResponse));
        }
        public void OnHelloRequest(HelloRequest helloRequest)
        {
            HelloRequest?.Invoke(this, new HelloRequestEventArgs(helloRequest));
        }
        /*private void OnGetTransactionBlockResponse(BlockResponse blockResponse)
        {
            TransactionBlockResponse?.Invoke(this, new BlockResponseEventArgs(blockResponse));
        }*/

        public MicroCoinClient()
        {

        }

        public void SendHello()
        {
            HelloRequest request = new HelloRequest
            {
                AccountKey = Node.Instance.AccountKey,
                AvailableProtocol = 6,
                Error = 0,
                NodeServers = Node.Instance.NodeServers,
                Operation = NetOperationType.Hello
            };
            SHA256Managed sha = new SHA256Managed();
            Hash h = sha.ComputeHash(Encoding.ASCII.GetBytes("(c) Peter Nemeth - Okes rendben okes"));
            request.Block = new Block
            {
                AccountKey = null,
                AvailableProtocol = 0,
                BlockNumber = 0,
                CompactTarget = 0,
                Fee = 0,
                Nonce = 0,
                TransactionHash = new byte[0],
                Payload = new byte[0],
                ProofOfWork = new byte[0],
                ProtocolVersion = 0,
                Reward = 0,
                CheckPointHash = h,
                BlockSignature = 3,
                Timestamp = 0
            };
            request.ProtocolVersion = 6;
            request.RequestId = 1;
            request.RequestType = RequestType.Request;
            request.ServerPort = ServerPort;
            request.Timestamp = DateTime.UtcNow;
            request.Version = "2.0.0wN";
            request.WorkSum = 0;
            MemoryStream ms = new MemoryStream();
            request.SaveToStream(ms);
            ms.Flush();
            ms.Position = 0;
            NetworkStream ns = TcpClient.GetStream();
            ms.CopyTo(ns);
            ms.Dispose();
            ns.Flush();
        }
        protected int WaitForData(int timeoutMS)
        {
            while (TcpClient.Available == 0)
            {
                Thread.Sleep(1);
            }
            return TcpClient.Available;
        }
        protected async Task<MessageHeader> ReadResponse(MemoryStream responseStream)
        {
            responseStream.Position = 0;
            NetworkStream ns = TcpClient.GetStream();
            return await Task.Run(() =>
            {
                while (TcpClient.Available > 0)
                {
                    byte[] buffer = new byte[TcpClient.Available];
                    ns.Read(buffer, 0, buffer.Length);
                    responseStream.Write(buffer, 0, buffer.Length);
                }
                responseStream.Position = 0;
                MessageHeader rp = new MessageHeader(responseStream);
                long pos = responseStream.Position;
                responseStream.Position = responseStream.Length;
                int wt = 0;
                log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {responseStream.Length}. Memory {GC.GetTotalMemory(true)}");
                while (rp.DataLength > responseStream.Length - RequestHeader.Size)
                {
                    do
                    {
                        Thread.Sleep(1);
                        wt++;
                        if (wt > 4000) break;
                    } while (TcpClient.Available == 0);

                    if (wt > 4000) break;

                    while (TcpClient.Available > 0)
                    {
                        log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {responseStream.Length}. Available: {TcpClient.Available}  Memory {GC.GetTotalMemory(true)}");
                        byte[] buffer = new byte[TcpClient.Available];
                        ns.Read(buffer, 0, buffer.Length);
                        responseStream.Write(buffer, 0, buffer.Length);
                        log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {responseStream.Length}. Available: {TcpClient.Available}  Memory {GC.GetTotalMemory(true)}");
                    }
                    wt = 0;
                }

                if (wt > 1000)
                {
                    log.Error($"Timeout. Received {responseStream.Length}, expected {rp.DataLength + RequestHeader.Size}");
                    throw new TimeoutException();
                }
                responseStream.Position = pos;
                return rp;
            });
        }
        public async Task<HelloResponse> SendHelloAsync()
        {
            HelloRequest request = new HelloRequest
            {
                AccountKey = Node.Instance.AccountKey,
                AvailableProtocol = 6,
                Error = 0,
                NodeServers = Node.Instance.NodeServers,
                Operation = NetOperationType.Hello
            };
            SHA256Managed sha = new SHA256Managed();
            byte[] h = sha.ComputeHash(Encoding.ASCII.GetBytes("(c) Peter Nemeth - Okes rendben okes"));
            request.Block = new Block
            {
                AccountKey = null,
                AvailableProtocol = 0,
                BlockNumber = 0,
                CompactTarget = 0,
                Fee = 0,
                Nonce = 0,
                TransactionHash = new byte[0],
                Payload = new byte[0],
                ProofOfWork = new byte[0],
                ProtocolVersion = 0,
                Reward = 0,
                CheckPointHash = h,
                BlockSignature = 3,
                Timestamp = 0
            };
            request.ProtocolVersion = 6;
            request.RequestId = 1;
            request.RequestType = RequestType.Request;
            request.ServerPort = ServerPort;
            request.Timestamp = DateTime.UtcNow;
            request.Version = "2.0.0wN";
            request.WorkSum = 0;
            MemoryStream ms = new MemoryStream();
            request.SaveToStream(ms);
            ms.Flush();
            ms.Position = 0;
            NetworkStream ns = TcpClient.GetStream();
            ms.CopyTo(ns);
            ms.Dispose();
            ns.Flush();
            WaitForData(10000);
            using (var responseStream = new MemoryStream())
            {
                var rp = await ReadResponse(responseStream);
                if (rp.Operation == NetOperationType.Hello)
                {
                    HelloResponse response = new HelloResponse(responseStream, rp);
                    return response;
                }
                throw new InvalidDataException($"Not hello {rp.Operation}");
            }
        }
        public async Task<bool> DownloadCheckPointAsync(uint blockCount)
        {
            {
                uint endBlock = (blockCount / 100) * 100;
                var blockResponse = await RequestTransactionBlocksAsync(endBlock, 1, null);
                var ns = TcpClient.GetStream();
                using (var ms = new MemoryStream())
                {
                    //log.Debug(transactionBlockResponse.List.First().BlockNumber);
                    for (uint i = 0; i <= blockResponse.Blocks.Last().BlockNumber / 10000; i++)
                    {
			log.Info(i);
                        CheckPointRequest checkPointRequest = new CheckPointRequest();
                        checkPointRequest.Operation = NetOperationType.CheckPoint;
                        checkPointRequest.RequestType = RequestType.Request;
                        checkPointRequest.StartBlock = (uint)(i * 10000);
                        checkPointRequest.EndBlock = (uint)(((i + 1) * 10000) - 1);
                        if (checkPointRequest.EndBlock > blockResponse.Blocks.Last().BlockNumber - 1)
                        {
                            checkPointRequest.EndBlock = blockResponse.Blocks.Last().BlockNumber - 1;
                        }
                        checkPointRequest.checkPointBlockCount = endBlock;
                        checkPointRequest.RequestId = 12345;
                        checkPointRequest.CheckPointHash = blockResponse.Blocks.Last().CheckPointHash;
                        using (MemoryStream requestStream = new MemoryStream())
                        {
                            checkPointRequest.SaveToStream(requestStream);
                            requestStream.Position = 0;
                            requestStream.CopyTo(ns);
                            ns.Flush();
                        }
                        WaitForData(10000);
			log.Info("Data received");
                        using (var responseStream = new MemoryStream())
                        {
                            var rp = await ReadResponse(responseStream);
			    log.Info("All Data received");
                            responseStream.Position = 0;
                            CheckPointResponse response = new CheckPointResponse(responseStream);
                            response = null;
                        }
                    }
                    return true;
                }
            }
        }
        public void DownloadCheckPoint(uint blockCount)
        {
            const uint REQUEST_ID = 123456;
            void handler(object o, BlockResponseEventArgs e)
            {
                if (e.BlockResponse.RequestId == REQUEST_ID)
                {
                    log.Debug(e.BlockResponse.Blocks.First().BlockNumber);
                    for (uint i = 0; i <= e.BlockResponse.Blocks.Last().BlockNumber / 10000; i++)
                    {
                        CheckPointRequest checkPointRequest = new CheckPointRequest();
                        checkPointRequest.Operation = NetOperationType.CheckPoint;
                        checkPointRequest.RequestType = RequestType.Request;
                        checkPointRequest.StartBlock = (uint)(i * 10000);
                        checkPointRequest.EndBlock = (uint)(((i + 1) * 10000) - 1);
                        if (checkPointRequest.EndBlock > e.BlockResponse.Blocks.Last().BlockNumber - 1)
                        {
                            checkPointRequest.EndBlock = e.BlockResponse.Blocks.Last().BlockNumber - 1;
                        }
                        checkPointRequest.checkPointBlockCount = 14700;
                        checkPointRequest.RequestId = 12345;
                        checkPointRequest.CheckPointHash = e.BlockResponse.Blocks.Last().CheckPointHash;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            checkPointRequest.SaveToStream(ms);
                            NetworkStream ns = TcpClient.GetStream();
                            ms.Position = 0;
                            ms.CopyTo(ns);
                            ns.Flush();
                        }
                    }
                }
            }
            BlockResponse += handler;
            uint endBlock = (blockCount / 100) * 100;
            RequestBlockChain(blockCount, 1, REQUEST_ID, NetOperationType.Transactions);
        }
        public async Task<BlockResponse> RequestBlocksAsync(uint startBlock, uint quantity, uint? requestId = null)
        {
            {
                BlockRequest br = new BlockRequest
                {
                    StartBlock = startBlock,
                    BlockNumber = quantity,
                    Operation = NetOperationType.Blocks
                };
                if (requestId != null)
                {
                    br.RequestId = requestId.Value;
                }
                NetworkStream ns = TcpClient.GetStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    br.SaveToStream(ms);
                    ms.Position = 0;
                    ms.CopyTo(ns);
                }
                ns.Flush();
                WaitForData(10000);
                using (var rs = new MemoryStream())
                {
                    var rp = await ReadResponse(rs);
                    switch (rp.Operation)
                    {
                        case NetOperationType.Blocks:
                            return new BlockResponse(rs, rp);
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
        }
        public async Task<BlockResponse> RequestTransactionBlocksAsync(uint startBlock, uint quantity, uint? requestId = null)
        {
            {
                BlockRequest br = new BlockRequest
                {
                    StartBlock = startBlock,
                    BlockNumber = quantity,
                    Operation = NetOperationType.Transactions
                };
                if (requestId != null)
                {
                    br.RequestId = requestId.Value;
                }
                NetworkStream ns = TcpClient.GetStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    br.SaveToStream(ms);
                    ms.Position = 0;
                    ms.CopyTo(ns);
                }
                ns.Flush();
                WaitForData(10000);
                using (var rs = new MemoryStream())
                {
                    var rp = await ReadResponse(rs);
                    switch (rp.Operation)
                    {
                        case NetOperationType.Transactions:
                            return new BlockResponse(rs, rp);
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
        }
        public void RequestBlockChain(uint startBlock, uint quantity, uint? requestId = null, NetOperationType netOperationType = NetOperationType.Blocks)
        {
            BlockRequest br = new BlockRequest
            {
                StartBlock = startBlock,
                BlockNumber = quantity,
                Operation = netOperationType
            };
            if (requestId != null)
            {
                br.RequestId = requestId.Value;
            }
            NetworkStream ns = TcpClient.GetStream();
            using (MemoryStream ms = new MemoryStream())
            {
                br.SaveToStream(ms);
                ms.Position = 0;
                ms.CopyTo(ns);
            }
            ns.Flush();
        }
        public bool Connect(string hostname, int port)
        {
            try
            {
                TcpClient = new TcpClient();                
                var result = TcpClient.BeginConnect(hostname, port, null, null);
                Connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                TcpClient.EndConnect(result);                
                if (!Connected)
                {
                    throw new Exception("");
                }
                log.Info($"Connected to {hostname}:{port}");
            }
            catch (Exception e)
            {
                if (TcpClient != null)
                {
                    TcpClient.Dispose();
                    TcpClient = null;
                }
                Connected = false;
                log.Debug($"Can't connect to {hostname}:{port}. {e.Message}");
                TcpClient = null;
                return false;
            }
            return Connected;
        }

        public void Handle(TcpClient client)
        {
            TcpClient = client;
            Connected = true;
            Start();
        }

        private Thread listenerThread;

        private bool stop = false;

        public void Start()
        {
            listenerThread = new Thread(() =>
            {
                while (true)
                {
                    while (TcpClient.Available == 0)
                    {
                        if (stop)
                        {
                            log.Info("Stopping Thread");
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    var ms = new MemoryStream();
                    NetworkStream ns = TcpClient.GetStream();
                    while (TcpClient.Available > 0)
                    {
                        byte[] buffer = new byte[TcpClient.Available];
                        ns.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, buffer.Length);
                    }
                    ms.Position = 0;
                    MessageHeader rp = new MessageHeader(ms);
                    long pos = ms.Position;
                    ms.Position = ms.Length;
                    int wt = 0;
                    log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {ms.Length}");
                    while (rp.DataLength > ms.Length - RequestHeader.Size)
                    {
                        do
                        {
                            Thread.Sleep(1);
                            wt++;
                            if (wt > 4000) break;
                        } while (TcpClient.Available == 0);
                        if (wt > 4000) break;
                        while (TcpClient.Available > 0)
                        {
                            log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {ms.Length}. Available: {TcpClient.Available}");
                            byte[] buffer = new byte[TcpClient.Available];
                            ns.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, buffer.Length);
                            log.Debug($"Expected: {rp.DataLength + RequestHeader.Size} received: {ms.Length}. Available: {TcpClient.Available}");
                        }
                        wt = 0;
                    }

                    if (wt > 1000)
                    {
                        log.Error($"Timeout. Received {ms.Length}, expected {rp.DataLength + RequestHeader.Size}");
                        continue;
                    }

                    ms.Position = pos;
                    log.Info($"{TcpClient.Client.RemoteEndPoint.ToString()} => {rp.Operation} {rp.RequestType} from {TcpClient.Client.RemoteEndPoint}. Data length: {rp.DataLength}");
                    if (rp.RequestType == RequestType.Response)
                    {
                        switch (rp.Operation)
                        {
                            case NetOperationType.Hello:
                                try
                                {
                                    HelloResponse response = new HelloResponse(ms, rp);
                                    log.Info($"{response.WorkSum} {CheckPoints.WorkSum}");
                                    Node.Instance.NodeServers.UpdateNodeServers(response.NodeServers);
                                    OnHelloResponse(response);
                                }
                                catch (Exception e)
                                {
                                    log.Error(e.Message, e);
                                }

                                break;
                            case NetOperationType.Transactions:
                                BlockResponse transactionBlockResponse = new BlockResponse(ms, rp);
                                OnGetBlockResponse(transactionBlockResponse);
                                break;
                            case NetOperationType.Blocks:
                                BlockResponse blockResponse = new BlockResponse(ms, rp);
                                OnGetBlockResponse(blockResponse);
                                break;
                            case NetOperationType.CheckPoint:
                                ms.Position = 0;
                                CheckPointResponse checkPointResponse = new CheckPointResponse(ms);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (rp.RequestType == RequestType.Request)
                    {
                        switch (rp.Operation)
                        {
                            case NetOperationType.Blocks:
                                BlockRequest blockRequest = new BlockRequest(ms, rp);
                                var blockResponse = new BlockResponse
                                {
                                    RequestId = blockRequest.RequestId,
                                    Blocks = BlockChain.Instance.GetBlocks(blockRequest.StartBlock, blockRequest.EndBlock)
                                };
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    blockResponse.SaveToStream(memoryStream);
                                    memoryStream.Position = 0;
                                    ms.CopyTo(ns);
                                }
                                break;
                            case NetOperationType.Transactions:
                                {
                                    BlockRequest transactionBlockRequest = new BlockRequest(ms, rp);
                                    BlockResponse transactionBlockResponse = new BlockResponse()
                                    {
                                        RequestId = transactionBlockRequest.RequestId
                                    };
                                    transactionBlockResponse.Blocks = BlockChain.Instance.GetBlocks(transactionBlockRequest.StartBlock, transactionBlockRequest.EndBlock).ToList<Block>();
                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        transactionBlockResponse.SaveToStream(memoryStream);
                                        memoryStream.Position = 0;
                                        memoryStream.CopyTo(ns);
                                    }
                                    break;
                                }
                            case NetOperationType.CheckPoint:
                                CheckPointRequest checkPointRequest = new CheckPointRequest(ms, rp);
                                CheckPointResponse checkPointResponse = new CheckPointResponse();                                
                                checkPointResponse.RequestId = checkPointRequest.RequestId;
                                using (MemoryStream m = new MemoryStream())
                                {
                                    /*var CheckPoint = Node.Instance.CheckPoint.SaveChunk(checkPointRequest.StartBlock, checkPointRequest.EndBlock);
                                    checkPointResponse.CheckPoint = CheckPoint;
                                    checkPointResponse.SaveToStream(m);
                                    m.Position = 0;
                                    m.CopyTo(ns);*/
                                }
                                break;
                            case NetOperationType.Hello:
                                HelloRequest request = new HelloRequest(ms, rp);
                                Node.Instance.NodeServers.UpdateNodeServers(request.NodeServers);
                                HelloResponse response = new HelloResponse(request);
                                response.Timestamp = DateTime.UtcNow;
                                response.Error = 0;
                                response.ServerPort = (ushort)((IPEndPoint)TcpClient.Client.LocalEndPoint).Port;
                                response.Block = BlockChain.Instance.GetLastBlock();
                                response.WorkSum = CheckPoints.WorkSum;
                                response.AccountKey = Node.Instance.AccountKey;
                                response.RequestType = RequestType.Response;
                                response.Operation = NetOperationType.Hello;
                                using (MemoryStream vm = new MemoryStream())
                                {
                                    response.SaveToStream(vm);
                                    vm.Position = 0;
                                    vm.CopyTo(ns);
                                    ns.Flush();
                                }
                                break;
                        }
                    }
                    else if (rp.RequestType == RequestType.AutoSend)
                    {
                        switch (rp.Operation)
                        {
                            case NetOperationType.Error:
                                byte[] buffer = new byte[rp.DataLength];
                                ms.Read(buffer, 0, rp.DataLength);
                                log.Error(TcpClient.Client.RemoteEndPoint.ToString() + " => " + Encoding.ASCII.GetString(buffer));
                                break;
                            case NetOperationType.NewBlock:
                                log.Info($"Received new block from client");
                                NewBlockRequest response = new NewBlockRequest(ms, rp);
                                log.Debug($"CheckPointBlock number {response.Block.BlockNumber}");
                                BlockChain.Instance.Append(response.Block);
                                break;
                            case NetOperationType.AddOperations:
                                log.Info($"Received new operation");
                                var newTransaction = new NewTransactionMessage(ms, rp);
                                OnNewTransaction(newTransaction);
                                break;
                        }

                    }
                    ms.Dispose();
                    ms = null;
                }
            });
            listenerThread.Name = TcpClient.Client.RemoteEndPoint.ToString();
            listenerThread.Start();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
        private void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                stop = true;                
                while(listenerThread!=null && listenerThread.IsAlive)
                {                    
                    Thread.Sleep(1);
                }
                if(TcpClient!=null)
                    TcpClient.Dispose();                
            }
            IsDisposed = true;
        }
    }

}