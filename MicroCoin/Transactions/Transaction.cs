using MicroCoin.Cryptography;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public abstract class Transaction
    {
        public uint SignerAccount { get; set; }
        public uint NumberOfOperations { get; set; }
        public uint TargetAccount { get; set; }
        public byte[] Payload { get; set; }
        public ECSig Signature { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public ulong Fee { get; set; }
        abstract public void SaveToStream(Stream s);
        abstract public void LoadFromStream(Stream s);
        protected void ReadPayLoad(BinaryReader br)
        {
            ushort len = br.ReadUInt16();
            Payload = br.ReadBytes(len);
        }
    }
}