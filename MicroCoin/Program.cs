﻿using System;
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
    class Program
    {
        static void Main(string[] args)
        {
            List<TransactionBlock> list = new List<TransactionBlock>();
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.HelloResponse += (o, e) =>
            {
                Console.WriteLine("BlockChain to receive: {0}", e.TransactionBlock.BlockNumber);                
                microCoinClient.BlockResponse += (ob, eb) => {
                    foreach (var l in eb.BlockTransactions)
                    {
                        list.Add(l);
                    }
                    Console.WriteLine("Received {0} Block from blockchain. BlockChain size: {1}, End block: {2}", eb.BlockTransactions.Count, list.Count, list.Last().BlockNumber);
                    if (list.Last().BlockNumber < e.TransactionBlock.BlockNumber)
                    {
                        microCoinClient.RequestBlockChain(list.Last().BlockNumber+1, 100);
                    }
                    else
                    {
                        Console.WriteLine("Saving blockChain");
                        FileStream fileStream = File.Create("blockchain");
                        foreach(var a in list)
                        {
                            a.SaveToStream(fileStream);
                        }
                        fileStream.Close();
                    }
                    //await Task.Delay(0);
                };                                
                microCoinClient.RequestBlockChain(1, 100);
                //await Task.Delay(1);
            };
            microCoinClient.Start();
            microCoinClient.SendHello();
            Console.ReadLine();
        }
    }
}