﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;
using MicroCoin.Chain;
using MicroCoin.Protocol;
using MicroCoin.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroCoin.Mining
{
    public class MinerServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<EventArgs> NewMinerClient;
        protected TcpListener TcpListener { get; set; }
        public List<TcpClient> Clients { get; set; } = new List<TcpClient>();        
        public bool Stop { get; set; }
        public MinerServer()
        {
            TcpListener = new TcpListener(IPAddress.Any, Node.NetParams.MinerPort);
        }

        public void HandleClient(TcpClient client)
        {
            try
            {
                try
                {
                    while (true)
                    {                        
                        Block block = BlockChain.Instance.NextBlock("GPUMINE---", Node.Instance.NodeKey);                        
                        
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
                            Log.InfoFormat("Sending \"{0}\" to miner", json);
                            ms.Write(json, 0, json.Length);
                            ms.Position = 0;
                            ms.CopyTo(client.GetStream());
                            client.GetStream().Flush();
                            
                        }

                        int timeout = 60000;
                        int i = 0;
                        while (client.Available == 0)
                        {
                            Thread.Sleep(1);
                            if (Stop) return;
                            i++;
                            if (i > timeout) break;
                        }
                        using (MemoryStream buffer = new MemoryStream())
                        {
                            while (client.Available > 0)
                            {                                
                                byte[] b = new byte[client.Available];
                                client.GetStream().Read(b, 0, client.Available);
                                buffer.Write(b, 0, b.Length);
                            }
                            if (buffer.Length > 0)
                            {
                                ByteString response = buffer.ToArray();
                                Log.Info(response);
                                JObject minerResponse = JObject.Parse(response);
                                if (minerResponse.Value<string>("method") == "miner-submit")
                                {
                                    var br = new NewBlockRequest();
                                    br.Block = block;
                                    br.Block.Nonce = minerResponse.Value<JArray>("params").Value<JObject>(0).Value<int>("nonce");
                                    br.Block.Timestamp = minerResponse.Value<JArray>("params").Value<JObject>(0).Value<uint>("timestamp");
                                    br.Block.ProofOfWork = br.Block.CalcProofOfWork();
                                    if (!br.Block.ProofOfWorkIsValid())
                                    {
                                        Log.Warn("Received invalid solution for block");
                                        continue;
                                    }
                                    byte[] targetPow = BlockChain.TargetFromCompact(block.CompactTarget);
                                    byte[] pow = block.ProofOfWork;
                                    i = 0;
                                    bool found = false;
                                    foreach (byte c in pow)
                                    {
                                        i++;
                                        if ((byte)c == (byte)targetPow[i]) continue;
                                        if ((byte)c < (byte)targetPow[i])
                                        {
                                            found = true;
                                            break;
                                        }
                                        break;
                                    }
                                    if (!found) {
                                        Log.Warn("Received invalid solution for block. PoW not valid");
                                        continue;
                                    }
                                    Log.Info("Received valid solution for block");
                                    BlockChain.Instance.AppendAll( new List<Block>() { br.Block });
                                    CheckPoints.AppendBlock(br.Block);                                    
                                    Node.Instance.SendNewBlock(br.Block);
                                    Node.Instance.PendingTransactions.Clear();
                                }
                                else
                                {

                                }
                            }
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
                                var minerThread = new Thread(() =>
                                {
                                    HandleClient(client);
                                });
                                minerThread.Name = "miner " + client.Client.RemoteEndPoint.ToString();
                                Log.InfoFormat("Connected miner {0}", minerThread.Name);
                                minerThread.Start();
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
