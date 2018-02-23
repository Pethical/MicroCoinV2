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
using System.Security.Cryptography;
using System.Text;

namespace MicroCoin.Chain
{
    /// <summary>
    /// One entry in the checkpoint
    /// </summary>
    public class CheckPointBlock : BlockBase
    {
        /// <summary>
        /// List of all accounts
        /// </summary>
        /// <value>The accounts.</value>
        public List<Account> Accounts { get; set; } = new List<Account>();
        /// <summary>
        /// The block hash
        /// </summary>
        /// <value>The block hash.</value>
        public Hash BlockHash { get; set; }
        /// <summary>
        /// Gets or sets the accumulated work.
        /// </summary>
        /// <value>The accumulated work.</value>
        public ulong AccumulatedWork { get; set; }

        public override void SaveToStream(Stream stream)
        {
            using(BinaryWriter bw = new BinaryWriter(stream, Encoding.Default, true))
            {
                SaveToStream(bw);
            }
        }

        public void SaveToStream(BinaryWriter bw, bool saveHash = true)
        {
            long s = bw.BaseStream.Position;
            bw.Write(BlockNumber);
            AccountKey.SaveToStream(bw.BaseStream, false);
            bw.Write(Reward);
            bw.Write(Fee);
            bw.Write(ProtocolVersion);
            bw.Write(AvailableProtocol);
            bw.Write(Timestamp);
            bw.Write(CompactTarget);
            bw.Write(Nonce);
            Payload.SaveToStream(bw);
            CheckPointHash.SaveToStream(bw);
            TransactionHash.SaveToStream(bw);
            ProofOfWork.SaveToStream(bw);
            for (int i = 0; i < 5; i++)
            {
                Accounts[i].SaveToStream(bw, saveHash);
            }
            if (saveHash)
            {
                BlockHash.SaveToStream(bw);
            }
            bw.Write(AccumulatedWork);
        }

        public CheckPointBlock() : base()
        {

        }

        public Hash CalculateBlockHash()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                SaveToStream(bw, false);
                ms.Position = 0;
                using (SHA256Managed sha = new SHA256Managed())
                {
                    return sha.ComputeHash(ms);
                }
            }
        }

        public CheckPointBlock(Stream stream) : base()
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                BlockNumber = br.ReadUInt32();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(stream, false);
                Reward = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ProtocolVersion = br.ReadUInt16();
                AvailableProtocol = br.ReadUInt16();
                Timestamp = br.ReadUInt32();
                DateTime t = Timestamp;
                CompactTarget = br.ReadUInt32();
                Nonce = br.ReadUInt32();
                ushort len = br.ReadUInt16();
                Payload = br.ReadBytes(len);
                len = br.ReadUInt16();
                CheckPointHash = br.ReadBytes(len);
                len = br.ReadUInt16();
                TransactionHash = br.ReadBytes(len);
                len = br.ReadUInt16();
                ProofOfWork = br.ReadBytes(len);
                for(int i = 0; i < 5; i++)
                {
                    Account acc = new Account(stream);
                    Accounts.Add(acc);
                }
                BlockHash = Hash.ReadFromStream(br);
                AccumulatedWork = br.ReadUInt64();
            }
//            Hash h = CalculateBlockHash();
//	    log.Info("Hash calculated");
        }
    }
}
