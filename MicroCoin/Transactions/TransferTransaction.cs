using MicroCoin.Cryptography;
using MicroCoin.Util;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Transactions
{
    public class TransferTransaction : Transaction
    {
        public ulong Amount { get; set; }
        public ulong AccountPrice { get; set; }
        public uint SellerAccount { get; set; }
        public ECKeyPair NewAccountKey { get; set; }
        public TransferTransaction(Stream stream)
        {
            LoadFromStream(stream);
        }
        override public void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(SignerAccount);
                bw.Write(NumberOfOperations);
                bw.Write(TargetAccount);
                bw.Write(Amount);
                bw.Write(Fee);
                Payload.SaveToStream(bw);
                AccountKey.SaveToStream(s, false);
                Signature.SaveToStream(s);
            }
        }

        public override void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                SignerAccount = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                Amount = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ReadPayLoad(br);
                try
                {
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(stream, false);
                }
                catch (Exception e)
                {                    
                    throw;
                }
                byte b = br.ReadByte();
                if (b > 2) { stream.Position -= 1; }
                if (b > 0 && b < 3)
                {
                    try
                    {
                        AccountPrice = br.ReadUInt64();
                        SellerAccount = br.ReadUInt32();
                        NewAccountKey = new ECKeyPair();
                        NewAccountKey.LoadFromStream(stream, false);
                    }
                    catch (Exception e)
                    {                     
                        throw;
                    }
                }
                Signature = new ECSig(stream);
            }

        }
    }
}