﻿/*************************************************************************
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

using MicroCoin.Net;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class BlockRequest : Request
    {
        public uint StartBlock { get; set; }
        public uint BlockNumber { get; set; }
        public BlockRequest()
        {
            RequestType = RequestType.Request;
            Operation = NetOperationType.GetBlocks;
            RequestId = 2;
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using(BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true)) {
                DataLength = 8;
                bw.Write(DataLength);
                bw.Write(StartBlock);
                bw.Write(BlockNumber+StartBlock-1);
            }
        }
    }
}
