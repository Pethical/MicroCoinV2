using MicroCoin.Cryptography;
using MicroCoin.Util;
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
            LoadFromStream(stream);
        }
        public override void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(SignerAccount);
                bw.Write(TargetAccount);
                bw.Write((ushort)TransactionType);
                bw.Write(NumberOfOperations);
                if (TransactionType == AccountTransactionType.ListAccount)
                {
                    bw.Write(AccountPrice);
                    bw.Write(AccountToPay);
                    AccountKey.SaveToStream(s, false);
                    NewPublicKey.SaveToStream(s, true);
                    bw.Write(LockedUntilBlock);
                }
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                Signature.SaveToStream(s);
            }

        }

        public override void LoadFromStream(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                TransactionType = (AccountTransactionType)br.ReadUInt16();
                NumberOfOperations = br.ReadUInt32();
                if (TransactionType == AccountTransactionType.ListAccount)
                {
                    AccountPrice = br.ReadUInt64();
                    AccountToPay = br.ReadUInt32();
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(s, false);
                    NewPublicKey = new ECKeyPair();
                    NewPublicKey.LoadFromStream(s, true);
                    LockedUntilBlock = br.ReadUInt32();
                }
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                Signature = new ECSig(s);
            }
        }
    }
}