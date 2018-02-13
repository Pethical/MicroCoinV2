using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.BlockChain
{

    class Block
    {
        public uint BlockNumber { get; set; }
        public byte[] AccountKey { get; set; }
        public UInt64 Reward { get; set; }
        public UInt64 Fee { get; set; }
        public ushort ProtocolVersion { get; set; }
        public ushort AvailableProtocolVersion { get; set; }
        public uint Timestamp { get; set; }
        public uint CompactTarget { get; set; }
        public uint Nonce { get; set; }
        public byte[] PreviusSnaphotHash { get; set; }
        public byte[] MerkleTreeHash{ get; set; }
        public byte[] ProofOfWork { get; set; }
        public Block(Stream stream)
        {
            using(BinaryReader br = new BinaryReader(stream))
            {              
            }
        }
    }
}
