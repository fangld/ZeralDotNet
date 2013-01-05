using System;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Global class
    /// </summary>
    public class Globals
    {
        #region Const Variables

        /// <summary>
        /// The header of protocol
        /// </summary>
        public static readonly byte[] ProtocolHeader;

        /// <summary>
        /// The length of sha1 hash value
        /// </summary>
        public const int Sha1HashLength = 20;

        /// <summary>
        /// The length of protocol header
        /// </summary>
        public const byte ProtocolHeaderLength = 19;

        /// <summary>
        /// The random generator
        /// </summary>
        public static readonly Random RandomGenerator;

        ///// <summary>
        ///// The SHA1 hasher
        ///// </summary>
        //public static readonly SHA1 sha1;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static Globals()
        {
                                       //'B', 'i', 't', 'T', 'o', 'r', 'r', 'e', 'n', 't', ' ', 'p', 'r', 'o', 't', 'o', 'c', 'o', 'l'
            ProtocolHeader = new byte[] { 66, 105, 116,  84, 111, 114, 114, 101, 110, 116,  32, 112, 114, 111, 116, 111,  99, 111, 108 };
            RandomGenerator = new Random();
            //sha1 = new SHA1CryptoServiceProvider();
        }

        #endregion

        #region Method

        public static byte[] GetSha1Hash(byte[] bytes)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] result;
            result = sha1.ComputeHash(bytes);
            sha1.Dispose();
            return result;
        }

        public static bool IsHashEqual(byte[] firstHash, byte[] secondHash, int length)
        {
            if (firstHash.Length == length && secondHash.Length == length)
            {
                for (int i = 0; i < length; i++)
                {
                    if (firstHash[i] != secondHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine the element of two array is equal
        /// </summary>
        /// <param name="firstArray">the first array</param>
        /// <param name="secondArray">the second array</param>
        /// <param name="length">the length of array</param>
        /// <returns>Return the element of two array is equal</returns>
        public static bool IsArrayEqual<T>(T[] firstArray, T[] secondArray, int length) where T : IEquatable<T>
        {
            if (firstArray.Length == length && secondArray.Length == length)
            {
                for (int i =0; i < length;i++)
                {
                    if (!firstArray[i].Equals(secondArray[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
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
        /// <param name="destination">写入的数组</param>
        public static void CopyBytes(byte[] source, int sourceOffset, byte[] destination)
        {
            int index;
            for (index = sourceOffset; index < source.Length; index++)
            {
                destination[index - sourceOffset] = source[index];
            }
        }

        /// <summary>
        /// 复制数组
        /// </summary>
        /// <param name="source">被复制的数组</param>
        /// <param name="destination">写入的数组</param>
        /// <param name="destinationOffset">写入数组的偏移位置</param>
        public static void CopyBytes(byte[] source, byte[] destination,int destinationOffset)
        {
            int index;
            for (index = 0; index < source.Length; index++)
            {
                destination[index + destinationOffset] = source[index];
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
            int result = (buffer[startOffset] << 24);
            result |= (buffer[++startOffset] << 16);
            result |= (buffer[++startOffset] << 8);
            result |= buffer[++startOffset];
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
