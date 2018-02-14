using MicroCoin.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.BlockChain
{

    public class Account
    {
        public uint AccountNumber { get; set; }
        public AccountInfo AccountInfo { get; set; }
        public ulong Balance { get; set; }
        public uint UpdatedByBlock { get; set; }
        public uint NumberOfOperations { get; set; }
        public ByteString Name { get; set; }
        public ushort AccountType { get; set; }
        public uint UpdatedBlock { get; set; }
        public Account()
        {
            
        }

        public Account(Stream s)
        {
            LoadFromStream(s);
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
                UpdatedBlock = br.ReadUInt32();
            }
        }

    }
}
