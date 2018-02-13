using System.Collections.Generic;
using System.IO;

namespace MicroCoin.BlockChain
{
    public class BlockChain : List<TransactionBlock>
    {
        private static BlockChain s_instance;

        protected BlockChain() { }

        public static BlockChain Instance
        {
            get
            {
                if (s_instance == null) s_instance = new BlockChain();
                return s_instance;
            }
        }

        public void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length - 1)
            {
                this.Add(new TransactionBlock(s));
            }
        }
    }
}
