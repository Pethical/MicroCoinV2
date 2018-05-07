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

        public static List<string> FixSeeds = new List<string>
        {
            "185.28.101.93",
            "80.211.211.48",
            "94.177.237.196",
            "185.33.146.44",
            "80.211.200.121"
        };
    }
}
