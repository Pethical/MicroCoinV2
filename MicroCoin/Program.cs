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


using MicroCoin.BlockChain;
using MicroCoin.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MicroCoin
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs = File.OpenRead("checkpoint7");
            var sh = new Snapshot(fs);
            Block b = sh[1002];
            sh.Reset();                        
            foreach (var bas in sh.Accounts.Where(p=>p.Name!=""))
            {
                Console.WriteLine($"{bas.AccountNumber} {bas.Name} {bas.Balance}");
            }            
            Console.WriteLine(b.Reward);

            Util.MicroCoin m = new Util.MicroCoin();
              m = 1000;
            List<TransactionBlock> list = new List<TransactionBlock>();
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.HelloResponse += (o, e) =>
            {
                Console.WriteLine("BlockChain to receive: {0}", e.HelloResponse.TransactionBlock.BlockNumber);                
                microCoinClient.BlockResponse += (ob, eb) => {
                    foreach (var l in eb.BlockResponse.BlockTransactions)
                    {
                        list.Add(l);
                    }
                    Console.WriteLine("Received {0} Block from blockchain. BlockChain size: {1}, End block: {2}", eb.BlockResponse.BlockTransactions.Count, list.Count, list.Last().BlockNumber);
                    if (list.Last().BlockNumber < e.HelloResponse.TransactionBlock.BlockNumber)
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
