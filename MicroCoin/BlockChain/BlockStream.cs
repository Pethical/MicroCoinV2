/*************************************************************************
 * Copyright (c) 2018 Peter Nemeth
 *
 * This file is part of MicroCoin.
 *
 * MicroCoin is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MicroCoin is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *************************************************************************/

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
