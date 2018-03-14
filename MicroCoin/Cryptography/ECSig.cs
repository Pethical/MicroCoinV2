//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// ECSig.cs - Copyright (c) 2018 Németh Péter
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


using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using MicroCoin.Util;

namespace MicroCoin.Cryptography
{
    public class ECSig
    {
        private Hash sign;

        public byte[] r { get; set; }
        public byte[] s { get; set; }

        public byte[] Signature
        {
            get
            {
                var ret = r.ToList();
                ret.AddRange(s);
                return ret.ToArray();
            }
        }
        
        public ECSig() { }
        public ECSig(Stream stream) {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ushort len = br.ReadUInt16();
                r = new byte[len];
                br.Read(r, 0, len);
                len = br.ReadUInt16();
                s = new byte[len];
                br.Read(s, 0, len);
            }
        }

        public ECSig(Hash sign)
        {
            this.sign = sign;
            byte[] data = sign;
            r = data.Take(32).ToArray();
            s = data.Skip(32).Take(32).ToArray();
        }

        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write((ushort)r.Length);
                bw.Write(r);
                bw.Write((ushort)s.Length);
                bw.Write(s);
            }
        }
    }
}