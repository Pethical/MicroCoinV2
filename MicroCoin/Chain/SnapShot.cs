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


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MicroCoin.Chain
{
    public class Snapshot : IEnumerable<Block>, IEnumerator<Block>
    {
        private uint currentIndex;
        private Stream stream;
        public SnapshotHeader Header { get; set; }
        public List<Account> Accounts { get; set; }

        public Block Current => this[currentIndex];

        object IEnumerator.Current => this[currentIndex];
                
        public Block this[uint i]
        {
            get
            {
                long p = stream.Position;
                stream.Position = Header.BlockOffset(i);
                Block block = new Block(stream);
                stream.Position = p;
                return block;
            }
            set
            {
            }
        }
        public Snapshot()
        {
            
        }
        public Snapshot(Stream s)
        {
            stream = s;
            Header = new SnapshotHeader(s);
            Accounts = new List<Account>();
            for(uint i = 0; i < Header.BlockCount; i++)
            {
                Accounts.AddRange(this[i].Accounts);
            }
            
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            if (currentIndex + 1 < Header.BlockCount)
            {
                currentIndex++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            currentIndex = 0;
        }
    }
}