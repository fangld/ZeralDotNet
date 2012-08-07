using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent
{
    public static class StringExtension
    {
        public static string ToHexString(this byte[] infoHash)
        {
            StringBuilder sb = new StringBuilder((infoHash.Length << 1));
            for (int i = 0; i < infoHash.Length; i++)
            {
                sb.AppendFormat("{0:X2}", infoHash[i]);
            }
            return sb.ToString();
        }

        public static string ToUrlEncodedFormat(this string infoHashHexString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < infoHashHexString.Length; i += 2)
            {
                byte b = (byte)((GetHexDecimal(infoHashHexString[i]) << 4) + (GetHexDecimal(infoHashHexString[i + 1])));

                if ((b >= '0' && b <= '9') || (b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || b == '.' || b == '-' || b == '_' || b == '~')
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", b);
                }
            }
            return sb.ToString();
        }

        public static byte[] ToHashBytes(this string infoHash)
        {
            byte[] result = new byte[(infoHash.Length >> 1)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte) ((GetHexDecimal(infoHash[(i << 1)]) << 4) + (GetHexDecimal(infoHash[(i << 1) + 1])));
            }
            return result;
        }

        public static byte[] ToPeerIdBytes(this string peerId)
        {
            byte[] result = new byte[peerId.Length];
            Parallel.For(0, peerId.Length, i => result[i] = (byte)peerId[i]);
            return result;
        }

        private static byte GetHexDecimal(char hexChar)
        {
            byte result = 0;
            if (hexChar >= '0' && hexChar <= '9')
            {
                result = (byte)(hexChar - '0');
            }
            else if (hexChar >= 'a' && hexChar <= 'z')
            {
                result = (byte)(hexChar - 87);
            }
            else if (hexChar >= 'A' && hexChar <= 'Z')
            {
                result = (byte)(hexChar - 55);
            }
            return result;
        }
    }
}
