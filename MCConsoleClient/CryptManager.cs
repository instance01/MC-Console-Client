using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Security;

namespace MCConsoleClient
{
    class CryptManager
    {
        public static byte[] generateAESKey()
        {
            AesManaged AES = new AesManaged();
            AES.KeySize = 128;
            AES.GenerateKey();
            return AES.Key;
        }

        public static string getServerHash(string serverID, byte[] PublicKey, byte[] SecretKey)
        {
            byte[] serverid_raw = Encoding.GetEncoding("iso-8859-1").GetBytes(serverID);
            byte[] serverhash;
            using(SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                sha1.TransformBlock(serverid_raw, 0, serverid_raw.Length, serverid_raw, 0);
                sha1.TransformBlock(PublicKey, 0, PublicKey.Length, PublicKey, 0);
                sha1.TransformBlock(SecretKey, 0, SecretKey.Length, SecretKey, 0);
                sha1.TransformFinalBlock(new byte[] { }, 0, 0);
                serverhash = sha1.Hash;
            }
            bool negative = (serverhash[0] & 0x80) == 0x80;
            if (negative)
                serverhash = TwosCompliment(serverhash);
            string digest = GetHexString(serverhash).TrimStart('0');
            if (negative)
                digest = "-" + digest;
            return digest;
        }

        private static string JavaHexDigest(string data)
        {
            var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
            bool negative = (hash[0] & 0x80) == 0x80;
            if (negative) // check for negative hashes
            hash = TwosCompliment(hash);
            // Create the string and trim away the zeroes
            string digest = GetHexString(hash).TrimStart('0');
            if (negative)
            digest = "-" + digest;
            return digest;
        }
 
        private static string GetHexString(byte[] p)
        {
            string result = string.Empty;
            for (int i = 0; i < p.Length; i++)
            result += p[i].ToString("x2"); // Converts to hex string
            return result;
        }

        private static byte[] TwosCompliment(byte[] p) // little endian
        {
            int i;
            bool carry = true;
            for (i = p.Length - 1; i >= 0; i--)
            {
                p[i] = (byte)~p[i];
                if (carry)
                {
                    carry = p[i] == 0xFF;
                    p[i]++;
                }
            }
            return p;
        }


    }
}
