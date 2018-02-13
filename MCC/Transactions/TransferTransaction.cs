using MicroCoin.Cryptography;
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
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                SignerAccount = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                TargetAccount = br.ReadUInt32();
                Amount = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ushort payloadLength = br.ReadUInt16();
                Payload = new byte[payloadLength];
                br.Read(Payload, 0, payloadLength); // 372
                try
                {
                    //ushort u = br.ReadUInt16();
                    //Console.WriteLine(u);
                    //stream.Position-=2;
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(stream, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(payloadLength);
                    Console.WriteLine("Error {0} {1} {2}", SignerAccount, TargetAccount, Amount);
                    throw e;
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
                        Console.WriteLine("Account Operation: Price: {0}, Seller: {1}, B:{2}, newKey {3}", AccountPrice, SellerAccount, b, Encoding.ASCII.GetString(NewAccountKey.pub.AffineXCoord.GetEncoded()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("B: {0}", b);
                        throw;
                    }
                }
                Signature = new ECSig(stream);
            }
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
                bw.Write((ushort)Payload.Length);
                bw.Write(Payload);
                AccountKey.SaveToStream(s, false);
                Signature.SaveToStream(s);
            }
        }
    }
}