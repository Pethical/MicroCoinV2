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

namespace MicroCoin.Transactions
{
    public enum TransactionType : uint
    {
        Transaction = 1, ChangeKey, RecoverFounds, ListAccountForSale,
        DeListAccountForSale, BuyAccount, ChangeKeySigned, ChangeAccountInfo
    };


    public abstract class Transaction
    {

        public uint SignerAccount { get; set; }

        public uint NumberOfOperations { get; set; }

        public uint TargetAccount { get; set; }

        public ByteString Payload { get; set; }

        public ECSig Signature { get; set; }

        public ECKeyPair AccountKey { get; set; }

        public ulong Fee { get; set; }

        public abstract void SaveToStream(Stream s);

        public abstract void LoadFromStream(Stream s);

	public TransactionType TransactionType{ get; set; }

        protected void ReadPayLoad(BinaryReader br)
        {
            ushort len = br.ReadUInt16();
            Payload = br.ReadBytes(len);
        }
    }
}