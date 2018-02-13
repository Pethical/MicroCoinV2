using System.Collections.Generic;
using System.IO;

namespace MicroCoin.BlockChain
{
    public class BlockChain : List<TransactionBlock>
    {
        public void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length-1)
            {
                this.Add(new TransactionBlock(s));
            }
        }
    }
}
