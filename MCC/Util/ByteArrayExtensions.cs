using System.Text;

namespace MicroCoin.Util
{
    public static class ByteArrayExtensions{
        public static string ToAnsiString(this byte[] b)
        {
            return Encoding.Default.GetString(b);
        }
    }
}