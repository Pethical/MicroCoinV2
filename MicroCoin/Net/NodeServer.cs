//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// NodeServerList.cs - Copyright (c) 2018 Németh Péter
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


using MicroCoin.Util;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MicroCoin.Net
{
    public class NodeServer
    {
        public ByteString IP { get; set; }

        public ushort Port { get; set; }

        public Timestamp LastConnection { get; set; }        

        public IPEndPoint EndPoint => new IPEndPoint(IPAddress.Parse(IP), Port);

        public TcpClient TcpClient { get; set; }

        public bool Connected { get; set; }

        internal MicroCoinClient MicroCoinClient { get; set; }

        internal static void LoadFromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return IP +":"+ Port;
        }

        private readonly object _clientLock = new object();

        internal MicroCoinClient Connect()
        {
            lock (_clientLock)
            {
                if (Connected) return MicroCoinClient;
                MicroCoinClient = new MicroCoinClient();
                if(!MicroCoinClient.Connect(IP, Port))
                {
                    return null;
                }
                MicroCoinClient.Start();
                if (MicroCoinClient.Connected)
                {
                    Connected = true;
                    return MicroCoinClient;
                }
                return null;
            }
        }
    }
}