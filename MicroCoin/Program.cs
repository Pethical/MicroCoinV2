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
