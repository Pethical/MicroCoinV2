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

using System.IO;
using System.Text;

namespace MicroCoin.Cryptography
{
    public class ECSig
    {
        public byte[] r { get; set; }
        public byte[] s { get; set; }

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