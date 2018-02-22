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


using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroCoin.Chain
{
    public class Snapshot : IEnumerable<Block>, IEnumerator<Block>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object loadLock = new object();
        private uint currentIndex = 0;
        private Stream stream;
        public ulong WorkSum { get; set; }
        public SnapshotHeader Header { get; set; }

        public uint BlockCount
        {
            get
            {
                if (Header == null) return 0;
                return Header.BlockCount;
            }
        }
        public List<Account> Accounts { get; set; } = new List<Account>();

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
            LoadFromStream(s);
        }

        public void LoadFromFile(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite);
            if (stream != null) stream.Dispose();
            stream = null;
            stream = fs;
            Accounts.Clear();
            Accounts = null;
            GC.Collect();
            Header = new SnapshotHeader(stream);
            Accounts = new List<Account>();
            WorkSum = 0;
            for (uint i = 0; i < Header.EndBlock - Header.StartBlock; i++)
            {
                WorkSum += this[i].CompactTarget;
                foreach (Account a in this[i].Accounts)
                {
                    
                    Accounts.Add(new Account
                    {
                        AccountNumber = a.AccountNumber,
                        AccountType = a.AccountType,
                        Name = a.Name,
                        Balance = a.Balance,
                        NumberOfOperations = a.NumberOfOperations,
                        BlockNumber = i
                    });
                    // Accounts.AddRange(b.Accounts);
                }
            }
        }

        public void LoadFromStream(Stream s)
        {
            if (stream != null) stream.Dispose();
            stream = null;
            stream = new MemoryStream();
            s.CopyTo(stream);
            // stream = s;
            stream.Position = 0;
            Header = new SnapshotHeader(stream);
            Accounts = new List<Account>();
            WorkSum = 0;
            for (uint i = 0; i < Header.EndBlock - Header.StartBlock; i++)
            {
                var b = this[i];
                WorkSum += b.CompactTarget;
                Accounts.AddRange(b.Accounts);                
            }
            log.Info(WorkSum);
        }

        public void Append(Snapshot snapshot)
        {
            lock (loadLock)
            {
                List<Block> list = new List<Block>();
                if (Header != null)
                {
                    for (uint i = Header.StartBlock; i < Header.EndBlock+1; i++)
                    {
                        list.Add(this[i]);
                    }
                    if (Header.BlockCount > snapshot.Header.BlockCount)
                    {
                        return;
                    }
                    Header.BlockCount = snapshot.Header.BlockCount;
                    Header.Hash = snapshot.Header.Hash;
                    Header.EndBlock = snapshot.Header.EndBlock;
                }
                else
                {
                    Header = snapshot.Header;
                }
                for (uint i = 0; i != snapshot.Header.EndBlock- snapshot.Header.StartBlock + 1; i++)
                    list.Add(snapshot[i]);
                MemoryStream ms = new MemoryStream();
                uint[] h = Header.offsets;
                Header.offsets = new uint[list.Count + 1];
                Header.SaveToStream(ms);
                long headerSize = ms.Position;
                ms.Position = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    uint i = 0;
                    foreach (Block b in list)
                    {
                        Header.offsets[i] = (uint)(ms.Position + headerSize);
                        b.SaveToStream(ms);
                        i++;
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                Header.SaveToStream(memoryStream);
                ms.Position = 0;
                ms.CopyTo(memoryStream);
                memoryStream.Write(Header.Hash, 0, Header.Hash.Length);
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                memoryStream.Position = 0;
                LoadFromStream(memoryStream);
                ms.Dispose();
                memoryStream.Dispose();
                ms = null;
                memoryStream = null;
                GC.Collect();
            }
        }

        public void SaveToStream(Stream s)
        {
            long pos = stream.Position;
            stream.Position = 0;
            stream.CopyTo(s);
            stream.Position = pos;
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
            if (stream != null)
            {
                stream.Dispose();                
                Accounts.Clear();
                Accounts = null;
                stream = null;
            }
        }

        public bool MoveNext()
        {
            if (stream == null) return false;
            if (currentIndex + 1 < Header.EndBlock - Header.StartBlock)
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

        public Snapshot SaveChunk(uint startBlock, uint endBlock)
        {
            lock (loadLock)
            {
                List<Block> list = new List<Block>();
                SnapshotHeader Header = new SnapshotHeader();
                Header.StartBlock = startBlock;
                Header.EndBlock = endBlock;
                Header.BlockCount = endBlock - startBlock + 1;
                for (uint i = startBlock; i <= endBlock; i++)
                    list.Add(this[i]);
                MemoryStream ms = new MemoryStream();
                Header.offsets = new uint[list.Count + 1];
                Header.SaveToStream(ms);
                long headerSize = ms.Position;
                ms.Position = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    uint i = 0;
                    foreach (Block b in list)
                    {
                        Header.offsets[i] = (uint)(ms.Position + headerSize);
                        b.SaveToStream(ms);
                        i++;
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                Header.SaveToStream(memoryStream);
                ms.Position = 0;
                ms.CopyTo(memoryStream);
                memoryStream.Write(Header.Hash, 0, Header.Hash.Length);
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                memoryStream.Position = 0;
                Snapshot snapshot = new Snapshot(memoryStream);
                ms.Dispose();
                memoryStream.Dispose();
                ms = null;
                memoryStream = null;
                GC.Collect();
                return snapshot;
            }
        }

    }
}