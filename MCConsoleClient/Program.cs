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
        public static string accessToken = "";
        public static string uuid = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Usage example: 5.104.104.45 25565 InstanceLabs password");
            Console.WriteLine("Leave password empty for offline mode.");
            bool cmd = false;
            String address = "127.0.0.1";
            String portstr = "25565";
            int port = 25565;
            String username = "";
            String password = "";
            if (args.Length > 3)
            {
                address = args[0];
                portstr = args[1];
                port = Convert.ToInt32(portstr);
                username = args[2];
                password = args[3];
                cmd = true;
            }else if(args.Length > 2 && args.Length < 4){
                address = args[0];
                portstr = args[1];
                port = Convert.ToInt32(portstr);
                username = args[2];
                password = "";
                cmd = true;
            }
            bool menu = true;
            while (menu)
            {
                if (cmd)
                {
                    login(address, port, username, password);
                    while (sock.Connected)
                    {
                        // send message
                        Packets.chatMessage0x01(Console.ReadLine());
                    }
                    return;
                }
                Console.Write("Server > ");
                address = Console.ReadLine();
                if (address == "")
                {
                    address = "127.0.0.1";
                }
                Console.Write("Port > ");
                portstr = Console.ReadLine();
                if (portstr == "")
                {
                    portstr = "25565";
                }
                port = Convert.ToInt32(portstr);
                connect(address, port);
                getServerInfo(address, port);
                Console.Write("Connect? (y/n) ");
                if (Console.ReadLine().ToLower().StartsWith("y"))
                {
                    // connect
                    Console.Write("Username > ");
                    username = Console.ReadLine();
                    Console.Write("EMail (leave empty if you still have an unmigrated account) > ");
                    string email = Console.ReadLine();
                    Console.Write("Passord (leave empty for offline mode) > ");
                    password = Console.ReadLine();
                    Console.WriteLine(Util.loginSession(username, email, password));
                    login(address, port, username, password);
                    while(sock.Connected){
                        // send message
                        Packets.chatMessage0x01(Console.ReadLine());
                    }
                }
            }
        }

        static void connect(String server, int port)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(server, out ip))
            {
                ip = Dns.GetHostEntry(server).AddressList[0];
            }
            else
            {
                ip = IPAddress.Parse(server);
            }
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

        static void login(string address, int port, string username, String password)
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

        static void loginSession()
        {

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
