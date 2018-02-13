using MicroCoin.BlockChain;
using MicroCoin.Cryptography;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public class ChangeKeyTransaction : Transaction
    {
        public ECKeyPair NewAccountKey { get; set; }
        public ChangeKeyTransaction() { }
        public ChangeKeyTransaction(Stream s, TransactionType OpType)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                SignerAccount = br.ReadUInt32();
                if (OpType == TransactionType.ChangeKey)
                {
                    TargetAccount = SignerAccount;
                }
                else if (OpType == TransactionType.ChangeKeySigned)
                {
                    TargetAccount = br.ReadUInt32();
                }
                NumberOfOperations = br.ReadUInt32();
                Fee = br.ReadUInt64();
                ushort len = br.ReadUInt16();
                Payload = new byte[len];
                br.Read(Payload, 0, len);
                AccountKey = new ECKeyPair();
                try
                {
                    AccountKey.LoadFromStream(s, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("PublicKey");
                    Console.ReadLine();
                    throw e;
                }
                NewAccountKey = new ECKeyPair();
                try
                {
                    NewAccountKey.LoadFromStream(s, true);
                    //Console.WriteLine("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine("NewAccountKey");
                    Console.ReadLine();
                    throw e;
                }
                Signature = new ECSig(s);
            }
        }

        public override void SaveToStream(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}