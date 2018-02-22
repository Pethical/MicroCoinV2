using MicroCoin.Chain;
using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace MicroCoin.Protocol
{
    public class CheckPointResponse : MessageHeader
    {
        public ByteString CheckPointResponseMagic { get; set; }
        public ushort Version { get; set; }
        public uint UncompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public CheckPoint CheckPoint { get; set; }

        public CheckPointResponse() : base()
        {
            RequestType = Net.RequestType.Response;
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void CompressData(byte[] inData, out byte[] outData)
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

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public override void SaveToStream(Stream s)
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
            unCompressed = null;
            Node.Instance.CheckPoint.Append(CheckPoint);
            CheckPoint.Dispose();
            CheckPoint = null;
        }
    }
}
