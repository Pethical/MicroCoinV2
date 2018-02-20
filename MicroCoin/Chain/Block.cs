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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Chain
{
    /// <summary>
    /// One entry in the snapshot
    /// </summary>
    public class Block
    {
        
        /// <summary>
        /// Block Number in blockchain
        /// </summary>
        public uint BlockNumber { get; set; }
        /// <summary>
        /// Account key for this block
        /// </summary>
        public ECKeyPair AccountKey { get; set; }
        /// <summary>
        /// Miner reward
        /// </summary>
        public ulong Reward { get; set; }
        /// <summary>
        /// Transaction Fees
        /// </summary>
        public ulong Fee { get; set; }
        /// <summary>
        /// Miner Protocol Version
        /// </summary>
        public ushort ProtocolVersion { get; set; }
        /// <summary>
        /// Miner Maximum Protocol Version
        /// </summary>
        public ushort AvailableProtocolVersion { get; set; }
        /// <summary>
        /// When the Block created
        /// </summary>
        public Timestamp Timestamp { get; set; }
        /// <summary>
        /// Compact Target (Difficulty)
        /// </summary>
        public uint CompactTarget { get; set; }
        /// <summary>
        /// Found Nonce value
        /// </summary>
        public uint Nonce { get; set; }
        /// <summary>
        /// Previus snapshot hash
        /// </summary>
        public byte[] PreviusSnaphotHash { get; set; }
        /// <summary>
        /// Gets or sets the block payload.
        /// </summary>
        /// <value>The block payload.</value>
        public byte[] BlockPayload { get; set; }
        /// <summary>
        /// Merkle Tree hash for checkpointing
        /// </summary>
        public byte[] MerkleTreeHash{ get; set; }
        /// <summary>
        /// Proof of work
        /// </summary>
        public byte[] ProofOfWork { get; set; }
        /// <summary>
        /// List of all accounts
        /// </summary>
        /// <value>The accounts.</value>
        public List<Account> Accounts { get; set; } = new List<Account>();
        /// <summary>
        /// The block hash
        /// </summary>
        /// <value>The block hash.</value>
        public ByteString BlockHash { get; set; }
        /// <summary>
        /// Gets or sets the accumulated work.
        /// </summary>
        /// <value>The accumulated work.</value>
        public ulong AccumulatedWork { get; set; }

        public void SaveToStream(Stream stream)
        {
            using(BinaryWriter bw = new BinaryWriter(stream, Encoding.Default, true))
            {
                SaveToStream(bw);
            }
        }

        public void SaveToStream(BinaryWriter bw)
        {
                long s = bw.BaseStream.Position;
                bw.Write(BlockNumber);
                AccountKey.SaveToStream(bw.BaseStream, false);
                bw.Write(Reward);
                bw.Write(Fee);
                bw.Write(ProtocolVersion);
                bw.Write(AvailableProtocolVersion);
                bw.Write(Timestamp);
                bw.Write(CompactTarget);
                bw.Write(Nonce);
                BlockPayload.SaveToStream(bw);
                PreviusSnaphotHash.SaveToStream(bw);
                MerkleTreeHash.SaveToStream(bw);
                ProofOfWork.SaveToStream(bw);
                for(int i=0;i<5;i++)
                {
                    Accounts[i].SaveToStream(bw);
                }
                BlockHash.SaveToStream(bw);
                bw.Write(AccumulatedWork);
        }

        public Block(Stream stream)
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                BlockNumber = br.ReadUInt32();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream, false);
                Reward = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ProtocolVersion = br.ReadUInt16();
                AvailableProtocolVersion = br.ReadUInt16();
                Timestamp = br.ReadUInt32();
                DateTime t = Timestamp;
                CompactTarget = br.ReadUInt32();
                Nonce = br.ReadUInt32();
                ushort len = br.ReadUInt16();
                BlockPayload = br.ReadBytes(len);
                len = br.ReadUInt16();
                PreviusSnaphotHash = br.ReadBytes(len);
                len = br.ReadUInt16();
                MerkleTreeHash = br.ReadBytes(len);
                len = br.ReadUInt16();
                ProofOfWork = br.ReadBytes(len);
                for(int i = 0; i < 5; i++)
                {
                    Account acc = new Account(stream);
                    Accounts.Add(acc);
                    string name = acc.Name;
                }
                BlockHash = ByteString.ReadFromStream(br);
                AccumulatedWork = br.ReadUInt64();
            }
        }
    }
}
