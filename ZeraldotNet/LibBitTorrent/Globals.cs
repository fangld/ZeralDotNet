using System;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 全局函数类
    /// </summary>
    public class Globals
    {
        #region Const Variables

        /// <summary>
        /// 协议名头
        /// </summary>
        public static readonly byte[] protocolName;

        /// <summary>
        /// 协议名长度
        /// </summary>
        public const byte protocolNameLength = 19;

        /// <summary>
        /// 随机函数生成器
        /// </summary>
        public static readonly Random random;

        /// <summary>
        /// SHA1散列函数生成器
        /// </summary>
        public static readonly SHA1Managed sha1;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static Globals()
        {
                                     //'B', 'i', 't', 'T', 'o', 'r', 'r', 'e', 'n', 't', ' ', 'p', 'r', 'o', 't', 'o', 'c', 'o', 'l'
            protocolName = new byte[] { 66, 105, 116, 84, 111, 114, 114, 101, 110, 116, 32, 112, 114, 111, 116, 111, 99, 111, 108 };
            random = new Random();
            sha1 = new SHA1Managed();
        }

        #endregion

        #region Method
        /// <summary>
        /// SHA1比较
        /// </summary>
        /// <param name="firstSHA1">第一个SHA1字节数组</param>
        /// <param name="secondSHA1">第二个SHA1字节数组</param>
        /// <returns>如果相同返回true,不同返回false</returns>
        public static bool IsSHA1Equal(byte[] firstSHA1, byte[] secondSHA1)
        {
            if (firstSHA1 != null && secondSHA1 != null && firstSHA1.Length == 20 && secondSHA1.Length == 20)
            {
                for (int i = 0; i < firstSHA1.Length; i++)
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
        /// <param name="firstMD">第一个MD4与MD5字节数组</param>
        /// <param name="secondMD">第二个MD4与MD5字节数组</param>
        /// <returns>如果相同返回true,不同返回false</returns>
        public static bool IsMDEqual(byte[] firstMD, byte[] secondMD)
        {
            if (firstMD != null && secondMD != null && firstMD.Length == 16 && secondMD.Length == 16)
            {
                for (int i = 0; i < firstMD.Length; i++)
                    if (firstMD[i] != secondMD[i])
                        return false;
                return true;
            }
            else
                return false;
        }

        public static byte[] DeleteBytes(byte[] source, int offset)
        {
            int sourceLength = source.Length;
            byte[] tempBytes;
            tempBytes = new byte[sourceLength - offset];

            int sourceIndex, tempIndex;
            for (sourceIndex = offset, tempIndex = 0; sourceIndex < sourceLength; sourceIndex++, tempIndex++)
            {
                tempBytes[tempIndex] = source[sourceIndex];
            }
            return tempBytes;
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
        /// 复制数组
        /// </summary>
        /// <param name="source">被复制的数组</param>
        /// <param name="target">写入的数组</param>
        /// <param name="targetOffset">写入数组的偏移位置</param>
        public static void CopyBytes(byte[] source, byte[] target,int targetOffset)
        {
            int position;
            for (position = 0; position < source.Length; position++)
            {
                target[position + targetOffset] = source[position];
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

        /// <summary>
        /// 将16位无符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的16位无符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        public static void UInt16ToBytes(ushort value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 8);
            buffer[++startIndex] = (byte)(value & 0xFF);
        }

        public static ushort BytesToUInt16(byte[] buffer, int startOffset)
        {
            ushort result = 0x0;
            result |= ((ushort)buffer[startOffset]);
            result <<= 8;
            result |= ((ushort)buffer[++startOffset]);
            return result;
        }
        #endregion
    }
}
