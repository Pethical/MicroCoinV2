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


using MicroCoin.Cryptography;
using MicroCoin.Util;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public sealed class ListAccountTransaction : Transaction
    {
        public enum AccountTransactionType : ushort
        {
            ListAccount = 4,
            DeListAccount = 5
        };

        public AccountTransactionType TransactionType { get; set; }

        public ulong AccountPrice { get; set; }

        public uint AccountToPay { get; set; }

        public ECKeyPair NewPublicKey { get; set; }

        public uint LockedUntilBlock { get; set; }

        public ListAccountTransaction() { }

        public ListAccountTransaction(Stream stream) {
            LoadFromStream(stream);
        }

        public override void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(SignerAccount);
                bw.Write(TargetAccount);
                bw.Write((ushort)TransactionType);
                bw.Write(NumberOfOperations);
                if (TransactionType == AccountTransactionType.ListAccount)
                {
                    bw.Write(AccountPrice);
                    bw.Write(AccountToPay);
                    AccountKey.SaveToStream(s, false);
                    NewPublicKey.SaveToStream(s);
                    bw.Write(LockedUntilBlock);
                }
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                Signature.SaveToStream(s);
            }

        }

        public override void LoadFromStream(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                TransactionType = (AccountTransactionType)br.ReadUInt16();
                NumberOfOperations = br.ReadUInt32();
                if (TransactionType == AccountTransactionType.ListAccount)
                {
                    AccountPrice = br.ReadUInt64();
                    AccountToPay = br.ReadUInt32();
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(s, false);
                    NewPublicKey = new ECKeyPair();
                    NewPublicKey.LoadFromStream(s);
                    LockedUntilBlock = br.ReadUInt32();
                }
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                Signature = new ECSig(s);
            }
        }
    }
}