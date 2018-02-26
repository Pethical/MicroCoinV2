//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// TransactionBlockResponse.cs - Copyright (c) 2018 Németh Péter
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


using MicroCoin.Chain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Protocol
{
    [Obsolete("Use BlockRespose")]
    public class TransactionBlockResponse : MessageHeader
    {
        public List<Block> List { get; set; }

        public TransactionBlockResponse()
        {
            RequestType = Net.RequestType.Response;
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.Default, true))
                {
                    bw.Write((uint)List.Count);
                    foreach (var c in List)
                    {
                        c.SaveToStream(ms);
                    }
                }                
                using(BinaryWriter bw = new BinaryWriter(s, Encoding.Default, true))
                {
                    DataLength = (int)ms.Length;
                    bw.Write(DataLength);
                }
                ms.CopyTo(s);
            }
        }

        public TransactionBlockResponse(Stream stream) : base(stream)
        {
        }

        public TransactionBlockResponse(Stream stream, MessageHeader rp) : base(rp)
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                List = new List<Block>(br.ReadInt32());
            }
            
            for(int i = 0; i < List.Capacity; i++)
            {
                List.Add(new Block(stream));
            }
        }

    }
}
