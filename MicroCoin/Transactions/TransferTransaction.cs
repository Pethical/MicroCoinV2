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
    public sealed class TransferTransaction : Transaction
    {

        public ulong Amount { get; set; }

        public ulong AccountPrice { get; set; }

        public uint SellerAccount { get; set; }

        public ECKeyPair NewAccountKey { get; set; }

        public TransferTransaction(Stream stream)
        {
            LoadFromStream(stream);
        }

        public override void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(SignerAccount);
                bw.Write(NumberOfOperations);
                bw.Write(TargetAccount);
                bw.Write(Amount);
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                AccountKey.SaveToStream(s, false);
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
                Amount = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream, false);
                byte b = br.ReadByte();
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