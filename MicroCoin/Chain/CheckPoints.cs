using log4net;
using MicroCoin.Transactions;
using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Chain
{
    public class CheckPoints
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string checkPointIndexName = "checkpoints.idx";
        public const string checkPointFileName = "checkpoints.mcc";

        private static List<uint> offsets = new List<uint>();
        public static ulong WorkSum { get; set; } = 0;
        public static List<Account> Accounts { get; set; } = new List<Account>();
        public static List<CheckPointBlock> Current { get; set; } = new List<CheckPointBlock>();
        public static void Init()
        {
            WorkSum = 0;
            Current = new List<CheckPointBlock>();
            if (!File.Exists((checkPointFileName))) return;
            try
            {
                FileStream fs = File.Open(checkPointIndexName, FileMode.Open);
                offsets = new List<uint>((int)(fs.Length / 4));
                Accounts = new List<Chain.Account>();                
                using (BinaryReader br = new BinaryReader(fs))
                {                    
                    while (fs.Position < fs.Length)
                    {
                        offsets.Add(br.ReadUInt32());                 
                    }
                }
                using (FileStream cf = File.OpenRead(checkPointFileName))
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
            catch { }
            log.Info($"Accounts: {Accounts.Last().AccountNumber}");
        }

        public static void Put(CheckPointBlock cb)
        {
            using (FileStream fs = File.OpenWrite(checkPointFileName))
            {
                uint position = 0;
                if (offsets.Count <= cb.BlockNumber)
                {
                    position = (uint)fs.Length;
                    offsets.Add(position);
                }
                else
                {
                    position = offsets[(int)cb.BlockNumber];
                }
                fs.Position = position;
                cb.SaveToStream(fs);
            }
        }

        public static CheckPointBlock Get(uint i)
        {
            return Current[(int)i];
        }

        public static Account Account(int i)
        {
            return Accounts[i];
        }

        public static void SaveList(List<CheckPointBlock> list, Stream stream, Stream indexStream)
        {
            List<uint> offsets = new List<uint>();
            foreach (var item in list)
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

        public static List<CheckPointBlock> ReadAll()
        {
            List<CheckPointBlock> list = new List<CheckPointBlock>(offsets.Count);
            using (FileStream fs = File.OpenRead(checkPointFileName))
            {
                while (fs.Position < fs.Length)
                {
                    list.Add(new CheckPointBlock(fs));
                }
            }
            return list;
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
            List<CheckPointBlock> checkPoint = new List<CheckPointBlock>(blockChain.BlockHeight() + 1);
            uint accNumber = 0;
            ulong accWork = 0;
            for (int block = 0; block < 100 * ((blockChain.GetLastBlock().BlockNumber + 1) / 100); block++)
            {
                if (block % 1000 == 0)
                {
                    log.Info($"Building checkpont: {block} block");
                }
                Block b = blockChain.Get(block);
                CheckPointBlock checkPointBlock = new CheckPointBlock();
                checkPointBlock.AccountKey = b.AccountKey;
                for (int i = 0; i < 5; i++)
                {
                    checkPointBlock.Accounts.Add(new Account
                    {
                        AccountNumber = accNumber,
                        Balance = (i == 0 ? 1000000ul + b.Fee : 0ul),
                        BlockNumber = b.BlockNumber,
                        UpdatedBlock = b.BlockNumber,
                        NumberOfOperations = 0,
                        AccountType = 0,
                        Name = "",
                        UpdatedByBlock = b.BlockNumber,
                        AccountInfo = new AccountInfo
                        {
                            AccountKey = b.AccountKey,
                            State = AccountInfo.AccountState.Normal,
                        }
                    });
                    accNumber++;
                }
                accWork += b.CompactTarget;
                checkPointBlock.AccumulatedWork = accWork;
                checkPointBlock.AvailableProtocol = b.AvailableProtocol;
                checkPointBlock.BlockNumber = b.BlockNumber;
                checkPointBlock.BlockSignature = 2;//b.BlockSignature;
                checkPointBlock.CheckPointHash = b.CheckPointHash;
                checkPointBlock.CompactTarget = b.CompactTarget;
                checkPointBlock.Fee = b.Fee;
                checkPointBlock.Nonce = b.Nonce;
                checkPointBlock.Payload = b.Payload == null ? new ByteString(new byte[0]) : b.Payload;
                checkPointBlock.ProofOfWork = b.ProofOfWork;
                checkPointBlock.ProtocolVersion = b.ProtocolVersion;
                checkPointBlock.Reward = b.Reward;
                checkPointBlock.Timestamp = b.Timestamp;
                checkPointBlock.TransactionHash = b.TransactionHash;
                WorkSum += b.CompactTarget;
                foreach (var t in b.Transactions)
                {
                    CheckPointBlock signer, target;
                    Account account;
                    signer = checkPoint.FirstOrDefault(p => p.Accounts.Count(a => a.AccountNumber == t.SignerAccount) > 0);
                    target = checkPoint.FirstOrDefault(p => p.Accounts.Count(a => a.AccountNumber == t.TargetAccount) > 0);
                    if (t.Fee != 0) signer.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                    switch (t.TransactionType)
                    {
                        case TransactionType.Transaction:
                            TransferTransaction transfer = (TransferTransaction)t;
                            if (signer != null && target != null)
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
                            if (signer != null && target != null)
                            {
                                if (listAccountTransaction.TransactionType == ListAccountTransaction.AccountTransactionType.ListAccount)
                                {
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
            foreach (var p in checkPoint) p.BlockHash = p.CalculateBlockHash();
            return checkPoint;
        }

        public static CheckPointBlock GetLastBlock()
        {
            return Current.Count > 0 ? Current.Last() : null;
        }

        protected static void SaveNext()
        {
            var offsets2 = new List<uint>();
            uint chunk = (Current.Last().BlockNumber / 100) % 10;
            log.Info($"Saving next checkpont => {chunk}");
            using (FileStream fs = File.Create(checkPointFileName + $".{chunk}"))
            {
                foreach (var block in Current)
                {                                     
                    offsets2.Add((uint)fs.Position);
                    block.SaveToStream(fs);
                }
                
            }
            using (FileStream fs = File.Create(checkPointIndexName + $".{chunk}"))
            {
                using(BinaryWriter bw = new BinaryWriter(fs))
                {
                    foreach (var o in offsets2) bw.Write(o);
                }
            }
            File.Copy(checkPointIndexName + $".{chunk}", checkPointIndexName, true);
            File.Copy(checkPointFileName + $".{chunk}", checkPointFileName, true);            
            log.Info("Saved next checkpont");
        }

        public static void AppendBlock(Block b)
        {
            int lastBlock = -1;
            if (GetLastBlock() != null)
            {
                if (b.BlockNumber <= GetLastBlock().BlockNumber) return;
                lastBlock = (int) GetLastBlock().BlockNumber;
            }
            CheckPointBlock checkPointBlock = new CheckPointBlock();
            checkPointBlock.AccountKey = b.AccountKey;
            uint accNumber = (uint) (lastBlock+1) * 5;
            if (accNumber == 0)
            {
                log.Info("NULL");
            }
            ulong accWork = WorkSum;
            for (int i = 0; i < 5; i++)
            {
                checkPointBlock.Accounts.Add(new Account
                {
                    AccountNumber = accNumber,
                    Balance = (i == 0 ? 1000000ul + b.Fee : 0ul),
                    BlockNumber = b.BlockNumber,
                    UpdatedBlock = b.BlockNumber,
                    NumberOfOperations = 0,
                    AccountType = 0,
                    Name = "",
                    UpdatedByBlock = b.BlockNumber,
                    AccountInfo = new AccountInfo
                    {
                        AccountKey = b.AccountKey,
                        State = AccountInfo.AccountState.Normal,
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
            checkPointBlock.Payload = b.Payload == null ? new ByteString(new byte[0]) : b.Payload;
            checkPointBlock.ProofOfWork = b.ProofOfWork;
            checkPointBlock.ProtocolVersion = b.ProtocolVersion;
            checkPointBlock.Reward = b.Reward;
            checkPointBlock.Timestamp = b.Timestamp;
            checkPointBlock.TransactionHash = b.TransactionHash;
            foreach (var t in b.Transactions)
            {
                CheckPointBlock signerBlock, targetBlock, sellerBlock;
                Account signerAccount = Accounts[(int)t.SignerAccount];
                Account targetAccount  = Accounts[(int)t.TargetAccount];
                signerBlock = Get(signerAccount.BlockNumber);
                targetBlock = Get(targetAccount.BlockNumber);
                if (t.Fee != 0) signerAccount.NumberOfOperations++;
                switch (t.TransactionType)
                {
                    case TransactionType.Transaction:
                        TransferTransaction transfer = (TransferTransaction)t;
                        if (signerBlock != null && targetBlock != null)
                        {
                            if (t.Fee == 0) signerAccount.NumberOfOperations++;
                            signerAccount.Balance -= (transfer.Fee + transfer.Amount);
                            signerAccount.UpdatedBlock = b.BlockNumber;
                            targetAccount.Balance += transfer.Amount;
                            targetAccount.UpdatedBlock = b.BlockNumber;
                        }
                        break;
                    case TransactionType.BuyAccount:
                        TransferTransaction transferTransaction = (TransferTransaction)t; // TODO: be kell fejezni
                        if (t.Fee == 0) signerBlock.Accounts.First(p => p.AccountNumber == t.SignerAccount).NumberOfOperations++;
                        signerBlock.Accounts.First(p => p.AccountNumber == t.SignerAccount).Balance -= (transferTransaction.Fee + transferTransaction.Amount);

                        Account sellerAccount = Accounts[(int)transferTransaction.SellerAccount];
                        sellerBlock = Get(sellerAccount.BlockNumber);
                        sellerAccount.Balance += transferTransaction.Amount;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        sellerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.AccountInfo.AccountKey = transferTransaction.NewAccountKey;
                        targetAccount.AccountInfo.Price = 0;
                        targetAccount.AccountInfo.LockedUntilBlock = 0;
                        targetAccount.AccountInfo.State = AccountInfo.AccountState.Normal;
                        targetAccount.AccountInfo.AccountToPayPrice = 0;
                        targetAccount.AccountInfo.NewPublicKey = null;
                        break;
                    case TransactionType.DeListAccountForSale:
                    case TransactionType.ListAccountForSale:
                        ListAccountTransaction listAccountTransaction = (ListAccountTransaction)t;                        
                        signerAccount.Balance -= listAccountTransaction.Fee;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        if (t.Fee == 0) signerAccount.NumberOfOperations++;
                        if (signerBlock != null && targetBlock != null)
                        {
                            if (listAccountTransaction.TransactionType == ListAccountTransaction.AccountTransactionType.ListAccount)
                            {
                                targetAccount.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                targetAccount.AccountInfo.LockedUntilBlock = listAccountTransaction.LockedUntilBlock;
                                targetAccount.AccountInfo.State = AccountInfo.AccountState.Sale;
                                targetAccount.AccountInfo.Price = listAccountTransaction.AccountPrice;
                                targetAccount.AccountInfo.NewPublicKey = listAccountTransaction.NewPublicKey;
                                targetAccount.AccountInfo.AccountToPayPrice = listAccountTransaction.AccountToPay;
                            }
                            else
                            {
                                targetAccount.AccountInfo.State = AccountInfo.AccountState.Normal;
                                targetAccount.AccountInfo.Price = 0;
                                targetAccount.AccountInfo.NewPublicKey = null;
                                targetAccount.AccountInfo.LockedUntilBlock = 0;
                                targetAccount.AccountInfo.AccountToPayPrice = 0;
                            }
                        }
                        break;
                    case TransactionType.ChangeAccountInfo:
                        ChangeAccountInfoTransaction changeAccountInfoTransaction = (ChangeAccountInfoTransaction)t;
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
                        break;
                    case TransactionType.ChangeKey:
                    case TransactionType.ChangeKeySigned:
                        ChangeKeyTransaction changeKeyTransaction = (ChangeKeyTransaction)t;
                        signerAccount.Balance -= changeKeyTransaction.Fee;
                        signerAccount.UpdatedBlock = b.BlockNumber;
                        targetAccount.AccountInfo.AccountKey = changeKeyTransaction.NewAccountKey;
                        targetAccount.UpdatedBlock = b.BlockNumber;
                        if (t.Fee == 0) signerAccount.NumberOfOperations++;
                        break;
                }
            }
            Current.Add(checkPointBlock);
            Accounts.AddRange(checkPointBlock.Accounts);
            if (checkPointBlock.BlockNumber == 99)
            {
                log.Info("9999");
            }
            if ((checkPointBlock.BlockNumber + 1) % 100 == 0)
            {
                SaveNext();
            }
        }
    }
}
