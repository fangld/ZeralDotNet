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
                for (i = 0; i < firstSHA1.Length; i++)
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
                for (i = 0; i < firstMD.Length; i++)
                    if (firstMD[i] != secondMD[i])
                        return false;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 复制数组
        /// </summary>
        /// <param name="source">被复制的数组</param>
        /// <param name="sourceOffset">被复制数组的偏移位置</param>
        /// <param name="target">写入的数组</param>
        public static void CopyBytes(byte[] source, int sourceOffset, byte[] target)
        {
            int position;
            for (position = sourceOffset; position < source.Length; position++)
            {
                target[position - sourceOffset] = source[position];
            }
        }

        /// <summary>
        /// 将32位有符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的32位有符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        public static void Int32ToBytes(int value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }

        /// <summary>
        /// 将字节流转换为32位有符号整数
        /// </summary>
        /// <param name="buffer">待读入的字节流</param>
        /// <param name="startOffset">待读入字节流的位置</param>
        /// <returns>返回32位有符号整数</returns>
        public static int BytesToInt32(byte[] buffer, int startOffset)
        {
            int result = 0x0;
            result |= ((int)buffer[startOffset]) << 24;
            result |= ((int)buffer[++startOffset]) << 16;
            result |= ((int)buffer[++startOffset]) << 8;
            result |= ((int)buffer[++startOffset]);
            return result;
        }
    }
}
