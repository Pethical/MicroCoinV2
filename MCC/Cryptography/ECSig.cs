using System.IO;
using System.Text;

namespace MicroCoin.Cryptography
{
    public class ECSig
    {
        public byte[] r { get; set; }
        public byte[] s { get; set; }

        public ECSig() { }
        public ECSig(Stream stream) {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ushort len = br.ReadUInt16();
                r = new byte[len];
                br.Read(r, 0, len);
                len = br.ReadUInt16();
                s = new byte[len];
                br.Read(s, 0, len);
            }
        }

        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write((ushort)r.Length);
                bw.Write(r);
                bw.Write((ushort)s.Length);
                bw.Write(s);
            }
        }
    }
}