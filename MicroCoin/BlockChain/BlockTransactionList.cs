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
        public uint TransactionCount { get; set; }
        public TransactionType TransactionsType { get; set; }
        public List<Transaction> Transactions;
        public BlockTransactionList(Stream s) : base(s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                TransactionCount = br.ReadUInt32();
                if (TransactionCount > 0)
                {
                    Transactions = new List<Transaction>();
                    for (int i = 0; i < TransactionCount; i++)
                    {
                        TransactionsType = (TransactionType)br.ReadUInt32();
                        if (TransactionsType == TransactionType.Transaction || TransactionsType == TransactionType.BuyAccount)
                        {
                            try
                            {
                                TransferTransaction t = new TransferTransaction(s);
                                Transactions.Add(t);
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }
                        else if (TransactionsType == TransactionType.ChangeKey || TransactionsType == TransactionType.ChangeKeySigned)
                        {
                            ChangeKeyTransaction ct = new ChangeKeyTransaction(s, TransactionsType);
                        }
                        else if(TransactionsType==TransactionType.ListAccountForSale || TransactionsType == TransactionType.DeListAccountForSale)
                        {
                            ListAccountTransaction t = new ListAccountTransaction(s);
                        }
                        else if(TransactionsType == TransactionType.ChangeAccountInfo)
                        {
                            ChangeAccountInfoTransaction tr = new ChangeAccountInfoTransaction(s);
                        }
                        else
                        {
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
                if (Transactions == null)
                {
                    bw.Write((uint)0);
                    return;
                }
                bw.Write((uint)Transactions.Count);
                foreach (var t in Transactions)
                {
                    t.SaveToStream(s);
                }
            }
        }

    }
}