using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroCoin.Chain;
using MicroCoin.Net;
using MicroCoin.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroCoin.Mining
{
    public class MinerServer
    {
        public event EventHandler<EventArgs> NewMinerClient;
        protected TcpListener TcpListener { get; set; }
        public List<TcpClient> Clients { get; set; } = new List<TcpClient>();        
        public bool Stop { get; set; }
        public MinerServer()
        {
            TcpListener = new TcpListener(IPAddress.Any, 4009);
        }

        public void HandleClient(TcpClient client)
        {
            try
            {
                try
                {
                    while (true)
                    {
                        Block block = BlockChain.Instance.NextBlock("GPUMINE", Node.Instance.NodeKey);
                        var header = block.GetBlockHeaderForHash();
                        JObject message = new JObject
                        {
                            new JProperty("method", "miner-notify"),
                            new JProperty("id", null)
                        };
                        Hash h = header.MinerPayload;
                        JObject jObject = new JObject
                        {
                            new JProperty("block", block.BlockNumber),
                            new JProperty("part1", (string) header.Part1),
                            new JProperty("part3", (string) header.Part3),
                            new JProperty("payload_start", (string) h),
                            new JProperty("version", 2),
                            new JProperty("timestamp", (uint) block.Timestamp),
                            new JProperty("target", block.CompactTarget),
                            new JProperty("target_pow", (string) BlockChain.TargetFromCompact(block.CompactTarget))
                        };
                        var param = new JArray
                        {
                            jObject
                        };
                        message.Add(new JProperty("params", param));
                        using (var ms = new MemoryStream())
                        {
                            ByteString json = message.ToString(Formatting.None) + Environment.NewLine;
                            ms.Write(json, 0, json.Length);
                            ms.Position = 0;
                            ms.CopyTo(client.GetStream());
                            client.GetStream().Flush();
                        }

                        int timeout = 5000;
                        int i = 0;
                        while (client.Available == 0)
                        {
                            Thread.Sleep(1);
                            if (Stop) return;
                            i++;
                            if (i > timeout) break;
                        }

                        if (client.Available > 0)
                        {
                            ByteString b = new byte[client.Available];
                            client.GetStream().Read(b, 0, client.Available);
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
            finally
            {
                Clients.Remove(client);
                client.Dispose();
            }
        }

        public void Start()
        {
            new Thread(() =>
            {
                try
                {
                    TcpListener.Start();
                    var connected = new ManualResetEvent(false);
                    while (!Stop)
                    {
                        connected.Reset();
                        TcpListener.BeginAcceptTcpClient(state =>
                        {
                            try
                            {
                                var client = TcpListener.EndAcceptTcpClient(state);
                                Clients.Add(client);
                                NewMinerClient?.Invoke(this, new EventArgs());
                                new Thread(() =>
                                {
                                    HandleClient(client);
                                }).Start();
                                connected.Set();
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        }, null);
                        while (!connected.WaitOne(1))
                        {
                            if (Stop) break;
                        }
                    }

                    TcpListener.Stop();
                }
                finally
                {
                    foreach (var client in Clients)
                    {
                        client.Dispose();
                    }
                }
            })
            {
                Name="MinerServer"
            }.Start();
        }

    }
}
