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


using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MicroCoin.Chain;
using MicroCoin.Net;
using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;

namespace MicroCoin
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args) 
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date{yyyy-MM-dd HH:mm:ss} %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();
            ManagedColoredConsoleAppender consoleAppender = new ManagedColoredConsoleAppender();
            consoleAppender.Layout = patternLayout;
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors()
            {
                ForeColor = ConsoleColor.Yellow,
                Level = Level.Warn
            });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors()
            {
                ForeColor = ConsoleColor.Cyan,
                Level = Level.Info
            });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors()
            {
                ForeColor = ConsoleColor.DarkGray,
                Level = Level.Debug
            });

            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors()
            {
                ForeColor = ConsoleColor.Red,
                Level = Level.Error
            });
            consoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(consoleAppender);
            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);
            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;

            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.HelloResponse += (o, e) =>
            {
               log.DebugFormat("Network BlockHeight: {0}. My BlockHeight: {1}", e.HelloResponse.TransactionBlock.BlockNumber, BlockChain.Instance.BlockHeight());
                microCoinClient.BlockResponse += (ob, eb) => {
                    log.DebugFormat("Received {0} Block from blockchain. BlockChain size: {1}. Block height: {2}", eb.BlockResponse.BlockTransactions.Count, BlockChain.Instance.Count, eb.BlockResponse.BlockTransactions.Last().BlockNumber);
//                    foreach (var l in eb.BlockResponse.BlockTransactions)
//                    {
                        /*if (l.BlockNumber > BlockChain.Instance.BlockHeight())
                        {
                            log.Debug($"Appending block {l.BlockNumber}");
                            
                        }*/
//                    }
                    BlockChain.Instance.AppendAll(eb.BlockResponse.BlockTransactions);
                    //log.DebugFormat("Received {0} Block from blockchain. BlockChain size: {1}, End block: {2}", eb.BlockResponse.BlockTransactions.Count, BlockChain.Instance.Count, BlockChain.Instance.Last().BlockNumber);
                    if (BlockChain.Instance.BlockHeight() < e.HelloResponse.TransactionBlock.BlockNumber)
                    {
                        microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()+1), 100);
                    }
                    else
                    {
/*                        log.Debug("Saving blockChain");
                        FileStream fileStream = File.Open("block.chain",FileMode.OpenOrCreate,FileAccess.ReadWrite);
                        BlockChain.Instance.SaveToStorage(fileStream);
                        //foreach(var a in list)
                        //{
                        //    a.SaveToStream(fileStream);
                        //}
                        fileStream.Close();
                        log.Debug("Saved");*/
                    }
                    //await Task.Delay(0);
                };
                if (BlockChain.Instance.BlockHeight() < e.HelloResponse.TransactionBlock.BlockNumber)
                {
                    microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                }
                /*
                foreach (var node in Node.Instance.NodeServers)
                {
                    var mc = node.Value.Connect();
                    if(mc!=null)
                        mc.SendHello();
                }
                */
                /*
                var badNodes = Node.Instance.NodeServers.Where(p => p.Value.MicroCoinClient?.Connected == false);                
                foreach(var b in badNodes)
                {
                    Node.Instance.NodeServers.TryRemove(b.Key, out NodeServer ns);
                    log.Info($"{ns} removed");
                }*/
                // microCoinClient.RequestBlockChain(1, 100);
                // await Task.Delay(1);
            };
	        log.Debug("Start Node Client...");
            microCoinClient.Start( "127.0.0.1" ,4004);
	        log.Debug("Sendign Hellos...");
            microCoinClient.SendHello();
            var t = BlockChain.Instance.GetLastTransactionBlock();
            log.Info($"Last block data: {t.BlockNumber} {t.CompactTarget} {t.Nonce} {t.PayloadString} {t.ProofOfWork.ToHexString()})");
            Console.ReadLine();
        }
    }
}
