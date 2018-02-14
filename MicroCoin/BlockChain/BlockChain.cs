// This file is part of MicroCoin.
// 
// Copyright (c) 2018 Peter Nemeth
//
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.


using System.Collections.Generic;
using System.IO;

namespace MicroCoin.BlockChain
{
    public class BlockChain : List<TransactionBlock>
    {
        private static BlockChain _sInstance;

        protected BlockChain() { }

        public static BlockChain Instance
        {
            get { return _sInstance ?? (_sInstance = new BlockChain()); }
        }

        public void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length - 1)
            {
                Add(new TransactionBlock(s));
            }
        }
    }
}
