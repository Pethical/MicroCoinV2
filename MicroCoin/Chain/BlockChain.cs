//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// BlockChain.cs - Copyright (c) 2018 Németh Péter
//-----------------------------------------------------------------------
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//-------------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MicroCoin.Transactions;

namespace MicroCoin.Chain
{
    public class BlockChain : List<Block>
    {
        private static BlockChain _sInstance;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected BlockChain() { }

        public static BlockChain Instance
        {
            get { return _sInstance ?? (_sInstance = new BlockChain()); }
        }

        public void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length - 1)
            {
                Add(new Block(s));
            }
        }
        public void SaveToStream(Stream s)
        {
            foreach (var t in this)
            {
                t.SaveToStream(s);
            }
        }

        public string BlockChainFileName { get; set; } = "block.chain";
        public int BlockHeight()
        {
            lock (flock)
            {
                if (true)
                {
                    FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    try
                    {
                        using (BinaryReader br = new BinaryReader(fi, Encoding.Default, true))
                        {
                            if (fi.Length == 0) return 0;
                            fi.Position = fi.Length - 16;
                            return br.ReadInt32();
                        }
                    }
                    finally
                    {
                        fi.Close();
                        fi.Dispose();
                        fi = null;
                    }
                }                
            }
        }

        public List<Transaction> GetAccountOperations(int accountNumber)
        {
            List<Transaction> result = new List<Transaction>();
            for (uint i = GetLastBlock().BlockNumber; i > 0; i--)
            {
                Block b = Get((int)i);
                if (b.Transactions != null)
                {
                    var l = b.Transactions.Where(p => p.SignerAccount == accountNumber || p.TargetAccount == accountNumber || (p is Transactions.TransferTransaction && ((Transactions.TransferTransaction)p).SellerAccount == accountNumber));
                    result.AddRange(l);
                }
            }
            return result;
        }

        private static object flock = new object();

        public Block Get(int blockNumber)
        {
            lock (flock)
            {
                FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    using (BinaryReader ir = new BinaryReader(fi, Encoding.Default, true))
                    {
                        fi.Position = 16;
                        uint first = ir.ReadUInt32();
                        fi.Position = (blockNumber-first) * 16 + 16;
                        uint bn = ir.ReadUInt32();
                        long pos = ir.ReadInt64();
                        if (bn != blockNumber)
                        {
                            return null;
                        }
                        FileStream f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        try
                        {
                            f.Position = pos;
                            Block tb = new Block(f);
                            return tb;
                        }
                        finally
                        {
                            f.Close();
                            f.Dispose();
                            f = null;
                        }
                    }
                }
                finally
                {
                    fi.Close();
                    fi.Dispose();
                    fi = null;
                }
            }
        }

        

        public Block GetLastBlock()
        {
            lock (flock)
            {
                FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    using (BinaryReader ir = new BinaryReader(fi, Encoding.Default, true))
                    {
                        if (fi.Length == 0) return Block.NullBlock;
                        fi.Position = fi.Length - 16;
                        uint blockNumber = ir.ReadUInt32();
                        long position = ir.ReadInt64();
			            //log.Debug($"Last block {blockNumber} {position}");
                        FileStream f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        try
                        {
                            if (f.Length == 0)
                            {
                                throw new Exception("No blockchain file.");
                            }
                            using (BinaryReader br = new BinaryReader(f, Encoding.Default, true))
                            {
                                f.Position = position;
                                return new Block(f);
                            }
                        }
                        finally
                        {
                            f.Close();
                            f.Dispose();
                            f = null;
                        }
                    }
                }
                finally
                {
                    fi.Close();
                    fi.Dispose();
                    fi = null;
                }
            }
        }

        public bool Append(Block t)
        {
            lock (flock)
            {
                uint blockHeight = GetLastBlock().BlockNumber+1;
                FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    FileStream f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    try
                    {
                        using (BinaryReader br = new BinaryReader(fi, Encoding.Default, true))
                        {
                            int count = 0;
                            long size = 0;
                            int indexSize = 0;
                            if (fi.Length == 0)
                            {
                                count = 0;
                            }
                            else
                            {
                                count = br.ReadInt32();
                                size = br.ReadInt64();
                                indexSize = (int)(size + 16);
                            }
                            if (blockHeight < t.BlockNumber-1)
                            {
                                return false;
                            }
                            else if (blockHeight > t.BlockNumber)
                            {
                                log.Warn($"CheckPointBlock already added to chain. My block height: #{blockHeight}. Received block: #{t.BlockNumber}");
                                return true;
                            }
                            using (BinaryWriter iw = new BinaryWriter(fi, Encoding.Default, true))
                            {
                                iw.BaseStream.Position = iw.BaseStream.Length;
                                fi.Position = 0;
                                iw.Write(count+1);
                                f.Position = f.Length;
                                fi.Position = fi.Length;
                                long pos = f.Position;
                                t.SaveToStream(f);
                                iw.Write(t.BlockNumber);
                                iw.Write(pos);
                                iw.Write((uint)(f.Position - pos));
                                log.Info($"Added new block #{t.BlockNumber}");
                            }
                            CheckPoints.AppendBlock(t);
                            return true;
                        }
                    }
                    finally
                    {
                        f.Dispose();
                    }
                }
                finally
                {
                    fi.Dispose();
                }
            }            
        }
        public void AppendAll(List<Block> ts, bool ignoreCheckPointing=false)
        {
            lock (flock)
            {
                var blockHeight = GetLastBlock().BlockNumber;
                FileStream f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (BinaryReader br = new BinaryReader(fi))
                {
                    int count = 0;
                    int size = 0;
                    int indexSize = 0;
                    if (fi.Length == 0)
                    {
                        count = 0;
                    }
                    else
                    {
                        count = br.ReadInt32();
                        size = br.ReadInt32();
                        indexSize = size + 16;
                        br.ReadUInt64(); // Padding
                    }
                    using (BinaryWriter iw = new BinaryWriter(fi, Encoding.Default, true))
                    {
                        fi.Position = 0;
                        iw.Write(count);
                        iw.Write(size);
                        iw.Write((ulong)0);
                        foreach (var t in ts)
                        {
                            if(t.BlockNumber!=0 ||blockHeight>0)
                                if (t.BlockNumber <= blockHeight) continue;
                            f.Position = f.Length;
                            fi.Position = fi.Length;
                            long pos = f.Position;
                            t.SaveToStream(f);
                            iw.Write(t.BlockNumber);
                            iw.Write(pos);
                            iw.Write((uint)(f.Position - pos));
                            iw.BaseStream.Position = 0;
			                count++;
                            iw.Write(count);
                            iw.Write(fi.Length);
                            if(!ignoreCheckPointing) CheckPoints.AppendBlock(t);
                        }
                        log.Info($"Saved {ts.Count} blocks. From {ts.FirstOrDefault()?.BlockNumber} to {ts.LastOrDefault()?.BlockNumber}. New count: {count}");
                    }
                }
                f.Close();
                f.Dispose();
                f = null;
            }
        }

        public void SaveToStorage(Stream s)
        {
            lock (flock)
            {
//                byte[] b;
                int count = 0;
                int size = 0;
                FileStream f = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (s.Length > 0 && f.Length > 0)
                {
                    using (BinaryReader br = new BinaryReader(f, Encoding.Default, true))
                    {
                        count = br.ReadInt32();
                        size = br.ReadInt32();
                        int indexSize = size > Count * 16 ? size : Count * 16;
                        br.ReadUInt64(); // Padding
                    }
                }
                using (BinaryWriter iw = new BinaryWriter(f))
                {
                    using (BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
                    {
                        f.Position = 0;
                        s.Position = s.Length;
                        iw.Write((uint)Count);
                        iw.Write((uint)0);
                        iw.Write((ulong)0);
//                        b = null;
                        foreach (var t in this)
                        {
                            long pos = s.Position;
                            t.SaveToStream(s);
                            iw.Write(t.BlockNumber);
                            iw.Write(pos);
                            iw.Write((uint)(s.Position - pos));
                        }
                    }
                }
            }
        }

        public List<Block> GetBlocks(uint startBlock, uint endBlock)
        {
            log.Info($"GetBlocks {startBlock}=>{endBlock}");
            if (endBlock >= GetLastBlock().BlockNumber)
            {
                endBlock = GetLastBlock().BlockNumber - 1;
            }
            List<Block> list = new List<Block>();
            for(uint i = startBlock; i < endBlock+1; i++)
            {
                list.Add(Get((int)i));
            }
            return list;
        }
    }
}