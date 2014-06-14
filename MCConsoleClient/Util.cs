using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace MCConsoleClient
{
    static class Util
    {
        // Minecraft: Packetbuffer.java -> readVarIntFromBuffer();
        public static int readVarInt()
        {
            int var1 = 0;
            int var2 = 0;
            byte var3;
            byte[] t = new byte[4];
            do
            {
                Program.sock.Receive(t, 1, SocketFlags.None);
                var3 = t[0];
                var1 |= (var3 & 127) << var2++ * 7;

                if (var2 > 5)
                {
                    return -1;
                }
            }
            while ((var3 & 128) == 128);

            return var1;
        }

        // Minecraft: Packetbuffer.java -> writeVarIntToBuffer()
        public static byte[] toVarInt(int i)
        {
            List<byte> ret = new List<byte>();
            while ((i & -128) != 0)
            {
                ret.Add((byte)(i & 127 | 128));
                i = (i >> 7);
            }
            ret.Add((byte)i);
            return ret.ToArray();
        }

        /*public static int readInt()
        {
            byte[] t = new byte[4];
            Program.sock.Receive(t, 1, SocketFlags.None);
            return BitConverter.ToInt32(t, 0);
        }*/

        public static string readString()
        {
            int length = readVarInt();
            byte[] t;
            if (length > 0 && length < 32768)
            {
                t = new byte[length];
                Program.sock.Receive(t, length, SocketFlags.None);
                return Encoding.UTF8.GetString(t);
            }
            return "";
        }

        public static void parseJSONMessage()
        {
            //TODO
        }


        public static string loginSession(string username, string email, string password)
        {
            try
            {
                WebClient w = new WebClient();
                w.Headers.Add("Content-Type: application/json");

                string json = "{\"agent\": { \"name\": \"Minecraft\", \"version\": 1 }, \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";

                string r = w.UploadString("https://authserver.mojang.com/authenticate", json);
                int i = r.IndexOf("\"id\":\"") + 6;
                int i_ = r.IndexOf("\"accessToken\":\"") + 15;
                string uuid = r.Substring(i, r.IndexOf("\"", i + 1) - i);
                string accessToken = r.Substring(i_, r.IndexOf("\"", i_ + 1) - i_);
                Program.uuid = uuid;
                Program.accessToken = accessToken;
                return uuid;
            }
            catch (WebException)
            {
                try
                {
                    WebClient w_ = new WebClient();
                    w_.Headers.Add("Content-Type: application/json");

                    string json = "{\"agent\": { \"name\": \"Minecraft\", \"version\": 1 }, \"username\": \"" + email + "\", \"password\": \"" + password + "\" }";

                    string r = w_.UploadString("https://authserver.mojang.com/authenticate", json);
                    int i = r.IndexOf("\"id\":\"") + 6;
                    int i_ = r.IndexOf("\"accessToken\":\"") + 15;
                    string uuid = r.Substring(i, r.IndexOf("\"", i + 1) - i);
                    string accessToken = r.Substring(i_, r.IndexOf("\"", i_ + 1) - i_);
                    Program.uuid = uuid;
                    Program.accessToken = accessToken;
                    return uuid;
                }
                catch (WebException)
                {
                    // k den
                    return "";
                }
            }
        }

        public static string joinServerSession(string serverHash)
        {
            if (Program.accessToken == "")
            {
                Console.WriteLine("Failed to login.");
                return "";
            }
            try
            {
                WebClient wClient = new WebClient();
                wClient.Headers.Add("Content-Type: application/json");

                var json = "{\"accessToken\":\"" + Program.accessToken + "\",\"selectedProfile\":\"" + Program.uuid + "\",\"serverId\":\"" + serverHash + "\"}";

                return wClient.UploadString("https://sessionserver.mojang.com/session/minecraft/join", json);
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                return "";
            }
        }
    }
}
