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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MicroCoin
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();
            ColoredConsoleAppender consoleAppender = new ColoredConsoleAppender();
            consoleAppender.Layout = patternLayout;
            consoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                ForeColor = ColoredConsoleAppender.Colors.Yellow,
                Level = Level.Warn
            });
            consoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                ForeColor = ColoredConsoleAppender.Colors.Red,
                Level = Level.Error
            });

            consoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(consoleAppender);
            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            MicroCoinClient microCoinClient = new MicroCoinClient();
            microCoinClient.HelloResponse += (o, e) =>
            {
                log.DebugFormat("BlockChain to receive: {0}", e.HelloResponse.TransactionBlock.BlockNumber);                
                microCoinClient.BlockResponse += (ob, eb) => {
                    foreach (var l in eb.BlockResponse.BlockTransactions)
                    {
                        log.DebugFormat("Received {0} Block from blockchain. BlockChain size: {1}. Block height: {2}", eb.BlockResponse.BlockTransactions.Count, BlockChain.Instance.Count, eb.BlockResponse.BlockTransactions.Last().BlockNumber);
                        if (l.BlockNumber > BlockChain.Instance.BlockHeight())
                        {
                            log.Debug($"Appending block {l.BlockNumber}");
                            BlockChain.Instance.Append(l);
                        }
                    }
                    //log.DebugFormat("Received {0} Block from blockchain. BlockChain size: {1}, End block: {2}", eb.BlockResponse.BlockTransactions.Count, BlockChain.Instance.Count, BlockChain.Instance.Last().BlockNumber);
                    if (BlockChain.Instance.BlockHeight() < e.HelloResponse.TransactionBlock.BlockNumber)
                    {
                        microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
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
//                microCoinClient.RequestBlockChain(1, 100);
                //await Task.Delay(1);
            };
            microCoinClient.Start();
            microCoinClient.SendHello();
            Console.ReadLine();
        }
    }
}
