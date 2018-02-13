﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.BlockChain
{

    class Block
    {
        
        /// <summary>
        /// Block Number in blockchain
        /// </summary>
        public uint BlockNumber { get; set; }
        /// <summary>
        /// Account key for this block
        /// </summary>
        public byte[] AccountKey { get; set; }
        /// <summary>
        /// Miner reward
        /// </summary>
        public UInt64 Reward { get; set; }
        /// <summary>
        /// Transaction Fees
        /// </summary>
        public UInt64 Fee { get; set; }
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
        public uint Timestamp { get; set; }
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
        /// Merkle Tree hash for checkpointing
        /// </summary>
        public byte[] MerkleTreeHash{ get; set; }
        /// <summary>
        /// Proof of work
        /// </summary>
        public byte[] ProofOfWork { get; set; }
        public Block(Stream stream)
        {
            using(BinaryReader br = new BinaryReader(stream))
            {              
            }
        }
    }
}
