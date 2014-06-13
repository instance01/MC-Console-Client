using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace MCConsoleClient
{
    public class Packets
    {
        // handshake (used for server status and for joining the server)
        public static void handshake0x00(String address, int port, int state)
        {
            List<byte> rawdata = new List<byte>();
            rawdata.AddRange(Util.toVarInt(0)); // packetid
            rawdata.AddRange(Util.toVarInt(4)); // protocol version
            rawdata.AddRange(Util.toVarInt(address.Length));
            rawdata.AddRange(Encoding.UTF8.GetBytes(address));
            rawdata.AddRange(BitConverter.GetBytes((ushort)port));
            rawdata.AddRange(Util.toVarInt(state));
            List<byte> data = new List<byte>();
            data.AddRange(Util.toVarInt(rawdata.ToArray().Length));
            data.AddRange(rawdata);
            Program.sock.Send(data.ToArray());
        }

        // server status request
        public static void request0x00()
        {
            List<byte> rawdata = new List<byte>();
            rawdata.AddRange(Util.toVarInt(0)); // packetid
            List<byte> data = new List<byte>();
            data.AddRange(Util.toVarInt(rawdata.ToArray().Length));
            data.AddRange(rawdata);
            Program.sock.Send(data.ToArray());
        }

        // server status response
        public static void response0x00()
        {
            int j = Util.readVarInt();
            if (j > 0)
            {
                int i = Util.readVarInt();
                if (i == 0)
                {
                    Console.WriteLine(Util.readString());
                }
            }
        }

        // try to login as username
        public static void loginStart0x00(string username)
        {
            List<byte> rawdata = new List<byte>();
            rawdata.AddRange(Util.toVarInt(0)); // packetid
            rawdata.AddRange(Util.toVarInt(username.Length));
            rawdata.AddRange(Encoding.UTF8.GetBytes(username));
            List<byte> data = new List<byte>();
            data.AddRange(Util.toVarInt(rawdata.ToArray().Length));
            data.AddRange(rawdata);
            Program.sock.Send(data.ToArray());
        }

        // response from server after trying to login as in loginStart0x00 given username
        public static bool loginResponse()
        {
            Util.readVarInt();
            int def = Util.readVarInt();
            if (def == 0) // 0x00 Disconnect
            {
                // server sent disconnect packet
                Console.WriteLine("Disconnected.");
                Console.WriteLine(Util.readString());
                return false;
            }
            else if (def == 1) // 0x01 Encryption Request 
            {
                // premium, asks for mojang key!
                // TODO
                return true;
            }
            else if (def == 2) // 0x02 Login Success 
            {
                // seems like an offline server, sent success packet
                Console.WriteLine("Successfully connected.");
                return true;
            }
            Console.WriteLine("Could not login into server. Status: " + Convert.ToString(def));
            return false;
        }

        // keep-alive needs to be responded to in order to not be kicked
        public static void keepalive0x00()
        {
            byte[] keepalive = new byte[4];
            Program.sock.Receive(keepalive, 4, SocketFlags.None);

            List<byte> rawdata = new List<byte>();
            rawdata.AddRange(Util.toVarInt(0)); // packetid
            rawdata.AddRange(keepalive);
            List<byte> data = new List<byte>();
            data.AddRange(Util.toVarInt(rawdata.ToArray().Length));
            data.AddRange(rawdata);
            Program.sock.Send(data.ToArray());
        }

        // send a message to the server (includes commands)
        public static void chatMessage0x01(String msg)
        {
            if (msg.Length > 160)
            {
                return;
            }
            List<byte> rawdata = new List<byte>();
            rawdata.AddRange(Util.toVarInt(1)); // packetid
            rawdata.AddRange(Util.toVarInt(msg.Length));
            rawdata.AddRange(Encoding.UTF8.GetBytes(msg));
            List<byte> data = new List<byte>();
            data.AddRange(Util.toVarInt(rawdata.ToArray().Length));
            data.AddRange(rawdata);
            Program.sock.Send(data.ToArray());
        }
    }
}
