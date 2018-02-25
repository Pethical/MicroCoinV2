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
            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);
            int port = 4004;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
            }
            Node node = await Node.StartNode(port);
            log.Info($"Last account: {CheckPoints.Accounts.Last().AccountNumber}");
            for(int i=0;i<CheckPoints.Accounts.Count;i++)
            {
                if (CheckPoints.Accounts[i].AccountNumber != i)
                {
                    log.Error("Hiba");
                }
            }
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
            Console.ReadLine();
            return 0;
        }
        
    }
}