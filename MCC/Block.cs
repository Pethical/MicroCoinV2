using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCC
{
     

    class BlockStreamHeader
    {
        public uint BlockNumber { get; set; }
        public UInt64 Offset { get; set; }
        public uint Size { get; set; }
    }

    class BlockStream
    {
        public List<BlockStreamHeader> Header { get; set; }
        public List<Block> Blocks { get; set; }
        public BlockStream(Stream stream)
        {
            Header = new List<BlockStreamHeader>();
            Blocks = new List<Block>();
            using (BinaryReader br = new BinaryReader(stream))
            {
                for(int i = 0; i < 1000; i++)
                {
                    Header.Add(new BlockStreamHeader
                    {
                        BlockNumber = br.ReadUInt32(),
                        Offset = br.ReadUInt64(),
                        Size = br.ReadUInt32()
                    });
                }
                foreach(BlockStreamHeader header in Header)
                {
                    
                }
            }
        }
    }

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
