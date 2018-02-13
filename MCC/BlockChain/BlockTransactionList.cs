using MicroCoin.Transactions;
using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.BlockChain
{
    public class BlockTransactionList : TransactionBlock
    {
        public uint OpCount { get; set; }
        public TransactionType OpType { get; set; }
        public List<Transaction> Transactions;
        public BlockTransactionList(Stream s) : base(s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                OpCount = br.ReadUInt32();
                if (OpCount > 0)
                {
                    Transactions = new List<Transaction>();
                    for (int i = 0; i < OpCount; i++)
                    {
                        OpType = (TransactionType)br.ReadUInt32();
                        if (OpType == TransactionType.Transaction || OpType == TransactionType.BuyAccount)
                        {
                            try
                            {
                                TransferTransaction t = new TransferTransaction(s);
                                Transactions.Add(t);
                                if (t.NewAccountKey != null)
                                    Console.WriteLine("Block {0}, new Transaction {1} => {2} {3}", BlockNumber, t.SignerAccount, t.TargetAccount, t.Amount);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(OpType);
                                Console.WriteLine(BlockNumber);
                                throw;
                            }
                        }
                        else if (OpType == TransactionType.ChangeKey || OpType == TransactionType.ChangeKeySigned)
                        {
                            ChangeKeyTransaction ct = new ChangeKeyTransaction(s, OpType);
                            //			    Console.WriteLine("Change key Signer: {0} Target: {1} ", ct.SignerAccount, ct.TargetAccount);
                        }
                        else if(OpType==TransactionType.ListAccountForSale || OpType == TransactionType.DeListAccountForSale)
                        {
                            ListAccountTransaction t = new ListAccountTransaction(s);
                        }
                        else if(OpType == TransactionType.ChangeAccountInfo)
                        {
                            Console.WriteLine(BlockNumber);
                            ChangeAccountInfoTransaction tr = new ChangeAccountInfoTransaction(s);
                            Console.WriteLine("Account info {0} {1} {2}", tr.ChangeType, tr.SignerAccount, tr.NewName.ToAnsiString());
                        }
                        else
                        {
                            Console.WriteLine(OpType);
                            s.Position = s.Length;
                            return;
                        }
                    }
                }
            }
        }
        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write((uint)Transactions.Count);
                foreach (var t in Transactions)
                {
                    t.SaveToStream(s);
                }
            }
        }

    }
}