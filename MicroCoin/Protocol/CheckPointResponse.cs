//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// CheckPointResponse.cs - Copyright (c) 2018 Németh Péter
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
using MicroCoin.Util;
using System.IO;
using System.Text;
using zlib;

namespace MicroCoin.Protocol
{
    public class CheckPointResponse : MessageHeader
    {
        public ByteString CheckPointResponseMagic { get; set; }
        public ushort Version { get; set; }
        protected uint UncompressedSize { get; set; }
        protected uint CompressedSize { get; set; }
        public CheckPoint CheckPoint { get; set; }

        public CheckPointResponse() 
        {
            RequestType = Net.RequestType.Response;
        }

        private static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
                {
                    using (Stream inMemoryStream = new MemoryStream(inData))
                    {
                        CopyStream(inMemoryStream, outZStream);
                        outZStream.finish();
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }

        private static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        internal override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using(BinaryWriter bw  = new BinaryWriter(s, Encoding.Default, true))
            {                
                using (MemoryStream ms = new MemoryStream())
                {
                    CheckPoint.SaveToStream(ms);
                    CompressData(ms.GetBuffer(), out byte[] compressed);
                    bw.Write("SafeBoxChunk");
                    bw.Write(Version);
                    bw.Write((uint)ms.Length);
                    bw.Write((uint)compressed.Length);
                    bw.Write(compressed);
                }                
            }
        }

        public CheckPointResponse(Stream s) : base(s)
        {
            MemoryStream unCompressed;
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true)) {
                CheckPointResponseMagic = ByteString.ReadFromStream(br);
                Version = br.ReadUInt16();
                UncompressedSize = br.ReadUInt32();
                CompressedSize = br.ReadUInt32();
                DecompressData(br.ReadBytes((int)CompressedSize), out byte[] decompressed);               
                unCompressed = new MemoryStream(decompressed);
            }
            unCompressed.Position = 0;
            CheckPoint = new CheckPoint(unCompressed);
            unCompressed.Dispose();
            //Node.Instance.CheckPoint.Append(CheckPoint);
            CheckPoint.Dispose();
            CheckPoint = null;
        }
    }
}
