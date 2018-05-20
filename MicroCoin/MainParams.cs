using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin
{
    public class MainParams
    {
#if MICROCOIN
        public static ushort Port { get; set; } = 4004;
        public static int MinerPort { get; set; } = 4009;
        
#else
        public static ushort Port { get; set; } = 4010;
        public static int MinerPort { get; set; } = 4011;
        public static string GenesisBlockPayload { get; set; } = "GPUMINE---";
        public static uint GenesisBlockTimeStamp { get; set; } = 1526765918;
        public static int GenesisBlockNonce { get; set; } = -1049394533;
#endif
        public static uint MaxBlockInPacket { get; set; } = 10000;
        public static uint MinimumDifficulty = 0x19000000;
        public static uint NetworkPacketMagic { get; set; } = 0x0A043580;
        public static ushort NetworkProtocolVersion { get; set; } = 2;
        public static ushort NetworkProtocolAvailable { get; set; } = 3;
#if MICROCOIN
        public const string CoinName  = "MicroCoin";
        public static string CoinTicker { get; set; } = "MCC";
#else
        public const string CoinName  = "PussyCoin";
        public static string CoinTicker { get; set; } = "PYC";
#endif
        public static bool EnableCheckPointing { get; set; } = true;
        public static int CheckPointFrequency { get; set; } = 100; // blocks
        public static int BlockTime { get; set; } = 300;  // seconds
        public static int DifficultyAdjustFrequency { get; set; } = 100; // blocks
        public static int DifficultyCalcFrequency { get; set; } = 10; // blocks
        public static uint MinimumBlocksToUseAccount { get; set; } = 100; // blocks
        public static uint FreeTransactionsPerBlock { get; set; } = 1; // blocks
        public static string GenesisPayload { get; set; } = "(c) Peter Nemeth - Okes rendben okes";
        public static string DataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\"+ CoinName+"V2";
        public static string CheckPointIndexName { get; set; } = System.IO.Path.Combine(DataDirectory, "checkpoints.idx");
        public static string CheckPointFileName { get;set; } = System.IO.Path.Combine(DataDirectory, "checkpoints.mcc");
        public static string BlockChainFileName { get; set; } = System.IO.Path.Combine(DataDirectory, "block.chain");
        public static string KeysFileName { get; set; } = System.IO.Path.Combine(DataDirectory, "WalletKeys.dat");

        public static readonly List<string> FixedSeedServers = new List<string>
        {
            "185.28.101.93"
#if MICROCOIN
            ,
            "185.28.101.93",
            "80.211.200.121",
            "80.211.211.48",
            "94.177.237.196",
            "185.33.146.44",
#endif
        };
    }
}
