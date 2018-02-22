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
    public class CheckPoint : IEnumerable<CheckPointBlock>, IEnumerator<CheckPointBlock>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object loadLock = new object();
        private uint currentIndex = 0;
        private Stream stream;
        public const string CheckPointFileName = "checkpoint";
        public ulong WorkSum { get; set; }
        public CheckPointHeader Header { get; set; }

        public uint BlockCount
        {
            get
            {
                if (Header == null) return 0;
                return Header.BlockCount;
            }
        }
        public List<Account> Accounts { get; set; } = new List<Account>();

        public CheckPointBlock Current => this[currentIndex];

        object IEnumerator.Current => this[currentIndex];

        public CheckPointBlock this[uint i]
        {
            get
            {
                long p = stream.Position;
                stream.Position = Header.BlockOffset(i);
                CheckPointBlock block = new CheckPointBlock(stream);
                stream.Position = p;
                return block;
            }
            set
            {
            }
        }

        public CheckPoint()
        {

        }

        public CheckPoint(Stream s)
        {
            LoadFromStream(s);
        }
        /*
        public static CheckPoint BuildFromBlockChain(BlockChain blockChain)
        {
            CheckPoint checkPoint = new CheckPoint();
            foreach(var b in blockChain)
            {
                CheckPointBlock checkPointBlock = new CheckPointBlock();
                checkPointBlock.AccountKey = b.AccountKey;
                for (int i = 0; i < 5; i++) {
                    checkPointBlock.Accounts.Add(new Account
                    {
                        AccountInfo = new AccountInfo
                        {
                            AccountKey = b.AccountKey,
                            State = AccountInfo.AccountState.Normal
                        }
                    });
                    checkPointBlock.AccumulatedWork = b.CompactTarget;
                    checkPointBlock.AvailableProtocolVersion = b.AvailableProtocol;
                    checkPointBlock.BlockHash = // TODO
                }
            }
        }
        */

        public void LoadFromFile(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite);
            if (stream != null) stream.Dispose();
            stream = null;
            stream = fs;
            Accounts.Clear();
            Accounts = null;
            GC.Collect();
            Header = new CheckPointHeader(stream);
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
            Header = new CheckPointHeader(stream);
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

        public void Append(CheckPoint checkPoint)
        {
            lock (loadLock)
            {
                List<CheckPointBlock> list = new List<CheckPointBlock>();
                if (Header != null)
                {
                    for (uint i = Header.StartBlock; i < Header.EndBlock+1; i++)
                    {
                        list.Add(this[i]);
                    }
                    if (Header.BlockCount > checkPoint.Header.BlockCount)
                    {
                        return;
                    }
                    Header.BlockCount = checkPoint.Header.BlockCount;
                    Header.Hash = checkPoint.Header.Hash;
                    Header.EndBlock = checkPoint.Header.EndBlock;
                }
                else
                {
                    Header = checkPoint.Header;
                }
                for (uint i = 0; i != checkPoint.Header.EndBlock- checkPoint.Header.StartBlock + 1; i++)
                    list.Add(checkPoint[i]);
                MemoryStream ms = new MemoryStream();
                uint[] h = Header.offsets;
                Header.offsets = new uint[list.Count + 1];
                Header.SaveToStream(ms);
                long headerSize = ms.Position;
                ms.Position = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    uint i = 0;
                    foreach (CheckPointBlock b in list)
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

        public IEnumerator<CheckPointBlock> GetEnumerator()
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

        public CheckPoint SaveChunk(uint startBlock, uint endBlock)
        {
            lock (loadLock)
            {
                List<CheckPointBlock> list = new List<CheckPointBlock>();
                CheckPointHeader Header = new CheckPointHeader();
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
                    foreach (CheckPointBlock b in list)
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
                CheckPoint checkPoint = new CheckPoint(memoryStream);
                ms.Dispose();
                memoryStream.Dispose();
                ms = null;
                memoryStream = null;
                GC.Collect();
                return checkPoint;
            }
        }

    }
}