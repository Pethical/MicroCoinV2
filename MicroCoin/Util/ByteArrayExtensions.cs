﻿// This file is part of MicroCoin.
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


using System.IO;
using System.Text;

namespace MicroCoin.Util
{
    public static class ByteArrayExtensions{
        public static string ToAnsiString(this byte[] b)
        {
            return Encoding.Default.GetString(b);
        }
        public static void SaveToStream(this byte[] b, BinaryWriter bw)
        {
            bw.Write((ushort)b.Length);
            bw.Write(b, 0, b.Length);
        }

    }
}