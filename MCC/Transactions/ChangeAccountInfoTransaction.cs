﻿using MicroCoin.Cryptography;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public class ChangeAccountInfoTransaction : Transaction
    {
        public enum AccountInfoChangeType : byte { PublicKey = 1, AccountName = 2, AccountType = 3 };
        public byte ChangeType { get; set; }
        public ECKeyPair NewAccountKey { get; set; }
        public byte[] NewName { get; set; }
        public ushort NewType { get; set; }

        public ChangeAccountInfoTransaction() { }

        public ChangeAccountInfoTransaction(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream, false);
                ChangeType = br.ReadByte();
                NewAccountKey = new ECKeyPair();
                NewAccountKey.LoadFromStream(stream,false);
                ushort len = br.ReadUInt16();
                NewName = br.ReadBytes(len);
                NewType = br.ReadUInt16();
                Signature = new ECSig(stream);
            }
        }
        public override void SaveToStream(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}