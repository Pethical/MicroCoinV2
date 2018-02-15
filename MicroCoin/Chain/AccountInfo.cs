using MicroCoin.Cryptography;
using System;
using System.IO;

namespace MicroCoin.Chain
{
    public class AccountInfo
    {
        public enum AccountState { Unknown, Normal, Sale}
        public AccountState State { get; set; }
        public ECKeyPair AccountKey { get; set; }
        public uint LockedUntilBlock { get; set; }
        public ulong Price { get; set; }
        public uint AccountToPayPrice { get; set; }
        public ECKeyPair NewPublicKey { get; set; }

        public AccountInfo()
        {

        }


        public static AccountInfo CreateFromStream(BinaryReader br)
        {
            AccountInfo ai = new AccountInfo();
            ai.LoadFromStream(br);
            return ai;
        }

        public void LoadFromStream(BinaryReader br)
        {
            ushort len = br.ReadUInt16();
            ushort stateOrKeyType = br.ReadUInt16();
            switch (stateOrKeyType)
            {
                case (ushort) CurveType.Secp256K1:
                case (ushort)CurveType.Secp384R1:
                case (ushort)CurveType.Secp521R1:
                case (ushort)CurveType.Sect283K1:
                    br.BaseStream.Position -= 2;
                    State = AccountState.Normal;
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(br.BaseStream, false);
                    LockedUntilBlock = 0;
                    Price = 0;
                    AccountToPayPrice = 0;
                    NewPublicKey = new ECKeyPair();
                    break;
                case 1000:
                    State = AccountState.Sale;
                    AccountKey = new ECKeyPair();
                    AccountKey.LoadFromStream(br.BaseStream, false);
                    LockedUntilBlock = br.ReadUInt32();
                    Price = br.ReadUInt64();
                    AccountToPayPrice = br.ReadUInt32();
                    NewPublicKey = new ECKeyPair();
                    NewPublicKey.LoadFromStream(br.BaseStream, false);
                    break;
                default:
                    throw new Exception("Invalid account info");
            }
        }

    }
}
