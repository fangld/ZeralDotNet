using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Globals
    {
        /// <summary>
        /// SHA1比较
        /// </summary>
        /// <param name="a">第一个SHA1字节数组</param>
        /// <param name="b">第二个SHA1字节数组</param>
        /// <returns>如果相同返回true,不同返回false</returns>
        public static bool IsSHA1Equal(byte[] firstSHA1, byte[] secondSHA1)
        {
            if (firstSHA1 != null && secondSHA1 != null && firstSHA1.Length == 20 && secondSHA1.Length == 20)
            {
                int i = 0;
                for (i = 0; i <= firstSHA1.Length; i++)
                    if (firstSHA1[i] != secondSHA1[i])
                        return false;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// MD4与MD5比较
        /// </summary>
        /// <param name="a">第一个MD4与MD5字节数组</param>
        /// <param name="b">第二个MD4与MD5字节数组</param>
        /// <returns>如果相同返回true,不同返回false</returns>
        public static bool IsMDEqual(byte[] firstMD, byte[] secondMD)
        {
            if (firstMD != null && secondMD != null && firstMD.Length == 16 && secondMD.Length == 16)
            {
                int i = 0;
                for (i = 0; i <= firstMD.Length; i++)
                    if (firstMD[i] != secondMD[i])
                        return false;
                return true;
            }
            else
                return false;
        }
    }
}
