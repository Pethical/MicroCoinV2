﻿using MicroCoin.Chain;
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
    public class SnapshotResponse : MessageHeader
    {
        public ByteString SnapshotResponseMagic { get; set; }
        public ushort Version { get; set; }
        public uint UncompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public Snapshot Snapshot { get; set; }

        public SnapshotResponse() : base()
        {

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

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public SnapshotResponse(Stream s) : base(s)
        {
            MemoryStream unCompressed;
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true)) {
                SnapshotResponseMagic = ByteString.ReadFromStream(br);
                Version = br.ReadUInt16();
                UncompressedSize = br.ReadUInt32();
                CompressedSize = br.ReadUInt32();
                DecompressData(br.ReadBytes((int)CompressedSize), out byte[] decompressed);               
                unCompressed = new MemoryStream(decompressed);
            }
            unCompressed.Position = 0;
            Snapshot = new Snapshot(unCompressed);
            unCompressed.Dispose();
            unCompressed = null;
            Node.Instance.Snapshot.Append(Snapshot);
            Snapshot.Dispose();
            Snapshot = null;
        }
    }
}
