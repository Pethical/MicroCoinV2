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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using MicroCoin.Transactions;
using MicroCoin.Util;

namespace MicroCoin.Chain
{
    public class BlockChain : List<Block>
    {
        private static BlockChain _sInstance;

        private static readonly object Flock = new object();

        protected BlockChain()
        {
        }

        public static uint TargetToCompact(BigInteger targetPow)
        {
                        
            BigInteger bn = targetPow;
            BigInteger bn2 = BigInteger.Parse("0800000000000000000000000000000000000000000000000000000000000000", System.Globalization.NumberStyles.HexNumber);
            uint nbits = 4;
            while ((bn < bn2) && (nbits < 231))
            {
                bn2 >>= 1;
                nbits++;
            }

            uint i = 0x19000000 >> 24;
            if (nbits < i)
            {
                return 0x19000000;
            }
            int s = ((256 - 25) - (int)nbits);
            bn = bn >> s;
            return (nbits << 24) + ((uint)bn & 0x00FFFFFF) ^ 0x00FFFFFF;
        }
        public static uint TargetToCompact(Hash targetPow)
        {

            BigInteger bn = new BigInteger( targetPow.Reverse() );
            BigInteger bn2 = BigInteger.Parse("0800000000000000000000000000000000000000000000000000000000000000", System.Globalization.NumberStyles.HexNumber);
            uint nbits = 4;
            while ((bn < bn2) && (nbits < 231))
            {
                bn2 >>= 1;
                nbits++;
            }

            uint i = 0x19000000 >> 24;
            if (nbits < i)
            {
                return 0x19000000;
            }
            int s = ((256 - 25) - (int)nbits);
            bn = bn >> s;
            return (nbits << 24) + ((uint)bn & 0x00FFFFFF) ^ 0x00FFFFFF;
        }

        public Hash GetNewTarget()
        {
            var lastBlock = Get(BlockHeight());
            var lastCheckPointBlock = Get(BlockHeight() - 10);
            var s = String.Format("{0:X}", lastBlock.CompactTarget);
            Hash actualTarget = TargetFromCompact(lastBlock.CompactTarget);
            DateTime ts1 = lastBlock.Timestamp;
            DateTime ts2 = lastCheckPointBlock.Timestamp;
            var tsReal = ts1.Subtract(ts2).TotalSeconds;            
            long tsTeorical = 10*300;
            //long tsReal = vreal;
            long factor1000 = ((long)(((tsTeorical - tsReal) * 1000) / tsTeorical) * -1);
            long factor1000Min = -500 / (100 / 2);
            long factor1000Max = 1000 / (100 / 2);
            if (factor1000 < factor1000Min) factor1000 = factor1000Min;
            else if (factor1000 > factor1000Max) factor1000 = factor1000Max;
            else if (factor1000 == 0) return actualTarget;
            byte[] aT = (byte[]) actualTarget;
            var bnact = new BigInteger(aT.Reverse().ToArray());
            var bnaux = new BigInteger(aT.Reverse().ToArray()); 
            var abc = new BigInteger(0xABCDEF);
            Hash h = abc.ToByteArray();
            bnact *=  factor1000;
            bnact /= 1000;
            bnact += bnaux;
            Hash h1 = bnact.ToByteArray().ToArray();
            Hash h2 = bnact.ToByteArray().Reverse().ToArray();
            var nt = TargetToCompact(bnact);
            var newTarget = nt;
            var sa = String.Format("{0:X}", newTarget);
            Hash Pow = TargetFromCompact(newTarget);
            return Pow;
        }

        public static Hash TargetFromCompact(uint encoded)
        {
            uint nbits = encoded >> 24;
            uint i = 0x19000000 >> 24;
            if (nbits < i)
            {
                nbits = i;
            }
            else if (nbits > 231)
            {
                nbits = 231;
            }
            uint offset = encoded << 8 >> 8;
            offset = ((offset ^ 0x00FFFFFF) | (0x01000000));
            BigInteger bn = new BigInteger(offset);
            uint shift = (256 - nbits - 25);
            bn = bn << (int)shift;
            byte[] r = new byte[32];
            byte[] ba = bn.ToByteArray().Reverse().ToArray();
            for (var index = 0; index < ba.Length; index++)
            {
                r[32 + index - ba.Length] = ba[index];
            }
            return r;
        }


        internal static BlockChain Instance => _sInstance ?? (_sInstance = new BlockChain());

        internal string BlockChainFileName { get; set; } = "block.chain";

        internal void LoadFromStream(Stream s)
        {
            while (s.Position < s.Length - 1) Add(new Block(s));
        }

        internal void SaveToStream(Stream s)
        {
            foreach (var t in this) t.SaveToStream(s);
        }

        public int BlockHeight()
        {
            lock (Flock)
            {
                if (true)
                {
                    var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    using (var br = new BinaryReader(fi))
                    {
                        if (fi.Length == 0) return 0;
                        fi.Position = fi.Length - 16;
                        return br.ReadInt32();
                    }
                }
            }
        }

        public List<Transaction> GetAccountOperations(int accountNumber)
        {
            var account = CheckPoints.Accounts[accountNumber];
            uint i = 0;
            var blocks = new List<Block>();
            var result = new List<Transaction>();
            while (i < account.UpdatedBlock)
            {
                var start = i;
                var end = i + 1000;
                if (start > account.UpdatedBlock) start = account.UpdatedBlock;
                if (end > account.UpdatedBlock) end = account.UpdatedBlock;
                blocks.AddRange(Get(start, end));
                i = end;
            }

            foreach (var b in blocks)
            {
                var l = b.Transactions.Where(p =>
                    p.SignerAccount == accountNumber || p.TargetAccount == accountNumber || p is TransferTransaction &&
                    ((TransferTransaction) p).SellerAccount == accountNumber);
                result.AddRange(l);
            }

            return result;
        }

        public Block Get(int blockNumber)
        {
            lock (Flock)
            {
                var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    using (var ir = new BinaryReader(fi, Encoding.Default, true))
                    {
                        fi.Position = 16;
                        var first = ir.ReadUInt32();
                        fi.Position = (blockNumber - first) * 16 + 16;
                        var bn = ir.ReadUInt32();
                        var pos = ir.ReadInt64();
                        if (bn != blockNumber) return null;
                        var f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        try
                        {
                            f.Position = pos;
                            var tb = new Block(f);
                            return tb;
                        }
                        finally
                        {
                            f.Close();
                            f.Dispose();
                        }
                    }
                }
                finally
                {
                    fi.Close();
                    fi.Dispose();
                }
            }
        }

        public List<Block> Get(uint start, uint end)
        {
            lock (Flock)
            {
                var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    using (var ir = new BinaryReader(fi, Encoding.Default, true))
                    {
                        fi.Position = 16;
                        var first = ir.ReadUInt32();
                        fi.Position = (start - first) * 16 + 16;
                        var bn = ir.ReadUInt32();
                        var pos = ir.ReadInt64();
                        if (bn != start) return null;
                        var f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        try
                        {
                            f.Position = pos;
                            var blocks = new List<Block>();
                            for (var i = start; i <= end; i++)
                            {
                                var tb = new Block(f);
                                blocks.Add(tb);
                            }

                            return blocks;
                        }
                        finally
                        {
                            f.Close();
                            f.Dispose();
                        }
                    }
                }
                finally
                {
                    fi.Close();
                    fi.Dispose();
                }
            }
        }

        public Block GetLastBlock()
        {
            lock (Flock)
            {
                var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    using (var ir = new BinaryReader(fi, Encoding.Default, true))
                    {
                        if (fi.Length == 0) return Block.GenesisBlock;
                        fi.Position = fi.Length - 16;
                        var blockNumber = ir.ReadUInt32();
                        var position = ir.ReadInt64();
                        var f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        try
                        {
                            if (f.Length == 0) throw new Exception("No blockchain file.");

                            f.Position = position;
                            return new Block(f);
                        }
                        finally
                        {
                            f.Close();
                            f.Dispose();
                        }
                    }
                }
                finally
                {
                    fi.Close();
                    fi.Dispose();
                }
            }
        }

        internal bool Append(Block t)
        {
            lock (Flock)
            {
                var blockHeight = GetLastBlock().BlockNumber + 1;
                var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    var f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    try
                    {
                        using (var br = new BinaryReader(fi, Encoding.Default, true))
                        {
                            int count;
                            if (fi.Length == 0)
                            {
                                count = 0;
                            }
                            else
                            {
                                count = br.ReadInt32();
                                var size = br.ReadInt64();
                            }

                            if (blockHeight < t.BlockNumber - 1)
                            {
                                return false;
                            }
                            else if (blockHeight > t.BlockNumber)
                            {
                                return true;
                            }

                            using (var iw = new BinaryWriter(fi, Encoding.Default, true))
                            {
                                iw.BaseStream.Position = iw.BaseStream.Length;
                                fi.Position = 0;
                                iw.Write(count + 1);
                                f.Position = f.Length;
                                fi.Position = fi.Length;
                                var pos = f.Position;
                                t.SaveToStream(f);
                                iw.Write(t.BlockNumber);
                                iw.Write(pos);
                                iw.Write((uint) (f.Position - pos));
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

        internal void AppendAll(List<Block> blocks, bool ignoreCheckPointing = false)
        {
            lock (Flock)
            {
                var blockHeight = GetLastBlock().BlockNumber;
                var f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (var br = new BinaryReader(fi))
                {
                    int count;
                    var size = 0;
                    if (fi.Length == 0)
                    {
                        count = 0;
                    }
                    else
                    {
                        count = br.ReadInt32();
                        size = br.ReadInt32();
                        br.ReadUInt64(); // Padding
                    }

                    using (var iw = new BinaryWriter(fi, Encoding.Default, true))
                    {
                        fi.Position = 0;
                        iw.Write(count);
                        iw.Write(size);
                        iw.Write((ulong) 0);
                        foreach (var block in blocks)
                        {
                            if (block.BlockNumber != 0 || blockHeight > 0)
                                if (block.BlockNumber <= blockHeight)
                                    continue;
                            f.Position = f.Length;
                            fi.Position = fi.Length;
                            var pos = f.Position;
                            block.SaveToStream(f);
                            iw.Write(block.BlockNumber);
                            iw.Write(pos);
                            iw.Write((uint) (f.Position - pos));
                            iw.BaseStream.Position = 0;
                            count++;
                            iw.Write(count);
                            iw.Write(fi.Length);
                            if (!ignoreCheckPointing) CheckPoints.AppendBlock(block);
                        }
                    }
                }

                f.Close();
                f.Dispose();
            }
        }

        internal void SaveToStorage(Stream s)
        {
            lock (Flock)
            {
//                byte[] b;
                var f = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (s.Length > 0 && f.Length > 0)
                    using (var br = new BinaryReader(f, Encoding.Default, true))
                    {
                        br.ReadInt32();
                        var size = br.ReadInt32();
                        br.ReadUInt64(); // Padding
                    }

                using (var iw = new BinaryWriter(f))
                {
                    f.Position = 0;
                    s.Position = s.Length;
                    iw.Write((uint) Count);
                    iw.Write((uint) 0);
                    iw.Write((ulong) 0);
//                        b = null;
                    foreach (var t in this)
                    {
                        var pos = s.Position;
                        t.SaveToStream(s);
                        iw.Write(t.BlockNumber);
                        iw.Write(pos);
                        iw.Write((uint) (s.Position - pos));
                    }
                }
            }
        }

        public List<Block> GetBlocks(uint startBlock, uint endBlock)
        {
            if (endBlock >= GetLastBlock().BlockNumber) endBlock = GetLastBlock().BlockNumber - 1;
            return Get(startBlock, endBlock);
        }
    }
}