using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Chain
{
    public class Account : IEquatable<Account>
    {
        private uint _updatedBlock;
        public uint AccountNumber { get; set; }
        public AccountInfo AccountInfo { get; set; }
        public ulong Balance { get; set; }
        public uint UpdatedByBlock { get; set; }
        public uint NumberOfOperations { get; set; }
        public ByteString Name { get; set; }
        public ushort AccountType { get; set; }
        public uint UpdatedBlock
        {
            get => _updatedBlock;
            set
            {
                if (_updatedBlock != value)
                {
                    if (_updatedBlock != BlockNumber)
                    {                        
                        UpdatedByBlock = _updatedBlock;
                    }
                    _updatedBlock = value;
                }
            }
        }
        
        /// <summary>
        /// Only reference, don't save
        /// The block number of the account
        /// </summary>
        public uint BlockNumber { get; internal set; }

        public Account()
        {

        }

        public Account(Stream s)
        {
            LoadFromStream(s);
        }

        public void SaveToStream(BinaryWriter bw, bool writeLengths = true)
        {
            bw.Write(AccountNumber);
            AccountInfo.SaveToStream(bw, writeLengths);
            bw.Write(Balance);
            bw.Write(UpdatedBlock);
            bw.Write(NumberOfOperations);
            Name.SaveToStream(bw, writeLengths);
            bw.Write(AccountType);
            if (writeLengths) bw.Write(UpdatedByBlock);
        }

        public void LoadFromStream(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                AccountNumber = br.ReadUInt32();
                AccountInfo = AccountInfo.CreateFromStream(br);
                Balance = br.ReadUInt64();
                UpdatedBlock = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                Name = ByteString.ReadFromStream(br);
                AccountType = br.ReadUInt16();
                UpdatedByBlock = br.ReadUInt32();
            }
        }

        public bool Equals(Account other)
        {
            if (other.AccountNumber != AccountNumber) return false;
            if (other.AccountType != AccountType) return false;
            if (other.Balance != Balance) return false;
            if (other.Name != Name) return false;
            if (other.NumberOfOperations != NumberOfOperations) return false;
            if (other.UpdatedBlock != UpdatedBlock) return false;
            //if (other.UpdatedByBlock != UpdatedByBlock) return false;
            return other.AccountInfo.Equals(AccountInfo);            
        }
    }
}
