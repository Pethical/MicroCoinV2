//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// TransferTransaction.cs - Copyright (c) 2018 Németh Péter
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


using MicroCoin.Cryptography;
using MicroCoin.Util;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MicroCoin.Transactions
{
    public sealed class TransferTransaction : Transaction
    {
        public enum TransferType : byte { Transaction, TransactionAndBuyAccount, BuyAccount };
        public ulong Amount { get; set; }
        public ulong AccountPrice { get; set; }
        public AccountNumber SellerAccount { get; set; }
        public ECKeyPair NewAccountKey { get; set; }
        public TransferType TransactionStyle { get; set; }
        public TransferTransaction(Stream stream)
        {
            LoadFromStream(stream);
        }
        public TransferTransaction()
        {

        }

        public ECSig GetSignature()
        {
            ECParameters parameters = new ECParameters();
            ECCurve curve = ECCurve.CreateFromFriendlyName("secp256k1");
            parameters.Curve = curve;
            parameters.Q.X = AccountKey.PublicKey.X;
            parameters.Q.Y = AccountKey.PublicKey.Y;
            byte[] D = AccountKey.D;
            if (D[0] == 0)
            {
                D = D.Skip(1).ToArray();
            }
            parameters.D = D;
            var eC = ECDsa.Create(parameters);
            Hash sign = eC.SignHash(GetHash());            
            ECSig ecs = new ECSig(sign);
            return ecs;
        }

        override public byte[] GetHash()
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(SignerAccount);
                    bw.Write(NumberOfOperations);
                    bw.Write(TargetAccount);
                    bw.Write(Amount);
                    bw.Write(Fee);
                    if (Payload != null && Payload.Length > 0) bw.Write((byte[])Payload);
                    if (AccountKey != null && AccountKey.X != null && AccountKey.Y != null
                        && AccountKey.X.Length>0 && AccountKey.Y.Length > 0)
                    {
                        bw.Write((ushort)AccountKey.CurveType);
                        bw.Write((byte[])AccountKey.X);
                        bw.Write((byte[])AccountKey.Y);
                    }
                    else
                    {
                        bw.Write((ushort)0);
                    }
                    ms.Position = 0;
                    return ms.ToArray();
                    /*
                    using(MemoryStream ms2 = new MemoryStream())
                    {
                        using (BinaryWriter bw2 = new BinaryWriter(ms2))
                        {
                            bw2.Write((ushort)ms.Length);
                            ms.CopyTo(ms2);
                            ms2.Position = 0;
                            return ms2.ToArray();
                        }
                    }*/

                }
            }
        }

        public override void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write((uint)SignerAccount);
                bw.Write(NumberOfOperations);
                bw.Write((uint)TargetAccount);
                bw.Write(Amount);
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                AccountKey.SaveToStream(s, false);
                if(TransactionStyle == TransferType.BuyAccount || TransactionStyle == TransferType.TransactionAndBuyAccount)
                {
                    bw.Write((byte)TransactionStyle);
                    bw.Write(AccountPrice);
                    bw.Write((uint)SellerAccount);
                    NewAccountKey.SaveToStream(s, false);
                }
                Signature.SaveToStream(s);
            }
        }

        public override void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                SignerAccount = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                if (TargetAccount == 530)
                {
                    TargetAccount.ToString();
                }
                Amount = br.ReadUInt64();
                Fee = br.ReadUInt64();
                Payload = ByteString.ReadFromStream(br);
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream, false);
                byte b = br.ReadByte();
                TransactionStyle = (TransferType)b;
                if (b > 2) { stream.Position -= 1; }
                if (b > 0 && b < 3)
                {
                    AccountPrice = br.ReadUInt64();
                    SellerAccount = br.ReadUInt32();
                    NewAccountKey = new ECKeyPair();
                    NewAccountKey.LoadFromStream(stream, false);
                }
                Signature = new ECSig(stream);
            }

        }
    }
}