/*************************************************************************
 * Copyright (c) 2018 Peter Nemeth
 *
 * This file is part of MicroCoin.
 *
 * MicroCoin is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MicroCoin is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *************************************************************************/

using MicroCoin.BlockChain;
using MicroCoin.Cryptography;
using MicroCoin.Util;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public class ChangeKeyTransaction : Transaction
    {
        public ECKeyPair NewAccountKey { get; set; }
        public TransactionType TransactionType { get; set; }
        public ChangeKeyTransaction() { }
        public ChangeKeyTransaction(Stream s, TransactionType TransactionType)
        {
            this.TransactionType = TransactionType;
            LoadFromStream(s);
        }

        public override void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(SignerAccount);
                if (TransactionType == TransactionType.ChangeKeySigned)
                {
                    bw.Write(TargetAccount);
                }
                bw.Write(NumberOfOperations);
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                AccountKey.SaveToStream(s, false);
                NewAccountKey.SaveToStream(s);
                Signature.SaveToStream(s);
            }
        }

        public override void LoadFromStream(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                if (TransactionType == TransactionType.ChangeKey)
                {
                    TargetAccount = SignerAccount;
                }
                else if (TransactionType == TransactionType.ChangeKeySigned)
                {
                    TargetAccount = br.ReadUInt32();
                }
                NumberOfOperations = br.ReadUInt32();
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(s, false);
                NewAccountKey = new ECKeyPair();
                NewAccountKey.LoadFromStream(s, true);
                Signature = new ECSig(s);
            }

        }
    }
}