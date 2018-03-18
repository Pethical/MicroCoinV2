//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// Program.cs - Copyright (c) 2018 Németh Péter
//-----------------------------------------------------------------------
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//-----------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


namespace MicroCoin
{
    class Program
    {
        static int Main(string[] args) 
        {
            return 0;
            /*             
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
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);
            int port = 4004;
            if (args.Length > 0)
            {
                port = Convert.ToInt32(args[0]);
            }
            var key = ECKeyPair.CreateNew(false);
            var x = key.PublicKey.AffineXCoord.GetEncoded();
            var y = key.PublicKey.AffineYCoord.GetEncoded();
            CngKeyCreationParameters creationParameters = new CngKeyCreationParameters();
            creationParameters.ExportPolicy = CngExportPolicies.AllowPlaintextExport;            
            var objCngKey = CngKey.Create(CngAlgorithm.ECDsaP256);
            Hash ha = objCngKey.Export(CngKeyBlobFormat.EccPublicBlob);
            ECDsaCng cDsaCng = new ECDsaCng(objCngKey);
            Console.WriteLine(ha);
            Console.ReadLine();
            byte[] magic = { 0x31, 0x53, 0x43, 0x45 };
            List<byte> l = new List<byte>();
            l.AddRange(magic.Reverse());
            l.Add(0x20);
            l.Add(0);
            l.Add(0);
            l.Add(0);
            l.AddRange(key.PublicKey.GetEncoded(false).Skip(1).ToArray());            
            Hash h = l.ToArray();            
            Console.WriteLine(h);
            Console.ReadLine();
            byte[] b = ha;
            byte[] xa = b.Skip(8).Take(32).ToArray();
            byte[] ya = b.Skip(40).Take(32).ToArray();
            ECKeyPair cKeyPair = new ECKeyPair();
            cKeyPair.CurveType = CurveType.Secp256K1;            
            cKeyPair.X = xa;
            cKeyPair.Y = ya;
            Hash enc = cKeyPair.PublicKey.GetEncoded(false);
            Console.WriteLine(enc);
            l.Clear();
            l.AddRange(magic.Reverse());
            l.Add(0x20);
            l.Add(0);
            l.Add(0);
            l.Add(0);
            l.AddRange(cKeyPair.PublicKey.GetEncoded(false).Skip(1));
            Hash bc = "a34b99f22c790c4e36b2b3c2c35a36db06226e41c692fc82b8b56ac1c540c5bd5b8dec5235a0fa8722476c7709c02559e3aa73aa03918ba2d492eea75abea235";
            //CngKey cKey = CngKey.Import(bc, CngKeyBlobFormat.EccPublicBlob);
            ECParameters parameters = new ECParameters();            
            ECCurve curve = ECCurve.CreateFromFriendlyName("secp256k1");
            parameters.Curve = curve;
            parameters.Q.X = x;
            parameters.Q.Y = y;
            byte[] D = key.PrivateKey.ToByteArray();
            if (D[0] == 0)
            {
                D = D.Skip(1).ToArray();
            }
            parameters.D = D;
            var eC = ECDsa.Create(parameters);
            ByteString sd = "hello";
            Hash sign = eC.SignData(sd, HashAlgorithmName.SHA256);
            bool valid = eC.VerifyData(sd, sign, HashAlgorithmName.SHA256);
            // Console.WriteLine(cKey.KeyName);
            Console.WriteLine(h);
            Console.ReadLine();
            ECKeyPair pair = ECKeyPair.CreateNew(false);
            ECParameters parameters = new ECParameters();
            ECCurve curve = ECCurve.CreateFromFriendlyName("secp256k1");
            parameters.Curve = curve;
            parameters.Q.X = pair.X;
            parameters.Q.Y = pair.Y;
            byte[] D = pair.PrivateKey.ToByteArray();
            if (D[0] == 0)
            {
                D = D.Skip(1).ToArray();
            }
            parameters.D = D;
            var eC = ECDsa.Create(parameters);
            ByteString sd = "hello";
            Hash sign = eC.SignHash(sd);            
            
            ECSignature sig = new ECSignature();
            byte[] signature = sign;
            sig.r = signature.Take(32).ToArray();
            sig.s = signature.Skip(32).Take(32).ToArray();
            pair.ValidateSignature(sd, sig);
            Node node = Node.StartNode(port).Result;            
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
            */
        }
        
    }
}