using MicroCoin.Cryptography;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public class ListAccountTransaction : Transaction
    {
        public enum AccountTransactionType : ushort
        {
            ListAccount = 4,
            DeListAccount = 5
        };
        public AccountTransactionType TransactionType { get; set; }
        public ulong AccountPrice { get; set; }
        public uint AccountToPay { get; set; }
        public ECKeyPair NewPublicKey { get; set; }
        public uint LockedUntilBlock { get; set; }
        public ListAccountTransaction() { }
        public ListAccountTransaction(Stream stream) {
            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                TransactionType = (AccountTransactionType) br.ReadUInt16();
                NumberOfOperations = br.ReadUInt32();
                if(TransactionType == AccountTransactionType.ListAccount)
                {
                    AccountPrice = br.ReadUInt64();
                    AccountToPay = br.ReadUInt32();
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(stream, false);
                    NewPublicKey = new ECKeyPair();
                    NewPublicKey.LoadFromStream(stream, true);
                    LockedUntilBlock = br.ReadUInt32();
                }
                Fee = br.ReadUInt64();
                ushort len = br.ReadUInt16();
                Payload = new byte[len];
                br.Read(Payload, 0, len);
                Signature = new ECSig(stream);
            }
        }
        public override void SaveToStream(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}