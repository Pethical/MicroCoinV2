//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// CheckPoints.cs - Copyright (c) 2018 Németh Péter
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


using log4net;
using MicroCoin.Transactions;
using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MicroCoin.Chain
{

    public class CheckPointBuildingEventArgs
    {
        public int BlocksNeeded { get; set; }
        public int BlocksDone { get; set; }

    }

    public class CheckPoints
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string CheckPointIndexName = "checkpoints.idx";
        public const string CheckPointFileName = "checkpoints.mcc";
        public static event EventHandler<CheckPointBuildingEventArgs> CheckPointBuilding;
        private static List<uint> _offsets = new List<uint>();
        internal static ulong WorkSum { get; set; }
        internal static List<Account> Accounts { get; set; } = new List<Account>();
        internal static List<CheckPointBlock> Current { get; set; } = new List<CheckPointBlock>();
        internal static void Init()
        {
            WorkSum = 0;
            Current = new List<CheckPointBlock>();
            if (!File.Exists((CheckPointFileName))) return;
            try
            {
                FileStream fs = File.Open(CheckPointIndexName, FileMode.Open);
                _offsets = new List<uint>((int) (fs.Length / 4));
                Accounts = new List<Account>();
                using (BinaryReader br = new BinaryReader(fs))
                {
                    while (fs.Position < fs.Length)
                    {
                        _offsets.Add(br.ReadUInt32());
                    }
                }

                using (FileStream cf = File.OpenRead(CheckPointFileName))
                {
                    while (cf.Position < cf.Length)
                    {
                        var block = new CheckPointBlock(cf);
                        WorkSum += block.CompactTarget;
                        Accounts.AddRange(block.Accounts);
                        Current.Add(block);
                    }
                }
            }
            catch
            {
            }

            Log.Info($"Accounts: {Accounts.Last().AccountNumber}");
        }
        internal static void Put(CheckPointBlock cb)
        {
            using (FileStream fs = File.OpenWrite(CheckPointFileName))
            {
                uint position;
                if (_offsets.Count <= cb.BlockNumber)
                {
                    position = (uint) fs.Length;
                    _offsets.Add(position);
                }
                else
                {
                    position = _offsets[(int) cb.BlockNumber];
                }

                fs.Position = position;
                cb.SaveToStream(fs);
            }
        }
        internal static CheckPointBlock Get(uint i)
        {
            return Current[(int) i];
        }
        internal static Account Account(int i)
        {
            return Accounts[i];
        }
        internal static void SaveList(List<CheckPointBlock> list, Stream stream, Stream indexStream)
        {
            List<uint> offsets = new List<uint>();
            foreach (var item in list)
            {
                long pos = stream.Position;
                offsets.Add((uint) pos);
                item.SaveToStream(stream);
                if (item.BlockNumber % 100 == 0)
                {
                    CheckPointBuilding?.Invoke(new object(), new CheckPointBuildingEventArgs
                    {
                        BlocksDone = (int)(item.BlockNumber),
                        BlocksNeeded = (int)(list.Count)
                    });
                }
            }

            using (BinaryWriter bw = new BinaryWriter(indexStream, Encoding.Default, true))
            {
                foreach (uint u in offsets)
                {
                    bw.Write(u);
                }
            }
        }
        internal static List<CheckPointBlock> ReadAll()
        {
            List<CheckPointBlock> list = new List<CheckPointBlock>(_offsets.Count);
            using (FileStream fs = File.OpenRead(CheckPointFileName))
            {
                while (fs.Position < fs.Length)
                {
                    list.Add(new CheckPointBlock(fs));
                }
            }

            return list;
        }
        internal static Hash CheckPointHash(List<CheckPointBlock> checkpoint)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var block in checkpoint)
                {
                    block.BlockHash = block.CalculateBlockHash();
                    ms.Write(block.BlockHash, 0, block.BlockHash.Length);
                }
                var sha = new SHA256Managed();
                ms.Position = 0;
                return sha.ComputeHash(ms);
            }
        }
        internal static List<CheckPointBlock> BuildFromBlockChain(BlockChain blockChain)
        {
            List<CheckPointBlock> checkPoint = new List<CheckPointBlock>(blockChain.BlockHeight() + 1);
            uint accNumber = 0;
            ulong accWork = 0;
            for (int block = 0; block < 100 * ((blockChain.GetLastBlock().BlockNumber + 1) / 100); block++)
            {
                Block currentBlock = blockChain.Get(block);
                CheckPointBlock checkPointBlock = new CheckPointBlock {AccountKey = currentBlock.AccountKey};
                for (int i = 0; i < 5; i++)
                {
                    checkPointBlock.Accounts.Add(new Account
                    {
                        AccountNumber = accNumber,
                        Balance = (i == 0 ? 1000000ul + (ulong) currentBlock.Fee : 0ul),
                        BlockNumber = currentBlock.BlockNumber,
                        UpdatedBlock = currentBlock.BlockNumber,
                        NumberOfOperations = 0,
                        AccountType = 0,
                        Name = "",
                        UpdatedByBlock = currentBlock.BlockNumber,
                        AccountInfo = new AccountInfo
                        {
                            AccountKey = currentBlock.AccountKey,
                            State = AccountState.Normal
                        }
                    });
                    accNumber++;
                }

                accWork += currentBlock.CompactTarget;
                checkPointBlock.AccumulatedWork = accWork;
                checkPointBlock.AvailableProtocol = currentBlock.AvailableProtocol;
                checkPointBlock.BlockNumber = currentBlock.BlockNumber;
                checkPointBlock.BlockSignature = 2; //b.BlockSignature;
                checkPointBlock.CheckPointHash = currentBlock.CheckPointHash;
                checkPointBlock.CompactTarget = currentBlock.CompactTarget;
                checkPointBlock.Fee = currentBlock.Fee;
                checkPointBlock.Nonce = currentBlock.Nonce;
                checkPointBlock.Payload = currentBlock.Payload;
                checkPointBlock.ProofOfWork = currentBlock.ProofOfWork;
                checkPointBlock.ProtocolVersion = currentBlock.ProtocolVersion;
                checkPointBlock.Reward = currentBlock.Reward;
                checkPointBlock.Timestamp = currentBlock.Timestamp;
                checkPointBlock.TransactionHash = currentBlock.TransactionHash;
                WorkSum += currentBlock.CompactTarget;
                foreach (var t in currentBlock.Transactions)
                {
                    Account account;
                    var signer = checkPoint.FirstOrDefault(p =>
                        p.Accounts.Count(a => a.AccountNumber == t.SignerAccount) > 0);
                    var target = checkPoint.FirstOrDefault(p =>
                        p.Accounts.Count(a => a.AccountNumber == t.TargetAccount) > 0);
                    if (t.Fee != 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                    switch (t.TransactionType)
                    {
                        case TransactionType.Transaction:
                            TransferTransaction transfer = (TransferTransaction) t;
                            if (signer != null && target != null)
                            {
                                if (t.Fee == 0)
                                    signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                                    (transfer.Fee + transfer.Amount);
                                target.Accounts.First(p => p.AccountNumber == t.TargetAccount).Balance +=
                                    transfer.Amount;
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock =
                                    currentBlock.BlockNumber;
                                target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock =
                                    currentBlock.BlockNumber;
                            }

                            break;
                        case TransactionType.BuyAccount:
                            TransferTransaction transferTransaction = (TransferTransaction) t; // TODO: be kell fejezni
                            if (t.Fee == 0)
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                                (transferTransaction.Fee + transferTransaction.Amount);
                            CheckPointBlock seller = checkPoint.FirstOrDefault(p =>
                                p.Accounts.Count(a => a.AccountNumber == transferTransaction.SellerAccount) > 0);
                            seller.Accounts.First(p => p.AccountNumber == transferTransaction.SellerAccount).Balance +=
                                transferTransaction.Amount;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = currentBlock.BlockNumber;
                            target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock = currentBlock.BlockNumber;
                            seller.Accounts.First(p => p.AccountNumber == transferTransaction.SellerAccount)
                                .UpdatedBlock = currentBlock.BlockNumber;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            account.AccountInfo.AccountKey = transferTransaction.NewAccountKey;
                            account.AccountInfo.Price = 0;
                            account.AccountInfo.LockedUntilBlock = 0;
                            account.AccountInfo.State = AccountState.Normal;
                            account.AccountInfo.AccountToPayPrice = 0;
                            account.AccountInfo.NewPublicKey = null;
                            break;
                        case TransactionType.DeListAccountForSale:
                        case TransactionType.ListAccountForSale:
                            ListAccountTransaction listAccountTransaction = (ListAccountTransaction) t;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                                listAccountTransaction.Fee;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = currentBlock.BlockNumber;
                            target.Accounts.First(p => p.AccountNumber == t.TargetAccount).UpdatedBlock = currentBlock.BlockNumber;
                            if (t.Fee == 0)
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            if (listAccountTransaction.TransactionType == TransactionType.ListAccountForSale)
                            {
                                account.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                account.AccountInfo.LockedUntilBlock = listAccountTransaction.LockedUntilBlock;
                                account.AccountInfo.State = AccountState.Sale;
                                account.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                account.AccountInfo.NewPublicKey = listAccountTransaction.NewPublicKey;
                                account.AccountInfo.AccountToPayPrice = listAccountTransaction.AccountToPay;
                            }
                            else
                            {
                                account.AccountInfo.State = AccountState.Normal;
                                account.AccountInfo.Price = 0;
                                account.AccountInfo.NewPublicKey = null;
                                account.AccountInfo.LockedUntilBlock = 0;
                                account.AccountInfo.AccountToPayPrice = 0;
                            }

                            break;
                        case TransactionType.ChangeAccountInfo:
                            ChangeAccountInfoTransaction changeAccountInfoTransaction =
                                (ChangeAccountInfoTransaction) t;
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

                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                                changeAccountInfoTransaction.Fee;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = currentBlock.BlockNumber;
                            account.UpdatedBlock = currentBlock.BlockNumber;
                            if (t.Fee == 0)
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            break;
                        case TransactionType.ChangeKey:
                        case TransactionType.ChangeKeySigned:
                            ChangeKeyTransaction changeKeyTransaction = (ChangeKeyTransaction) t;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                                changeKeyTransaction.Fee;
                            account = target.Accounts.First(p => p.AccountNumber == t.TargetAccount);
                            account.AccountInfo.AccountKey = changeKeyTransaction.NewAccountKey;
                            account.UpdatedBlock = currentBlock.BlockNumber;
                            signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).UpdatedBlock = currentBlock.BlockNumber;
                            if (t.Fee == 0)
                                signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                            break;
                    }
                }

                checkPoint.Add(checkPointBlock);
                if (block % 100 == 0)
                {
                    CheckPointBuilding?.Invoke(new object(), new CheckPointBuildingEventArgs
                    {
                        BlocksDone = block,
                        BlocksNeeded = (int)(100 * ((blockChain.GetLastBlock().BlockNumber + 1) / 100))*2
                    });
                }
            }

            foreach (var p in checkPoint) { p.BlockHash = p.CalculateBlockHash();

                if (p.BlockNumber % 100 == 0)
                {
                    CheckPointBuilding?.Invoke(new object(), new CheckPointBuildingEventArgs
                    {
                        BlocksDone = (int)(p.BlockNumber+checkPoint.Count),
                        BlocksNeeded = (int)(checkPoint.Count * 2)
                    });
                }

            }
            return checkPoint;
        }
        internal static CheckPointBlock GetLastBlock()
        {
            return Current.Count > 0 ? Current.Last() : null;
        }
        protected static void SaveNext()
        {
            var offsets2 = new List<uint>();
            uint chunk = (Current.Last().BlockNumber / 100) % 10;
            Log.Info($"Saving next checkpont => {chunk}");
            using (FileStream fs = File.Create(CheckPointFileName + $".{chunk}"))
            {
                foreach (var block in Current)
                {
                    offsets2.Add((uint) fs.Position);
                    block.SaveToStream(fs);
                }
            }

            using (FileStream fs = File.Create(CheckPointIndexName + $".{chunk}"))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    foreach (var o in offsets2) bw.Write(o);
                }
            }

            File.Copy(CheckPointIndexName + $".{chunk}", CheckPointIndexName, true);
            File.Copy(CheckPointFileName + $".{chunk}", CheckPointFileName, true);
        }
        internal static void AppendBlock(Block b)
        {
            int lastBlock = -1;
            if (GetLastBlock() != null)
            {
                if (b.BlockNumber <= GetLastBlock().BlockNumber) return;
                lastBlock = (int) GetLastBlock().BlockNumber;
            }

            CheckPointBlock checkPointBlock = new CheckPointBlock {AccountKey = b.AccountKey};
            uint accNumber = (uint) (lastBlock + 1) * 5;
            if (accNumber == 0)
            {
                Log.Info("NULL");
            }

            ulong accWork = WorkSum;
            for (int i = 0; i < 5; i++)
            {                
                checkPointBlock.Accounts.Add(new Account
                {
                    AccountNumber = accNumber,
                    Balance = (i == 0 ? 1000000ul + (ulong) b.Fee : 0ul),
                    BlockNumber = b.BlockNumber,
                    UpdatedBlock = b.BlockNumber,
                    NumberOfOperations = 0,
                    AccountType = 0,
                    Name = "",
                    UpdatedByBlock = b.BlockNumber,
                    AccountInfo = new AccountInfo
                    {
                        AccountKey = b.AccountKey,
                        State = AccountState.Normal
                    }
                });
                accNumber++;
            }

            accWork += b.CompactTarget;
            WorkSum += b.CompactTarget;
            checkPointBlock.AccumulatedWork = accWork;
            checkPointBlock.AvailableProtocol = b.AvailableProtocol;
            checkPointBlock.BlockNumber = b.BlockNumber;
            checkPointBlock.BlockSignature = 2;
            checkPointBlock.CheckPointHash = b.CheckPointHash;
            checkPointBlock.CompactTarget = b.CompactTarget;
            checkPointBlock.Fee = b.Fee;
            checkPointBlock.Nonce = b.Nonce;
            checkPointBlock.Payload = b.Payload;
            checkPointBlock.ProofOfWork = b.ProofOfWork;
            checkPointBlock.ProtocolVersion = b.ProtocolVersion;
            checkPointBlock.Reward = b.Reward;
            checkPointBlock.Timestamp = b.Timestamp;
            checkPointBlock.TransactionHash = b.TransactionHash;
            foreach (var t in b.Transactions)
            {
                Account signerAccount = Accounts[t.SignerAccount];
                Account targetAccount = Accounts[t.TargetAccount];
                var signerBlock = Get(signerAccount.BlockNumber);
                var targetBlock = Get(targetAccount.BlockNumber);
                if (t.Fee != 0) signerAccount.NumberOfOperations++;
                switch (t.TransactionType)
                {
                    case TransactionType.Transaction:
                        TransferTransaction transfer = (TransferTransaction) t;
                        if (signerBlock != null && targetBlock != null)
                        {
                            if (t.Fee == 0) signerAccount.NumberOfOperations++;
                            signerAccount.Balance -= (transfer.Fee + transfer.Amount);
                            signerAccount.UpdatedBlock = b.BlockNumber;
                            targetAccount.Balance += transfer.Amount;
                            targetAccount.UpdatedBlock = b.BlockNumber;
                            targetAccount.Saved = true;
                            signerAccount.Saved = true;
                        }

                        break;
                    case TransactionType.BuyAccount:
                        TransferTransaction transferTransaction = (TransferTransaction) t; // TODO: be kell fejezni
                        if (t.Fee == 0)
                            signerBlock.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                        signerBlock.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -=
                            (transferTransaction.Fee + transferTransaction.Amount);
                        Account sellerAccount = Accounts[transferTransaction.SellerAccount];
                        Get(sellerAccount.BlockNumber);
                        sellerAccount.Balance += transferTransaction.Amount;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        sellerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.AccountInfo.AccountKey = transferTransaction.NewAccountKey;
                        targetAccount.AccountInfo.Price = 0;
                        targetAccount.AccountInfo.LockedUntilBlock = 0;
                        targetAccount.AccountInfo.State = AccountState.Normal;
                        targetAccount.AccountInfo.AccountToPayPrice = 0;
                        targetAccount.AccountInfo.NewPublicKey = null;
                        targetAccount.Saved = true;
                        signerAccount.Saved = true;
                        break;
                    case TransactionType.DeListAccountForSale:
                    case TransactionType.ListAccountForSale:
                        ListAccountTransaction listAccountTransaction = (ListAccountTransaction) t;
                        signerAccount.Balance -= listAccountTransaction.Fee;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        if (t.Fee == 0) signerAccount.NumberOfOperations++;
                        if (signerBlock != null && targetBlock != null)
                        {
                            if (listAccountTransaction.TransactionType == TransactionType.ListAccountForSale)
                            {
                                targetAccount.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                targetAccount.AccountInfo.LockedUntilBlock = listAccountTransaction.LockedUntilBlock;
                                targetAccount.AccountInfo.State = AccountState.Sale;
                                targetAccount.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                targetAccount.AccountInfo.NewPublicKey = listAccountTransaction.NewPublicKey;
                                targetAccount.AccountInfo.AccountToPayPrice = listAccountTransaction.AccountToPay;
                            }
                            else
                            {
                                targetAccount.AccountInfo.State = AccountState.Normal;
                                targetAccount.AccountInfo.Price = 0;
                                targetAccount.AccountInfo.NewPublicKey = null;
                                targetAccount.AccountInfo.LockedUntilBlock = 0;
                                targetAccount.AccountInfo.AccountToPayPrice = 0;
                            }
                        }

                        targetAccount.Saved = true;
                        signerAccount.Saved = true;
                        break;
                    case TransactionType.ChangeAccountInfo:
                        ChangeAccountInfoTransaction changeAccountInfoTransaction = (ChangeAccountInfoTransaction) t;
                        if ((changeAccountInfoTransaction.ChangeType & 1) == 1)
                        {
                            targetAccount.AccountInfo.AccountKey = changeAccountInfoTransaction.NewAccountKey;
                        }

                        if ((changeAccountInfoTransaction.ChangeType & 4) == 4)
                        {
                            targetAccount.AccountType = changeAccountInfoTransaction.NewType;
                        }

                        if ((changeAccountInfoTransaction.ChangeType & 2) == 2)
                        {
                            targetAccount.Name = changeAccountInfoTransaction.NewName;
                        }

                        signerAccount.Balance -= changeAccountInfoTransaction.Fee;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        if (t.Fee == 0) signerAccount.NumberOfOperations++;
                        targetAccount.Saved = true;
                        signerAccount.Saved = true;
                        break;
                    case TransactionType.ChangeKey:
                    case TransactionType.ChangeKeySigned:
                        ChangeKeyTransaction changeKeyTransaction = (ChangeKeyTransaction) t;
                        signerAccount.Balance -= changeKeyTransaction.Fee;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.AccountInfo.AccountKey = changeKeyTransaction.NewAccountKey;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        if (t.Fee == 0) signerAccount.NumberOfOperations++;
                        targetAccount.Saved = true;
                        signerAccount.Saved = true;
                        break;
                }
            }

            Current.Add(checkPointBlock);
            Accounts.AddRange(checkPointBlock.Accounts);
            if ((checkPointBlock.BlockNumber + 1) % 100 == 0)
            {
                SaveNext();
            }
        }
    }

 

    public static class AccountNumberExtensions
    {
        public static bool IsValid(this AccountNumber number)
        {
            if (CheckPoints.Accounts.Count(p => p.AccountNumber == number) != 1) return false;
            return true;
        }

        public static Account Account(this AccountNumber an)
        {
            if(!an.IsValid()) throw new InvalidCastException();
            return CheckPoints.Accounts[an];
        }
    }
}