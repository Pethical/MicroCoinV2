//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// NewTransactionMessage.cs - Copyright (c) 2018 Németh Péter
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


using MicroCoin.Chain;
using MicroCoin.Transactions;
using MicroCoin.Util;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class NewTransactionMessage : MessageHeader
    {
        
        public uint TransactionCount { get; set; }
        private TransactionType[] TransactionTypes { get; set; }
        public ITransaction[] Transactions { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public NewTransactionMessage(Transaction[] transactions)
        {
            Transactions = transactions;
        }
        public NewTransactionMessage() : base()
        {
        }

        public NewTransactionMessage(Stream stream, MessageHeader rp) : base(rp)
        {
            using(BinaryReader br = new BinaryReader(stream))
            {
                TransactionCount = br.ReadUInt32();
                Transactions = new Transaction[TransactionCount];
                TransactionTypes = new TransactionType[TransactionCount];
                for (int i = 0; i < TransactionCount; i++)
                {
                    TransactionTypes[i] = (TransactionType)br.ReadByte();
                    switch (TransactionTypes[i])
                    {
                        case TransactionType.Transaction:
                        case TransactionType.BuyAccount:
                            TransferTransaction t = new TransferTransaction(stream);
                            Transactions[i] = t;
                            break;
                        case TransactionType.ChangeKey:
                        case TransactionType.ChangeKeySigned:
                            Transactions[i] = new ChangeKeyTransaction(stream, TransactionTypes[i]);
                            break;
                        case TransactionType.ListAccountForSale:
                        case TransactionType.DeListAccountForSale:
                            Transactions[i] = new ListAccountTransaction(stream);
                            break;
                        case TransactionType.ChangeAccountInfo:
                            Transactions[i] = new ChangeAccountInfoTransaction(stream);
                            break;
                        default:
                            stream.Position = stream.Length;
                            return;
                    }
                }
            }

        }

        override public void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                using (BinaryWriter bw = new BinaryWriter(memoryStream))
                {
                    bw.Write((uint)Transactions.Length);                    
                    foreach (var t in Transactions)
                    {
                        bw.Write((byte)t.TransactionType);
                        t.SaveToStream(memoryStream);
                    }
                    using (BinaryWriter bw2 = new BinaryWriter(s, Encoding.Default, true))
                    {
                        memoryStream.Position = 0;                                                
                        bw2.Write((int)memoryStream.Length);
                        memoryStream.CopyTo(s);
                        s.Position = 0;
                    }
                }
            }
            finally
            {
                memoryStream.Dispose();
                memoryStream = null;
            }
        }

        public Hash GetHash()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    foreach (var t in Transactions)
                    {
                        bw.Write(t.Fee);
                        if (t.Payload != null)
                        {
                            t.Payload.SaveToStream(bw);
                        }
                        bw.Write(t.SignerAccount);
                        bw.Write(t.TargetAccount);
                        t.Signature.SaveToStream(ms);
                        t.AccountKey.SaveToStream(ms);
                        bw.Write((uint)t.TransactionType);
                        bw.Write(Created.Hour);
                        bw.Write((int)(Created.Minute / 10));
                    }
                    System.Security.Cryptography.SHA256Managed sha = new System.Security.Cryptography.SHA256Managed();
                    ms.Position = 0;                    
                    return sha.ComputeHash(ms);
                }
            }
            finally
            {
                ms.Dispose();
            }
        }

        public T GetTransaction<T>(int i) where T: Transaction
        {
            return (T)Transactions[i];
        }

    }
}