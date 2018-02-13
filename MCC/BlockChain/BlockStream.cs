using System.Collections.Generic;
using System.IO;

namespace MicroCoin.BlockChain
{
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
}
