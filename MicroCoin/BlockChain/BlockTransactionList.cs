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
                        Transaction t = null;
                        switch (TransactionsType)
                        {
                            case TransactionType.Transaction:
                            case TransactionType.BuyAccount:
                                t = new TransferTransaction(s);
                                break;
                            case TransactionType.ChangeKey:
                            case TransactionType.ChangeKeySigned:
                                t = new ChangeKeyTransaction(s, TransactionsType);
                                break;
                            case TransactionType.ListAccountForSale:
                            case TransactionType.DeListAccountForSale:
                                t = new ListAccountTransaction(s);
                                break;
                            case TransactionType.ChangeAccountInfo:
                                t = new ChangeAccountInfoTransaction(s);
                                break;
                            default:
                                s.Position = s.Length;
                                return;
                        }
                        Transactions.Add(t);
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