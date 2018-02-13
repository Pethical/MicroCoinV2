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