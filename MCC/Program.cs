using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X9;
using System.Collections.Generic;

namespace MCC
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


    class Program
    {


        static MemoryStream CreateStream(MemoryStream data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(0x0A043580); //Magic
            bw.Write((ushort)1);  // op
            bw.Write((ushort)1);  // op
            bw.Write((ushort)0);  // error
            bw.Write((uint)1);    // request_id

            bw.Write((ushort)6);  // Protv
            bw.Write((ushort)6);    // prota
            // HEADER END
            bw.Write((int) data.Length);

            bw.Write(data.ToArray());
            bw.Flush();
            return ms;
        }

        static void Main(string[] args)
        {
            List<OperationBlock> list = new List<OperationBlock>();
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.BlockResponse += (o, e) => {
                foreach(var l in e.OperationBlocks)
                {
                    list.Add(l);
                }
                // list.AddRange(e.OperationBlocks);
            };
            microCoinClient.Start();
            microCoinClient.SendHello();
            //microCoinClient.RequestBlockChain(1262, 100);
            Thread.Sleep(100);
            //microCoinClient.RequestBlockChain(12989, 1);
            for (uint i = 0; i < 14000; i += 100)
            {
                microCoinClient.RequestBlockChain(i, 100);
                Thread.Sleep(50);
            }
            Console.ReadLine();
            HelloRequest request = new HelloRequest();
            request.AccountKey = ECKeyPair.createNew(false);
            request.AvailableProtocol = 6;
            request.Error = 0;
            request.NodeServers = new NodeServerList();
            request.Operation = NetOperationType.Hello;
            SHA256Managed sha = new SHA256Managed();
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            byte[] h = sha.ComputeHash(Encoding.ASCII.GetBytes("(c) Peter Nemeth - Okes rendben okes"));
            request.OperationBlock = new OperationBlock
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
                Soob = 3,
                Timestamp = 0
            };

            request.ProtocolVersion = 6;
            request.RequestId = 1;
            request.RequestType = RequestType.Request;
            request.ServerPort = 4004;
            request.Timestamp = unixTimestamp;
            request.Version = "2.0.0wN";
            request.WorkSum = 0;
            FileStream block = File.Create("FSOP");
            request.SaveToStream(block);
            block.Close();
            MemoryStream ms = new MemoryStream();
            request.SaveToStream(ms);
            ms.Flush();
            ms.Position = 0;
            TcpClient client = new TcpClient("127.0.0.1", 4004);
            NetworkStream ns = client.GetStream();
            ms.CopyTo(ns);
            ns.Flush();
            while (client.Available == 0) Thread.Sleep(1);
            if (client.Available > 0)
            {
                NetworkStream n1s = client.GetStream();
                HelloResponse response = new HelloResponse(n1s);
                //n1s.Read(new byte[response.DataLength], 0, response.DataLength);
            }

            BlockRequest br = new BlockRequest();
            br.StartBlock = Convert.ToUInt32(args[0]);
            br.BlockNumber = 1;
            ms.Dispose();
            ms = new MemoryStream();
            br.SaveToStream(ms);
            ms.Position = 0;
            ms.CopyTo(ns);
            ns.Flush();
            int ss = 0;
            while (true)
            {
                if (client.Available>0)
                {
                    NetworkStream n1s = client.GetStream();
                    //Response response = new Response(n1s);
                    //n1s.Read(new byte[response.DataLength], 0, response.DataLength);

//                    client.GetStream().Read(new byte[client.Available], 0, a);
//                    continue;
                    FileStream f1s = File.Create("blocks"+ss.ToString());
                    BinaryReader nr = new BinaryReader(client.GetStream());
                    byte[]r = nr.ReadBytes(client.Available);
                    f1s.Write(r, 0, r.Length);
                    f1s.Close();
                    ss++;
                }
            }
            //client.Close();
            Console.ReadLine();
            FileStream file = File.OpenRead("BlockChainStream.blocks");
            var bh = new BlockStream(file);
            var np = new Discovery();
            
            while (true)
            {
                np.Discover();
                Thread.Sleep(5000);
                foreach(var n in np.GetEndPoints().ToArray())
                {
                    Console.WriteLine("EndPoint: {0}",n);
                }
                Console.WriteLine();
            }
        }
    }
}
