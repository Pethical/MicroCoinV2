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

using MicroCoin.Cryptography;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public abstract class Transaction
    {
        public uint SignerAccount { get; set; }
        public uint NumberOfOperations { get; set; }
        public uint TargetAccount { get; set; }
        public byte[] Payload { get; set; }
        public ECSig Signature { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public ulong Fee { get; set; }
        abstract public void SaveToStream(Stream s);
        abstract public void LoadFromStream(Stream s);
        protected void ReadPayLoad(BinaryReader br)
        {
            ushort len = br.ReadUInt16();
            Payload = br.ReadBytes(len);
        }
    }
}