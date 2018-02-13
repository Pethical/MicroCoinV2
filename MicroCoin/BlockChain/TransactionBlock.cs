using MicroCoin.Cryptography;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.BlockChain
{
    public enum TransactionType : uint
    {
        Transaction = 1, ChangeKey, RecoverFounds, ListAccountForSale,
        DeListAccountForSale, BuyAccount, ChangeKeySigned, ChangeAccountInfo
    };

    public class TransactionBlock
    {
        public TransactionBlock(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, System.Text.Encoding.ASCII, true))
            {
                OperationBlockSignature = br.ReadByte();
                if (OperationBlockSignature > 0)
                {
                    ProtocolVersion = br.ReadUInt16();
                    AvailableProtocol = br.ReadUInt16();
                }
                BlockNumber = br.ReadUInt32();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(s);
                Reward = br.ReadUInt64();
                Fee = br.ReadUInt64();
                Timestamp = br.ReadUInt32();
                CompactTarget = br.ReadUInt32();
                Nonce = br.ReadUInt32();
                ushort pl = br.ReadUInt16();
                if (pl > 0)
                {
                    Payload = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    SafeBoxHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    OperationHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    ProofOfWork = br.ReadBytes(pl);
                }
            }
        }

        public TransactionBlock()
        {

        }

        public byte OperationBlockSignature { get; set; } = 3;
        public ushort ProtocolVersion { get; set; } = 0;
        public ushort AvailableProtocol { get; set; } = 0;
        public uint BlockNumber { get; set; } = 0;
        public ECKeyPair AccountKey { get; set; } = new ECKeyPair();
        public UInt64 Reward { get; set; } = 0;
        public UInt64 Fee { get; set; } = 0;
        public uint Timestamp { get; set; } = 0;
        public uint CompactTarget { get; set; } = 0;
        public uint Nonce { get; set; } = 0;
        public byte[] Payload { get; set; }
        public string PayloadString
        {
            get
            {
                return Encoding.UTF8.GetString(Payload);
            }
        }
        public byte[] SafeBoxHash { get; set; }
        public byte[] OperationHash { get; set; }
        public byte[] ProofOfWork { get; set; }

        public virtual void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, System.Text.Encoding.ASCII, true))
            {
                bw.Write(OperationBlockSignature);
                bw.Write(ProtocolVersion);
                bw.Write(AvailableProtocol);
                bw.Write(BlockNumber);
                if (AccountKey == null)
                {
                    bw.Write((ushort)6);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                }
                else
                {
                    AccountKey.SaveToStream(s);
                }
                bw.Write(Reward);
                bw.Write(Fee);
                bw.Write(Timestamp);
                bw.Write(CompactTarget);
                bw.Write(Nonce);
                if (Payload != null)
                {
                    bw.Write((ushort)Payload.Length);
                    bw.Write(Payload);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (SafeBoxHash != null)
                {
                    bw.Write((ushort)SafeBoxHash.Length);
                    bw.Write(SafeBoxHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (OperationHash != null)
                {
                    bw.Write((ushort)OperationHash.Length);
                    bw.Write(OperationHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (ProofOfWork != null)
                {
                    bw.Write((ushort)ProofOfWork.Length);
                    bw.Write(ProofOfWork);
                }
                else
                {
                    bw.Write((ushort)0);
                }
            }
        }

    }
}