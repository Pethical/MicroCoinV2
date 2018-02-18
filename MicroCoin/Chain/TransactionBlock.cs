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
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Chain
{
    public enum TransactionType : uint
    {
        Transaction = 1, ChangeKey, RecoverFounds, ListAccountForSale,
        DeListAccountForSale, BuyAccount, ChangeKeySigned, ChangeAccountInfo
    };

    
    // TPCOperationsComp
    public class TransactionBlock
    {

        public byte TransactionBlockSignature { get; set; } = 3;
        public ushort ProtocolVersion { get; set; }
        public ushort AvailableProtocol { get; set; }
        public uint BlockNumber { get; set; }
        public ECKeyPair AccountKey { get; set; } = new ECKeyPair();
        public UInt64 Reward { get; set; }
        public UInt64 Fee { get; set; }
        public uint Timestamp { get; set; }
        public uint CompactTarget { get; set; }
        public uint Nonce { get; set; }
        public ByteString Payload { get; set; }
        public byte[] SnapshotHash { get; set; }
        public byte[] OperationHash { get; set; }
        public byte[] ProofOfWork { get; set; }
        public static TransactionBlock NullBlock { get
            {
                return new TransactionBlock
                {
                    BlockNumber = 0
                };
            }
        }

        public TransactionBlock(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.ASCII, true))
            {
                TransactionBlockSignature = br.ReadByte();
                if (TransactionBlockSignature > 0)
                {
                    ProtocolVersion = br.ReadUInt16();
                    AvailableProtocol = br.ReadUInt16();
                }
                BlockNumber = br.ReadUInt32();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(s);
                Reward = br.ReadUInt64();
                Fee = br.ReadUInt64();
                Timestamp = br.ReadUInt32();
                CompactTarget = br.ReadUInt32();
                Nonce = br.ReadUInt32();
                ushort pl = br.ReadUInt16();
                if (pl > 0)
                {
                    Payload = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    SnapshotHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    OperationHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    ProofOfWork = br.ReadBytes(pl);
                }
            }
        }

        public TransactionBlock()
        {

        }

        public virtual void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(TransactionBlockSignature);
                bw.Write(ProtocolVersion);
                bw.Write(AvailableProtocol);
                bw.Write(BlockNumber);
                if (AccountKey == null)
                {
                    bw.Write((ushort)6);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                }
                else
                {
                    AccountKey.SaveToStream(s);
                }
                bw.Write(Reward);
                bw.Write(Fee);
                bw.Write(Timestamp);
                bw.Write(CompactTarget);
                bw.Write(Nonce);
                if (Payload != null)
                {
                    bw.Write((ushort)Payload.Length);
                    bw.Write((byte[])Payload);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (SnapshotHash != null)
                {
                    bw.Write((ushort)SnapshotHash.Length);
                    bw.Write(SnapshotHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (OperationHash != null)
                {
                    bw.Write((ushort)OperationHash.Length);
                    bw.Write(OperationHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (ProofOfWork != null)
                {
                    bw.Write((ushort)ProofOfWork.Length);
                    bw.Write(ProofOfWork);
                }
                else
                {
                    bw.Write((ushort)0);
                }
            }
        }

    }
}