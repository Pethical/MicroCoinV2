using log4net;
using MicroCoin.Transactions;
using MicroCoin.Util;
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

        private static uint[] offsets;
        public static ulong WorkSum { get; set; } = 0;

        public static void Init()
        {
            FileStream fs = File.Open(checkPointIndexName, FileMode.Open);
            offsets = new uint[ fs.Length/4 ];
            using(BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (fs.Position < fs.Length)
                {
                    offsets[i] = br.ReadUInt32();
                    i++;
                }
            }
            WorkSum = (ulong) ReadAll().Sum(p => p.CompactTarget);            
        }

        public static CheckPointBlock Get(uint i)
        {
            
            uint position = offsets[i];
            using (FileStream fs = File.OpenRead(checkPointFileName))
            {
                fs.Position = position;
                return new CheckPointBlock(fs);
            }            
        }

        public static Account Account(int i)
        {
            uint position = offsets[i/5];
            using (FileStream fs = File.OpenRead(checkPointFileName))
            {
                fs.Position = position;
                var block = new CheckPointBlock(fs);
                return block.Accounts.FirstOrDefault(p => p.AccountNumber == i);
            }
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
            List<CheckPointBlock> list = new List<CheckPointBlock>(offsets.Length);
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



    }
}
