using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCC
{
    public enum DiscoveryCommand { HelloRequest, HelloResponse, NodeListResponse, NodeListRequest };
    public class DiscoveryMessage
    {
        public DiscoveryCommand Command { get; set; }
        public ushort PayloadLength
        {
            get
            {
                if(Payload!=null)
                    return (ushort) Payload.Length;
                return 0;
            }
        }
        public byte[] Payload { get; set; }
        public int Length
        {
            get
            {
                return PayloadLength + sizeof(int) +sizeof(ushort);
            }
        }

        public DiscoveryMessage() { }

        public DiscoveryMessage(byte[] data)
        {
            int dt = (data[3] << 24 | data[2] << 16 | data[1] << 8 | data[0]);
            Command = (DiscoveryCommand)dt;
            ushort PayloadLength =  (ushort)((data[4] << 8) | (data[2]));
            Payload = data.Skip(6).ToArray();
        }

        public byte[] ToByteArray()
        {            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((int)Command);
                    bw.Write(PayloadLength);
                    if (PayloadLength > 0)
                    {
                        bw.Write(Payload, 0, Payload.Length);
                    }
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }
        public override string ToString() => Payload==null?"":Encoding.ASCII.GetString(Payload);
        public static DiscoveryMessage fromString(DiscoveryCommand Command, string message)
        {
            return new DiscoveryMessage
            {
                Payload = Encoding.UTF8.GetBytes(message),
                Command = Command
            };
        }
    }

    public class Discovery
    {
        private readonly UdpClient udp;
        
        private string[] fixSeedIPs = { "185.33.146.44" };
        public List<IPEndPoint> endPoints { get; set; } = new List<IPEndPoint>();
        private object lockObject = new object();
        public Discovery()
        {
            try
            {
                udp = new UdpClient(new IPEndPoint(IPAddress.Any, 15000));
            }
            catch
            {
                udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0));       
                
            }
            StartListening();
            Discover();
            
        }

        public List<IPEndPoint> GetEndPoints()
        {
            lock (lockObject)
            {
                return new List<IPEndPoint>(endPoints);                
            }
        }

        private void StartListening()
        {
            udp.AllowNatTraversal(true);            
            udp.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {          
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 15000);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            DiscoveryMessage Message = new DiscoveryMessage(bytes);
            DiscoveryMessage Response = new DiscoveryMessage();
            string message = Message.ToString();
            if (Message.Command==DiscoveryCommand.HelloRequest)
            {
                // byte[] send = Encoding.ASCII.GetBytes("server");
                Console.WriteLine("Server response");
                Response.Command = DiscoveryCommand.NodeListRequest;
                Response.Payload = null;
                udp.Send(Response.ToByteArray(), Response.Length, ip);
            }
            if (Message.Command == DiscoveryCommand.HelloResponse)
            {
                Console.WriteLine("Request nodelist");
                Response.Command = DiscoveryCommand.NodeListRequest;
                Response.Payload = null;
                udp.Send(Response.ToByteArray(), Response.Length, ip);
            }
            if (Message.Command == DiscoveryCommand.NodeListRequest)
            {
                Console.WriteLine("Send nodelist");
                using (MemoryStream m = new MemoryStream())
                {
                    //SslStream stream = new SslStream(m);
                    StreamWriter sw = new StreamWriter(m);                    
                    foreach (var p in GetEndPoints().ToArray()) {                        
                        sw.Write(p.Address.ToString());
                        sw.Write(':');
                        sw.Write(p.Port);
                        sw.Write(';');
                    }
                    sw.Flush();
                    Response.Command = DiscoveryCommand.NodeListResponse;
                    Response.Payload = m.ToArray();
                    udp.Send(Response.ToByteArray(), Response.Length, ip);
                    sw.Dispose();
                }
            }
            else if (Message.Command == DiscoveryCommand.NodeListResponse)
            {
                Console.WriteLine("Got nodelist");
                lock (lockObject)
                {
                    string[] ips = Message.ToString().Split(';');
                    foreach (string p in ips)
                    {
                        if (p == "") continue;
                        Console.WriteLine("Node: " + p);
                        string[] ps = p.Split(':');
                        if (endPoints.Count(p1 => p1.Address.ToString() == ps[0] && p1.Port==Convert.ToInt32(ps[1]))==0)
                        {
                            endPoints.Add(new IPEndPoint(IPAddress.Parse(ps[0]), Convert.ToInt32(ps[1])));
                        }
                    }
                }
            }
            else
            {                
                Console.WriteLine(message);
            }
            lock (lockObject)
            {
                if (endPoints.Count(p => p.Address.ToString() == ip.Address.ToString() && p.Port == ip.Port) == 0)
                {
                    endPoints.Add(ip);
                }
            }

            StartListening();
        }

        public void Discover()
        {
            lock (lockObject)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 15000);
                DiscoveryMessage Response = new DiscoveryMessage();
                Response.Command = DiscoveryCommand.HelloRequest;
                foreach (var seed in fixSeedIPs)
                {

                    ip = new IPEndPoint(IPAddress.Parse(seed), 15000);
                    Response.Command = DiscoveryCommand.HelloRequest;                                        
                    udp.Send(Response.ToByteArray(), Response.Length, ip);
                }
                foreach (var point in endPoints.ToArray())
                {
                    Response.Command = DiscoveryCommand.HelloRequest;
                    udp.Send(Response.ToByteArray(), Response.Length, point);
                }
                endPoints.Clear();
            }
        }

    }
}
