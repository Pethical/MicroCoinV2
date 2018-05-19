using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin
{
    public class MainParams
    {
        public static ushort Port { get; set; } = 4004;
        public static string GenesisPayload { get; internal set; } = "(c) Peter Nemeth - Okes rendben okes";
        public static int MinerPort { get; internal set; } = 4009;

        public static string DataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MicroCoinV2";

        public static string CheckPointIndexName {
            get {
                return System.IO.Path.Combine(DataDirectory, "checkpoints.idx");
            }
        }
        public static string CheckPointFileName
        {
            get
            {
                return System.IO.Path.Combine(DataDirectory, "checkpoints.mcc");
            }
        }
        public static string BlockChainFileName
        {
            get
            {
                return System.IO.Path.Combine(DataDirectory, "block.chain");
            }
        }

        public static string KeysFileName {
            get
            {                
                return System.IO.Path.Combine(DataDirectory, "WalletKeys.dat");                
            }
        }
        public static List<string> FixSeeds = new List<string>
        {
            "185.28.101.93",
            "80.211.200.121",
            "80.211.211.48",
            "94.177.237.196",
            "185.33.146.44",
        };
    }
}
