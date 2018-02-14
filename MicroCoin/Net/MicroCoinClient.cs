using MicroCoin.BlockChain;
using MicroCoin.Cryptography;
using MicroCoin.Protocol;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public class MicroCoinClient
    {
        public event EventHandler<HelloRequest> HelloRequest;
        public event EventHandler<HelloResponse> HelloResponse;
        public event EventHandler<BlockResponse> BlockResponse;

        private object threadLock = new object();

        protected TcpClient tcpClient;

        public void SendHello()
        {
            HelloRequest request = new HelloRequest();
            request.AccountKey = ECKeyPair.createNew(false);
            request.AvailableProtocol = 6;
            request.Error = 0;
            request.NodeServers = new NodeServerList();
            request.Operation = NetOperationType.Hello;
            SHA256Managed sha = new SHA256Managed();
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            byte[] h = sha.ComputeHash(Encoding.ASCII.GetBytes("(c) Peter Nemeth - Okes rendben okes"));
            request.OperationBlock = new TransactionBlock
            {
                AccountKey = null,
                AvailableProtocol = 0,
                BlockNumber = 0,
                CompactTarget = 0,
                Fee = 0,
                Nonce = 0,
                OperationHash = null,
                Payload = null,
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
            request.Timestamp = unixTimestamp;
            request.Version = "2.0.0wN";
            request.WorkSum = 0;
            MemoryStream ms = new MemoryStream();
            request.SaveToStream(ms);
            ms.Flush();
            ms.Position = 0;
            NetworkStream ns = tcpClient.GetStream();
            ms.CopyTo(ns);
            ms.Close();
            ms.Dispose();
            ns.Flush();
        }

        public void RequestBlockChain(uint startBlock, uint blockNumber)
        {
            BlockRequest br = new BlockRequest();
            br.StartBlock = startBlock;
            br.BlockNumber = blockNumber;
            NetworkStream ns = tcpClient.GetStream();
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
            HelloResponse?.Invoke(this, helloResponse);
        }
        public void OnGetBlockResponse(BlockResponse blockResponse)
        {
            BlockResponse?.Invoke(this, blockResponse);
        }


        public void OnHelloRequest(HelloRequest helloRequest)
        {
            HelloRequest?.Invoke(this, helloRequest);
        }

        public void Start()
        {
            tcpClient = new TcpClient("127.0.0.1", 4004);
            tcpClient.ReceiveBufferSize = 1024 * 1014 * 1024;
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    while (tcpClient.Available == 0) Thread.Sleep(1);
                    var ms = new MemoryStream();
                    NetworkStream ns = tcpClient.GetStream();
                    while (tcpClient.Available > 0)
                    {
                        byte[] buffer = new byte[tcpClient.Available];
                        ns.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, buffer.Length);
                    }
                    ms.Position = 0;
                    Response rp = new Response(ms);
                    long pos = ms.Position;
                    int wt = 0;
                    while (rp.DataLength > ms.Length - Response.size)
                    {
                        while (tcpClient.Available == 0)
                        {
                            Thread.Sleep(1);
                            wt++;
                            if (wt > 1000) break;
                        }
                        if (wt > 1000) break;
                        while (tcpClient.Available > 0)
                        {
                            byte[] buffer = new byte[tcpClient.Available];
                            ns.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, buffer.Length);
                        }
                    }
                    
                    if (wt > 1000)
                    {
                        Console.WriteLine("Timeout");
                        continue;
                    }
                    ms.Position = pos;
                    switch (rp.Operation)
                    {
                        case NetOperationType.Hello:
                            try
                            {
                                HelloResponse response = new HelloResponse(ms, rp);
                                OnHelloResponse(response);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            break;
                        case NetOperationType.GetBlocks:
                            BlockResponse blockResponse = new BlockResponse(ms, rp);
                            OnGetBlockResponse(blockResponse);
                            break;
                        default:
                            break;
                    }
                    ms.Dispose();
                }
            });
            t.Start();            
        }
    }
}
