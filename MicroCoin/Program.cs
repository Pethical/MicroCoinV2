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
using MicroCoin.BlockChain;
using MicroCoin.Net;
using MicroCoin.Protocol;
using MicroCoin.Cryptography;
using MicroCoin.Net.Discovery;

namespace MicroCoin
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
            List<TransactionBlock> list = new List<TransactionBlock>();
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.BlockResponse += async (o, e) => {
                foreach(var l in e.OperationBlocks)
                {
                    list.Add(l);
                }
                await Task.Delay(0);
                Console.WriteLine("Received {0} Block from blockchain. BlockChain size: {1}, End block: {2}", e.OperationBlocks.Count, list.Count, list.Last().BlockNumber);
            };
            microCoinClient.HelloResponse += async (o, e) =>
            {
                Console.WriteLine("BlockChain to receive: {0}", e.OperationBlock.BlockNumber);
                for (uint i = 1; i < e.OperationBlock.BlockNumber; i += 100)
                {
                    microCoinClient.RequestBlockChain(i, 100);
                    await Task.Delay(100);
                    //Thread.Sleep(100);
                }
            };
            microCoinClient.Start();
            microCoinClient.SendHello();
            Console.ReadLine();
        }
    }
}
