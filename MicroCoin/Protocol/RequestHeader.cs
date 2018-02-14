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


using MicroCoin.Net;
using System.IO;
using System.Text;

namespace MicroCoin.Protocol
{
    public class RequestHeader
    {
        public static int size = 4 + 2 + 2 + 2 + 4 + 2 + 2 + 4;

        public uint Magic { get; set; }
        public RequestType RequestType { get; set; }
        public NetOperationType Operation { get; set; }
        public ushort Error { get; set; }
        public uint RequestId { get; set; }
        public ushort ProtocolVersion { get; set; } = 2;
        public ushort AvailableProtocol { get; set; } = 2;
        public int DataLength { get; set; }

        public RequestHeader()
        {
            Magic = 0x0A043580;
            RequestType = RequestType.Request;
            Operation = NetOperationType.Hello;
            ProtocolVersion = 2;
            AvailableProtocol = 2;
            Error = 0;
        }

        public virtual void SaveToStream(Stream s)
        {
            using (BinaryWriter br = new BinaryWriter(s, Encoding.ASCII, true))
            {
                br.Write(Magic);
                br.Write((ushort)RequestType);
                br.Write((ushort)Operation);
                br.Write(Error);
                br.Write(RequestId);
                br.Write(ProtocolVersion);
                br.Write(AvailableProtocol);
            }
        }
    }
}
