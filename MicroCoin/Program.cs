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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCoin
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static async Task<int> Main(string[] args) 
        {
            Thread.CurrentThread.Name = "Main";
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date{yyyy-MM-dd HH:mm:ss} %-5level %logger [%thread] %message%newline";
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
                log.DebugFormat("Network CheckPointBlock height: {0}. My CheckPointBlock height: {1}", e.HelloResponse.Block.BlockNumber, BlockChain.Instance.BlockHeight());
                microCoinClient.BlockResponse += (ob, eb) => {
                    log.DebugFormat("Received {0} CheckPointBlock from blockchain. BlockChain size: {1}. CheckPointBlock height: {2}", eb.BlockResponse.Blocks.Count, BlockChain.Instance.Count, eb.BlockResponse.Blocks.Last().BlockNumber);
                    BlockChain.Instance.AppendAll(eb.BlockResponse.Blocks);
                    if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                    {
                        microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()+1), 100);
                    }
                    else
                    {
                    }
                };
                if (BlockChain.Instance.BlockHeight() < e.HelloResponse.Block.BlockNumber)
                {
                    microCoinClient.RequestBlockChain((uint)(BlockChain.Instance.BlockHeight()), 100);
                }
            };

            Node node = await Node.StartNode();
            string block;
            do
            {
                block = Console.ReadLine();
                if (block == "l")
                {
                    var t = BlockChain.Instance.GetLastBlock();
                    log.Info($"Last block: {t.BlockNumber} {t.CompactTarget} {t.Nonce} {t.Payload} {t.ProofOfWork}");
                    foreach (var tt in t.Transactions)
                    {
                        log.Info($"\t {tt.SignerAccount} => {tt.TargetAccount}");
                    }
                    continue;
                }
                if (block == "q") break;
                try
                {
                    var t = BlockChain.Instance.Get(Convert.ToInt32(block));
                    log.Info($"Last block: {t.BlockNumber} {t.CompactTarget} {t.Nonce} {t.Payload} {t.ProofOfWork} {t.Transactions.Count}");
                    foreach (var tt in t.Transactions)
                    {
                        log.Info($"\t {tt.SignerAccount} => {tt.TargetAccount}");
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);
                }
            } while (block != "q");
            Node.Instance.Dispose();
            microCoinClient.Dispose();
            Console.ReadLine();
            return 0;
        }
        
    }
}