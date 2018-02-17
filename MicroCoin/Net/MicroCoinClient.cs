// This file is part of MicroCoin.
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
using MicroCoin.Cryptography;
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

namespace MicroCoin.Net
{
    
    public enum RequestType : ushort { None = 0, Request, Response, AutoSend, Unknown };
    public enum NetOperationType : ushort
    {
        Hello = 1,
        Error = 2,
        Message = 3,
        GetOperationBlocks = 0x05,
        GetBlocks = 0x10,
        NewBlock = 0x11,
        AddOperations = 0x20,
        GetSafeBox = 0x21
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


    public class MicroCoinClient : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<HelloRequestEventArgs> HelloRequest;
        public event EventHandler<HelloResponseEventArgs> HelloResponse;
        public event EventHandler<BlockResponseEventArgs> BlockResponse;

        protected TcpClient TcpClient;

        public bool Connected { get; internal set; } = false;

        public void SendHello()
        {
            HelloRequest request = new HelloRequest
            {
                AccountKey = ECKeyPair.CreateNew(false),
                AvailableProtocol = 6,
                Error = 0,
                NodeServers = Node.Instance.NodeServers,
                Operation = NetOperationType.Hello
            };
            SHA256Managed sha = new SHA256Managed();
            //Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            byte[] h = sha.ComputeHash(Encoding.ASCII.GetBytes("(c) Peter Nemeth - Okes rendben okes"));
            request.TransactionBlock = new TransactionBlock
            {
                AccountKey = null,
                AvailableProtocol = 0,
                BlockNumber = 0,
                CompactTarget = 0,
                Fee = 0,
                Nonce = 0,
                OperationHash = null,
                Payload = new byte[0],
                ProofOfWork = null,
                ProtocolVersion = 0,
                Reward = 0,
                SafeBoxHash = h,
                TransactionBlockSignature = 3,
                Timestamp = 0
            };

            request.ProtocolVersion = 6;
            request.RequestId = 1;
            request.RequestType = RequestType.Request;
            request.ServerPort = 4004;
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
        public void RequestBlockChain(uint startBlock, uint quantity)
        {
            BlockRequest br = new BlockRequest
            {
                StartBlock = startBlock,
                BlockNumber = quantity
            };
            NetworkStream ns = TcpClient.GetStream();
            using (MemoryStream ms = new MemoryStream())
            {
                br.SaveToStream(ms);
                ms.Position = 0;
                ms.CopyTo(ns);
            }
            ns.Flush();
        }
        public void OnHelloResponse(HelloResponse helloResponse)
        {            
            HelloResponse?.Invoke(this, new HelloResponseEventArgs( helloResponse ));
        }
        public void OnGetBlockResponse(BlockResponse blockResponse)
        {
            BlockResponse?.Invoke(this, new BlockResponseEventArgs( blockResponse ));
        }
        public void OnHelloRequest(HelloRequest helloRequest)
        {
            HelloRequest?.Invoke(this, new HelloRequestEventArgs( helloRequest));
        }
        protected void UpdateNodeServers(HelloResponse response)
        {
            foreach (var n in response.NodeServers)
            {
                if (!Node.Instance.NodeServers.ContainsKey(n.Value.ToString()))
                {
                    Node.Instance.NodeServers.TryAddNew(n.Value.ToString(), n.Value);
                    log.Debug($"New node connection: {n.Value}");
                }
            }
            if (Node.Instance.NodeServers.Count > 100)
            {
                var list = Node.Instance.NodeServers.OrderByDescending(p => p.Value.LastConnection).Take(100 - Node.Instance.NodeServers.Count);
                foreach (var l in list)
                {
                    Node.Instance.NodeServers.TryRemove(l.Key, out NodeServer n);
                    log.Debug($"Removed connection: {n}");
                }
            }
        }

        protected void UpdateNodeServers(HelloRequest request)
        {
            foreach (var n in request.NodeServers)
            {
                if (!Node.Instance.NodeServers.ContainsKey(n.Value.ToString()))
                {
                    Node.Instance.NodeServers.TryAddNew(n.Value.ToString(), n.Value);
                    log.Debug($"New node connection: {n.Value}");
                }
            }
            if (Node.Instance.NodeServers.Count > 100)
            {
                var list = Node.Instance.NodeServers.OrderByDescending(p => p.Value.LastConnection).Take(100 - Node.Instance.NodeServers.Count);
                foreach (var l in list)
                {
                    Node.Instance.NodeServers.TryRemove(l.Key, out NodeServer n);
                    log.Debug($"Removed connection: {n}");
                }
            }
        }

        public void Start(string hostname, int port)
        {
            try
            {
                
                TcpClient = new TcpClient();
                var result = TcpClient.BeginConnect(hostname, port, null, null);
                Connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                if(!Connected)
                {
                    throw new Exception("");
                }
            }
            catch (Exception e)
            {
                if (TcpClient != null)
                {
                    TcpClient.Dispose();
                    TcpClient = null;
                }
                Connected = false;
                log.Warn($"Can't connect to {hostname}:{port}");
                TcpClient = null;
                return;
            }
            System.Timers.Timer timer = new System.Timers.Timer(30000+new Random().Next(10000));
            timer.Elapsed += (sender, e) => SendHello();
            timer.Start();
            Thread t = new Thread(() =>
            {
                log.Info($"Connected to {hostname}:{port}");
                while (true)
                {
                    while (TcpClient.Available == 0) Thread.Sleep(1);
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
                            log.Info($"Expected: {rp.DataLength + RequestHeader.Size} received: {ms.Length}. Available: {TcpClient.Available}");
                            byte[] buffer = new byte[TcpClient.Available];
                            ns.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, buffer.Length);
                            log.Info($"Expected: {rp.DataLength + RequestHeader.Size} received: {ms.Length}. Available: {TcpClient.Available}");
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
                                    UpdateNodeServers(response);
                                    OnHelloResponse(response);
                                }
                                catch (Exception e)
                                {
                                    log.Error(e.Message, e);
                                }

                                break;
                            case NetOperationType.GetBlocks:
                                BlockResponse blockResponse = new BlockResponse(ms, rp);
                                OnGetBlockResponse(blockResponse);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (rp.RequestType == RequestType.Request)
                    {
                        switch (rp.Operation)
                        {
                            case NetOperationType.Hello:
                                HelloRequest request = new HelloRequest(ms, rp);
                                UpdateNodeServers(request);
                                HelloResponse response = new HelloResponse(request);
                                response.Timestamp = DateTime.UtcNow;
                                response.Error = 0;
                                response.ServerPort = (ushort) ((IPEndPoint)TcpClient.Client.LocalEndPoint).Port;
                                response.TransactionBlock = BlockChain.Instance.GetLastTransactionBlock();
                                response.WorkSum = request.WorkSum; // TODO: Csalás
                                response.AccountKey = ECKeyPair.CreateNew(false);
                                response.RequestType = RequestType.Response;
                                response.Operation = NetOperationType.Hello;
                                MemoryStream vm = new MemoryStream();
                                response.SaveToStream(vm);
                                vm.Position = 0;
                                vm.CopyTo(ns);
                                ns.Flush();
                                ms.Close();
                                ms.Dispose();
                                //TcpClient = new TcpClient("127.0.0.1", 4004) { ReceiveBufferSize = 1024 * 1014 * 1024 };
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
                                log.Error( TcpClient.Client.RemoteEndPoint.ToString()+" => "+ Encoding.ASCII.GetString(buffer));
                                break;
                            case NetOperationType.NewBlock:
                                log.Debug($"Received new block from client");
                                NewBlockRequest response = new NewBlockRequest(ms, rp);
                                log.Debug($"Block number {response.TransactionBlock.BlockNumber}");
                                BlockChain.Instance.Append(response.TransactionBlock);
                                break;
                        }

                    }
                    ms.Dispose();
                }
            });
            t.Start();            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                TcpClient.Dispose();
            }
        }
    }
}
