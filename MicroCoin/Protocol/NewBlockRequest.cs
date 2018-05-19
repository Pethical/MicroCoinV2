//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// NewBlockRequest.cs - Copyright (c) 2018 N�meth P�ter
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
//-----------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using MicroCoin.Chain;
using System.IO;

namespace MicroCoin.Protocol
{
    public class NewBlockRequest : MessageHeader
    {

        public Block Block { get; set; }

        public NewBlockRequest() : base()
        {

        }

        internal NewBlockRequest(Stream stream) : base(stream)
        {
        }

        internal NewBlockRequest(Stream stream, MessageHeader rp) :base(rp)
        {
            Block = new Block(stream);
        }
    }
}
