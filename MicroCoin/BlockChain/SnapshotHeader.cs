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


using System;

namespace MicroCoin.BlockChain
{
    public class SnapshotHeader
    {
	public byte[] Magic { get; set; }
	public ushort Protocol{ get; set; }
	public ushort Version { get ;set; }
	public uint BlockCount { get; set; }
	public uint StartBlock { get; set; }
	public uint EndBlock { get; set; }
	public byte[] Hash { get; set; }

	public void LoadFromStream(Stream s) {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
		ushort len = br.ReadUInt16();
		Magic = br.ReadBytes(len);
		Version = br.ReadUIn16();
		BlockCount = br.ReadUInt32();
		StartBlock = br.ReadUInt32();
		EndBlock = br.ReadUInt32();
	    }
	}
    }
}
