using System.Collections.Generic;
using System.IO;

namespace MCC
{
    public class BlockChain : List<OperationBlock>
    {
        public void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length-1)
            {
                this.Add(new OperationBlock(s));
            }
        }
    }
}
