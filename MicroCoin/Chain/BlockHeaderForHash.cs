using System;
using MicroCoin.Util;

namespace MicroCoin.Chain
{
    public struct BlockHeaderForHash
    {
        public Hash Part1 { get; set; }
        public ByteString MinerPayload { get; set; }
        public Hash Part3 { get; set; }
        public Hash Join()
        {
            return Part1 + MinerPayload + Part3;
        }

        public Hash GetBlockHeaderHash(uint nonce, uint timestamp)
        {
            Hash s1 = $"{timestamp:X04}";
            Hash s2 = $"{nonce:X04}";
            Hash h = (byte[]) MinerPayload;
            return Part1 + h + Part3 + s1.Reverse() + s2.Reverse();
        }
    }
}