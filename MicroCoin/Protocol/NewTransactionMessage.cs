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


using MicroCoin.Chain;
using MicroCoin.Transactions;
using System.IO;

namespace MicroCoin.Protocol
{
    public class NewTransactionMessage : MessageHeader
    {
        public uint TransactionCount { get; set; }
        public TransactionType[] TransactionTypes { get; set; }
        private object[] transactions;
        protected Stream stream;

        public NewTransactionMessage(Stream stream, MessageHeader rp) : base(rp)
        {
            using(BinaryReader br = new BinaryReader(stream))
            {
                TransactionCount = br.ReadUInt32();
                transactions = new object[TransactionCount];
                TransactionTypes = new TransactionType[TransactionCount];
                for (int i = 0; i < TransactionCount; i++)
                {
                    TransactionTypes[i] = (TransactionType)br.ReadByte();
                    
                    switch (TransactionTypes[i])
                    {
                        case TransactionType.Transaction:
                        case TransactionType.BuyAccount:
                            transactions[i] = new TransferTransaction(stream);
                            break;
                        case TransactionType.ChangeKey:
                        case TransactionType.ChangeKeySigned:
                            transactions[i] = new ChangeKeyTransaction(stream, TransactionTypes[i]);
                            break;
                        case TransactionType.ListAccountForSale:
                        case TransactionType.DeListAccountForSale:
                            transactions[i] = new ListAccountTransaction(stream);
                            break;
                        case TransactionType.ChangeAccountInfo:
                            transactions[i] = new ChangeAccountInfoTransaction(stream);
                            break;
                        default:
                            stream.Position = stream.Length;
                            return;
                    }
                }
            }
        }

        public T GetTransaction<T>(int i) where T: Transaction
        {
            return (T)transactions[i];
        }

    }
}