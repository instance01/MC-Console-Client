using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace MCConsoleClient
{
    class Program
    {
        public static Socket sock;
        public static Thread thread;

        static void Main(string[] args)
        {
            bool menu = true;
            while (menu)
            {
                Console.Write("Server > ");
                String address = Console.ReadLine();
                if (address == "")
                {
                    address = "127.0.0.1";
                }
                Console.Write("Port > ");
                String portstr = Console.ReadLine();
                if (portstr == "")
                {
                    portstr = "25565";
                }
                connect(address, Convert.ToInt32(portstr));
                getServerInfo(address, Convert.ToInt32(portstr));
                Console.Write("Connect? (y/n) ");
                if (Console.ReadLine().ToLower().StartsWith("y"))
                {
                    // connect
                    Console.Write("Username > ");
                    String username = Console.ReadLine();
                    login(address, Convert.ToInt32(portstr), username);
                    while(sock.Connected){
                        // send message
                        Packets.chatMessage0x01(Console.ReadLine());
                    }
                }
            }
        }

        static void connect(String server, int port)
        {
            IPAddress ip = IPAddress.Parse(server); // TODO add domains and IPv6 support
            IPEndPoint ipEndpoint = new IPEndPoint(ip, port);
            sock = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(ipEndpoint);
        }

        static void getServerInfo(string address, int port)
        {
            Packets.handshake0x00(address, port, 1);
            Packets.request0x00();
            Packets.response0x00();
            sock.Close();
        }

        static void login(string address, int port, string username)
        {
            connect(address, port);
            Packets.handshake0x00(address, port, 2);
            Packets.loginStart0x00(username);
            if (Packets.loginResponse())
            {
                thread = new Thread(new ThreadStart(handleUpdates));
                thread.Start();
            }
        }

        static void handleUpdates()
        {
            while (true)
            {
                Thread.Sleep(50);
                tick();
            }
        }

        static void tick()
        {
            if (!sock.Connected)
            {
                Console.WriteLine("Connection failed.");
                thread.Abort();
                return;
            }
            while (sock.Available > 0)
            {
                // receive packets and respond if needed (keep-alive)
                int length = Util.readVarInt();
                int id = Util.readVarInt();
                if (id == 0)
                {
                    // keep-alive
                    Packets.keepalive0x00();
                }
                else if (id == 2)
                {
                    // chat message
                    Console.WriteLine(Util.readString());
                }
                else
                {
                    // everything else (this includes chunk updates and all that stuff we don't need in a console)
                    if (length > 0 && length < 32768)
                    {
                        byte[] buff = new byte[length + 1];
                        sock.Receive(buff, length, SocketFlags.None);
                    }
                }

            }
        }
    }
}
