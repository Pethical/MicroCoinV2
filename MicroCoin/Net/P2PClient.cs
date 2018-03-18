﻿using log4net;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MicroCoin.Net
{
    public abstract class P2PClient : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected Thread ListenerThread;
        protected bool Stop;
        internal virtual event EventHandler Disconnected;
        internal static ushort ServerPort { get; set; }
        internal TcpClient TcpClient { get; set; }
        internal bool Connected { get; set; }
        internal bool IsDisposed { get; set; }

        protected int WaitForData(int timeoutMs)
        {
            while (TcpClient.Available == 0)
            {
                Thread.Sleep(1);
            }
            return TcpClient.Available;
        }

        internal bool Connect(string hostname, int port)
        {
            try
            {
                TcpClient = new TcpClient();                
                var result = TcpClient.BeginConnect(hostname, port, null, null);
                Connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                TcpClient.EndConnect(result);                
                if (!Connected)
                {
                    throw new Exception("");
                }
                Log.Info($"Connected to {hostname}:{port}");
            }
            catch (Exception e)
            {
                if (TcpClient != null)
                {
                    TcpClient.Dispose();
                    TcpClient = null;
                }
                Connected = false;
                Log.Debug($"Can't connect to {hostname}:{port}. {e.Message}");
                TcpClient = null;
                return false;
            }
            return Connected;
        }

        internal void Handle(TcpClient client)
        {
            Log.Info($"Connected client {client.Client.RemoteEndPoint}");
            TcpClient = client;
            Connected = true;
            Start();
        }

        public void Dispose()
        {
            Disconnected?.Invoke(this, new EventArgs());
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        private void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                Stop = true;
                while(ListenerThread!=null && ListenerThread.IsAlive)
                {                    
                    Thread.Sleep(1);
                }

                TcpClient?.Dispose();
            }
            IsDisposed = true;
        }

        protected bool ReadData(int requiredSize, MemoryStream ms)
        {
            int wt = 0;
            while (requiredSize > ms.Length)
            {
                do
                {
                    Thread.Sleep(1);
                    wt++;
                    if (wt > 10000) break;
                } while (TcpClient.Available == 0);

                if (wt > 10000) break;
                ReadAvailable(ms);
                /*while (TcpClient.Available > 0)
                {
                    byte[] buffer = new byte[TcpClient.Available];
                    ns.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, buffer.Length);
                }
                */
                wt = 0;
            }

            return wt > 10000;
        }

        protected void ReadAvailable(MemoryStream ms)
        {
            NetworkStream ns = TcpClient.GetStream();
            while (TcpClient.Available > 0)
            {
                byte[] buffer = new byte[TcpClient.Available];
                ns.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, buffer.Length);
            }
        }

        protected bool WaitForPacket()
        {
            while (TcpClient.Available == 0)
            {
                if (Stop)
                {
                    return true;
                }

                Thread.Sleep(1);
            }

            return false;
        }

        internal void SendRaw(Stream stream)
        {
            if (!TcpClient.Connected) return;
            NetworkStream ns = TcpClient.GetStream();
            stream.Position = 0;
            stream.CopyTo(ns);
            ns.Flush();
        }

        protected abstract bool HandleConnection();

        internal virtual void Start()
        {
            ListenerThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        if (WaitForPacket()) return;
                        if (HandleConnection()) return;
                    }
                }
                finally
                {
                    TcpClient.Close();
                }
            }) {Name = TcpClient.Client.RemoteEndPoint.ToString()};
            ListenerThread.Start();            
        }
    }
}