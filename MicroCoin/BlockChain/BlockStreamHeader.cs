using System;

namespace MicroCoin.BlockChain
{
    public class BlockStreamHeader
    {
        public uint BlockNumber { get; set; }
        public UInt64 Offset { get; set; }
        public uint Size { get; set; }
    }
}
