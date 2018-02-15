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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Chain
{
    public class BlockChain : List<TransactionBlock>
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
            blockHeight = 0;
            while (s.Position < s.Length - 1)
            {
                Add(new TransactionBlock(s));
            }
        }
        public void SaveToStream(Stream s)
        {
            blockHeight = 0;
            foreach (var t in this)
            {                
                t.SaveToStream(s);
            }
        }

        public string BlockChainFileName { get; set; } = "block.chain";

        private int blockHeight = 0;
        public int BlockHeight()
        {
            lock (flock)
            {
                if (blockHeight == 0)
                {
                    FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    using (BinaryReader br = new BinaryReader(fi))
                    {
                        if (fi.Length == 0) return 0;
                        blockHeight = br.ReadInt32()-1;
                    }
                }
                return blockHeight;
            }
        }
        private static object flock = new object();

        public void Append(TransactionBlock t)
        {
            lock (flock)
            {
                blockHeight = 0;
                FileStream f = File.Open(BlockChainFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                FileStream fi = File.Open(BlockChainFileName + ".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (BinaryReader br = new BinaryReader(fi, Encoding.Default, true))
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
                    if (count > t.BlockNumber)
                    {u
                        throw new Exception($"Bad block. My cont {count}. BlockNumber: {t.BlockNumber}. Need to download new chain");
                    }
                    using (BinaryWriter iw = new BinaryWriter(fi, Encoding.Default, true))
                    {
                        iw.BaseStream.Position = iw.BaseStream.Length;
                        fi.Position = 0;
                        iw.Write(count + 1);
                        f.Position = f.Length;
                        fi.Position = fi.Length;
                        long pos = f.Position;
                        t.SaveToStream(f);
                        iw.Write(t.BlockNumber);
                        iw.Write(pos);
                        iw.Write((uint)(f.Position - pos));
                        iw.Write((long)0);
                        log.Debug($"Saved {t.BlockNumber}");
                    }
                }
                fi.Dispose();
                f.Dispose();
            }
            
        }

        public void SaveToStorage(Stream s)
        {
            blockHeight = 0;
            byte[] b;
            int count = 0;
            int size = 0;
            FileStream f = File.Open(BlockChainFileName+".index", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (s.Length > 0 && f.Length > 0)
            {
                using(BinaryReader br = new BinaryReader(f,Encoding.Default, true))
                {
                    count = br.ReadInt32();
                    size = br.ReadInt32();
                    int indexSize = size > Count * 16 ? size : Count * 16;
                    b = new byte[indexSize + 16];
                    br.ReadUInt64(); // Padding
                    br.Read(b, 0, size);
                    log.Debug($"Loaded index for {count} blocks. We have {Count} blocks");
                }
            }
            else
            {
                b = new byte[Count * 16];
            }
            using (BinaryWriter iw = new BinaryWriter(f))
            {
                using (BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
                {
                    f.Position = 0;
                    s.Position = 0;
                    iw.Write((uint)Count);
                    iw.Write(b.Length);
                    iw.Write((ulong)0);
                    b = null;
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
}