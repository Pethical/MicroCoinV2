﻿//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// CheckPoint.cs - Copyright (c) 2018 Németh Péter
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
//-------------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using log4net;
using MicroCoin.Transactions;
using MicroCoin.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroCoin.Chain
{
    public class CheckPoint
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object loadLock = new object();
        private uint currentIndex = 0;
        private Stream stream;
        public const string CheckPointFileName = "checkpoint";
        public ulong WorkSum { get; set; }
        public CheckPointHeader Header { get; set; }

        public uint BlockCount
        {
            get
            {
                if (Header == null) return 0;
                return Header.BlockCount;
            }
        }
        public List<Account> Accounts { get; set; } = new List<Account>();                

        public CheckPointBlock this[uint i]
        {
            get
            {
                long p = stream.Position;
//		log.Info(Header.BlockOffset(i));
                stream.Position = Header.BlockOffset(i);
                CheckPointBlock block = new CheckPointBlock(stream);
                stream.Position = p;
                return block;
            }
            set
            {
            }
        }

        public CheckPoint()
        {

        }

        public CheckPoint(Stream s)
        {
            LoadFromStream(s);
        } 
        
        public static void SaveList(List<CheckPointBlock> list, Stream stream, Stream indexStream)
        {
            List<uint> offsets = new List<uint>();
            foreach(var item in list)
            {
                long pos = stream.Position;
                offsets.Add((uint)pos);
                item.SaveToStream(stream);
            }
            using (BinaryWriter bw = new BinaryWriter(indexStream, Encoding.Default, true))
            {
                foreach (uint u in offsets)
                {
                    bw.Write(u);
                }
            }
        }

        public static Hash CheckPointHash(List<CheckPointBlock> checkpoint)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var block in checkpoint)
                {
                    ms.Write(block.BlockHash, 0, block.BlockHash.Length);
                }
                System.Security.Cryptography.SHA256Managed sha = new System.Security.Cryptography.SHA256Managed();
                ms.Position = 0;
                return sha.ComputeHash(ms);
            }           
        }

        public static List<CheckPointBlock> BuildFromBlockChain(BlockChain blockChain)
        {
            List<CheckPointBlock> checkPoint = new List<CheckPointBlock>(blockChain.BlockHeight()+1);
            uint accNumber = 0;
            ulong accWork = 0;
            for (int block=0; block < 100*((blockChain.GetLastBlock().BlockNumber+1)/100); block++)                
            {
                if (block % 1000==0)
                {
                    log.Info($"Building checkpont: {block} block");
                }
                Block b = blockChain.Get(block);
                CheckPointBlock checkPointBlock = new CheckPointBlock();
                checkPointBlock.AccountKey = b.AccountKey;
                for (int i = 0; i < 5; i++) {
                    checkPointBlock.Accounts.Add(new Account
                    {
                        AccountNumber=accNumber,
                        Balance = (i==0?1000000ul+b.Fee:0ul),
                        BlockNumber = b.BlockNumber,
                        UpdatedBlock = b.BlockNumber,
                        NumberOfOperations=0,
                        AccountType = 0,
                        Name="",
                        UpdatedByBlock=b.BlockNumber,
                        AccountInfo = new AccountInfo
                        {
                            AccountKey = b.AccountKey,
                            State = AccountInfo.AccountState.Normal,                            
                        }
                    });
                    accNumber++;
                }
                accWork+=b.CompactTarget;                
                checkPointBlock.AccumulatedWork = accWork;
                checkPointBlock.AvailableProtocol = b.AvailableProtocol;
                checkPointBlock.BlockNumber = b.BlockNumber;
                checkPointBlock.BlockSignature = 2;//b.BlockSignature;
                checkPointBlock.CheckPointHash = b.CheckPointHash;
                checkPointBlock.CompactTarget = b.CompactTarget;
                checkPointBlock.Fee = b.Fee;
                checkPointBlock.Nonce = b.Nonce;
                checkPointBlock.Payload = b.Payload==null?new ByteString(new byte[0]):b.Payload;
                checkPointBlock.ProofOfWork = b.ProofOfWork;
                checkPointBlock.ProtocolVersion = b.ProtocolVersion;
                checkPointBlock.Reward = b.Reward;
                checkPointBlock.Timestamp = b.Timestamp;
                checkPointBlock.TransactionHash = b.TransactionHash;
                foreach(var t in b.Transactions)
                {
		            CheckPointBlock signer, target;
                    Account account;
                    signer = checkPoint.FirstOrDefault(p => p.Accounts.Count(a => a.AccountNumber == t.SignerAccount) > 0);
                    target = checkPoint.FirstOrDefault(p => p.Accounts.Count(a => a.AccountNumber == t.TargetAccount) > 0);
                    if(t.Fee!=0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                    switch (t.TransactionType)
                    {
                        case TransactionType.Transaction:
                            TransferTransaction transfer = (TransferTransaction)t;
                            if(signer!=null && target != null)
                            {
                                if (t.Fee == 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= (transfer.Fee + transfer.Amount);
                                target.Accounts.First(p => p.AccountNumber == t.TargetAccount).Balance += transfer.Amount;
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = b.BlockNumber;
                                target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock = b.BlockNumber;
                            }
                            break;
                        case TransactionType.BuyAccount:
                            TransferTransaction transferTransaction = (TransferTransaction)t; // TODO: be kell fejezni
                            if (t.Fee == 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= (transferTransaction.Fee + transferTransaction.Amount);
                            CheckPointBlock seller = checkPoint.FirstOrDefault(p => p.Accounts.Count(a => a.AccountNumber == transferTransaction.SellerAccount) > 0);
                            seller.Accounts.First(p => p.AccountNumber == transferTransaction.SellerAccount).Balance += transferTransaction.Amount;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = b.BlockNumber;
                            target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock = b.BlockNumber;
                            seller.Accounts.First(p => p.AccountNumber == transferTransaction.SellerAccount).UpdatedBlock = b.BlockNumber;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            account.AccountInfo.AccountKey = transferTransaction.NewAccountKey;
                            account.AccountInfo.Price = 0;
                            account.AccountInfo.LockedUntilBlock = 0;
                            account.AccountInfo.State = AccountInfo.AccountState.Normal;
                            account.AccountInfo.AccountToPayPrice = 0;
                            account.AccountInfo.NewPublicKey = null;
                            break;
                        case TransactionType.DeListAccountForSale:
                        case TransactionType.ListAccountForSale:
                            ListAccountTransaction listAccountTransaction = (ListAccountTransaction)t;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= listAccountTransaction.Fee;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = b.BlockNumber;
                            target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock = b.BlockNumber;
                            if (t.Fee == 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            if (signer!=null && target != null)
                            {
                                if (listAccountTransaction.TransactionType == ListAccountTransaction.AccountTransactionType.ListAccount) {
                                    account.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                    account.AccountInfo.LockedUntilBlock = listAccountTransaction.LockedUntilBlock;
                                    account.AccountInfo.State = AccountInfo.AccountState.Sale;                                    
                                    account.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                    account.AccountInfo.NewPublicKey = listAccountTransaction.NewPublicKey;
                                    account.AccountInfo.AccountToPayPrice = listAccountTransaction.AccountToPay;
                                }
                                else
                                {
                                    account.AccountInfo.State = AccountInfo.AccountState.Normal;
                                    account.AccountInfo.Price = 0;
                                    account.AccountInfo.NewPublicKey = null;
                                    account.AccountInfo.LockedUntilBlock = 0;
                                    account.AccountInfo.AccountToPayPrice = 0;
                                }
                            }
                            break;
                        case TransactionType.ChangeAccountInfo:
                            ChangeAccountInfoTransaction changeAccountInfoTransaction = (ChangeAccountInfoTransaction)t;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            if ((changeAccountInfoTransaction.ChangeType & 1) == 1)
                            {
                                account.AccountInfo.AccountKey = changeAccountInfoTransaction.NewAccountKey;
                            }
                            if ((changeAccountInfoTransaction.ChangeType & 4) == 4)
                            {
                                account.AccountType = changeAccountInfoTransaction.NewType;
                            }
                            if ((changeAccountInfoTransaction.ChangeType & 2) == 2)
                            {
                                account.Name = changeAccountInfoTransaction.NewName;
                            }
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= changeAccountInfoTransaction.Fee;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = b.BlockNumber;
                            account.UpdatedBlock = b.BlockNumber;                            
                            if (t.Fee == 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            break;
                        case TransactionType.ChangeKey:
                        case TransactionType.ChangeKeySigned:
                            ChangeKeyTransaction changeKeyTransaction = (ChangeKeyTransaction)t;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= changeKeyTransaction.Fee;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            account.AccountInfo.AccountKey = changeKeyTransaction.NewAccountKey;
                            account.UpdatedBlock = b.BlockNumber;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = b.BlockNumber;
                            if (t.Fee == 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            break;
                    }
                }
                checkPoint.Add(checkPointBlock);
            }
            foreach(var p in checkPoint) p.BlockHash = p.CalculateBlockHash();
            return checkPoint;
        }

        public void LoadFromFile(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite);
            if (stream != null) stream.Dispose();
            stream = null;
            stream = fs;
            Accounts.Clear();
            Accounts = null;
            GC.Collect();
            Header = new CheckPointHeader(stream);
            Accounts = new List<Account>();
            WorkSum = 0;
            for (uint i = 0; i < Header.EndBlock - Header.StartBlock; i++)
            {
                WorkSum += this[i].CompactTarget;
                foreach (Account a in this[i].Accounts)
                {
                    
                    Accounts.Add(a);
                    // Accounts.AddRange(b.Accounts);
                }
            }
        }
        public void LoadFromStream(Stream s)
        {
            if (stream != null) stream.Dispose();
            stream = null;
            stream = new MemoryStream();
            s.CopyTo(stream);
            // stream = s;
            stream.Position = 0;
            Header = new CheckPointHeader(stream);
            Accounts = new List<Account>();
            WorkSum = 0;
            for (uint i = 0; i < Header.EndBlock - Header.StartBlock; i++)
            {
//		log.Info(i);
		try{
                    var b = this[i];
//		log.Info($"checkP {b.CompactTarget}");
                WorkSum += b.CompactTarget;
//		log.Info($"checkP {i}");
                Accounts.AddRange(b.Accounts);
		}catch(Exception e){
		    log.Error(e.Message, e);
		}
            }
            log.Info(WorkSum);
        }

        public void Append(CheckPoint checkPoint)
        {
            lock (loadLock)
            {
                List<CheckPointBlock> list = new List<CheckPointBlock>();
                if (Header != null)
                {
                    for (uint i = Header.StartBlock; i < Header.EndBlock+1; i++)
                    {
                        list.Add(this[i]);
                    }
                    if (Header.BlockCount > checkPoint.Header.BlockCount)
                    {
                        return;
                    }
                    Header.BlockCount = checkPoint.Header.BlockCount;
                    Header.Hash = checkPoint.Header.Hash;
                    Header.EndBlock = checkPoint.Header.EndBlock;
                }
                else
                {
                    Header = checkPoint.Header;
                }
                for (uint i = 0; i != checkPoint.Header.EndBlock- checkPoint.Header.StartBlock + 1; i++){
//		    log.Info($"Added block {i}");
                    list.Add(checkPoint[i]);
		}
                MemoryStream ms = new MemoryStream();
                uint[] h = Header.offsets;
                Header.offsets = new uint[list.Count + 1];
                Header.SaveToStream(ms);
                long headerSize = ms.Position;
                ms.Position = 0;
//		log.Info("checkPoint 1");
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    uint i = 0;
                    foreach (CheckPointBlock b in list)
                    {
                        Header.offsets[i] = (uint)(ms.Position + headerSize);
                        b.SaveToStream(ms);
                        i++;
                    }
                }
//		log.Info("checkPoint 2");
                MemoryStream memoryStream = new MemoryStream();
                Header.SaveToStream(memoryStream);
                ms.Position = 0;
                ms.CopyTo(memoryStream);
                memoryStream.Write(Header.Hash, 0, Header.Hash.Length);
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                memoryStream.Position = 0;
//		log.Info("checkPoint 3");
                LoadFromStream(memoryStream);
//		log.Info("checkPoint 4");
                ms.Dispose();
                memoryStream.Dispose();
                ms = null;
                memoryStream = null;
                GC.Collect();
//		log.Info("checkPoint appended");
            }
        }

        public void SaveToStream(Stream s)
        {
            long pos = stream.Position;
            stream.Position = 0;
            stream.CopyTo(s);
            stream.Position = pos;
        }

        public void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();                
                Accounts.Clear();
                Accounts = null;
                stream = null;
            }
        }

        public CheckPoint SaveChunk(uint startBlock, uint endBlock)
        {
            lock (loadLock)
            {
                List<CheckPointBlock> list = new List<CheckPointBlock>();
                CheckPointHeader Header = new CheckPointHeader();
                Header.StartBlock = startBlock;
                Header.EndBlock = endBlock;
                Header.BlockCount = endBlock - startBlock + 1;
                for (uint i = startBlock; i <= endBlock; i++)
                    list.Add(this[i]);
                MemoryStream ms = new MemoryStream();
                Header.offsets = new uint[list.Count + 1];
                Header.SaveToStream(ms);
                long headerSize = ms.Position;
                ms.Position = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    uint i = 0;
                    foreach (CheckPointBlock b in list)
                    {
                        Header.offsets[i] = (uint)(ms.Position + headerSize);
                        b.SaveToStream(ms);
                        i++;
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                Header.SaveToStream(memoryStream);
                ms.Position = 0;
                ms.CopyTo(memoryStream);
                memoryStream.Write(Header.Hash, 0, Header.Hash.Length);
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                memoryStream.Position = 0;
                CheckPoint checkPoint = new CheckPoint(memoryStream);
                ms.Dispose();
                memoryStream.Dispose();
                ms = null;
                memoryStream = null;
                GC.Collect();
                return checkPoint;
            }
        }

    }
}