using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

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
    }
}
