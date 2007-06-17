using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class BitField
    {
        /// <summary>
        /// 将布尔数组转换为字节数组
        /// </summary>
        /// <param name="booleans">待转换的布尔数组</param>
        /// <returns>转换所得的字节数组</returns>
        public static byte[] ToBitField(bool[] booleans)
        {            
            //如果booleans数组等于零,返回空字节数组
            if (booleans.Length == 0)
                return new byte[0];

            //计算字节数组长度
            //numBytes = booleans / 8;
            int numBytes = booleans.Length >> 3;

            //(booleans.Length % 8) != 0
            if ((booleans.Length & 7) != 0)
                numBytes++;

            //初始化字节数组
            byte[] bitField = new byte[numBytes];

            //初始化变量
            int byteIndex = 0;
            int i;
            int length = booleans.Length;
            byte v = 0;
            byte p = 0x80;

            //开始转换布尔数组
            for (i = 0; i < length; i++)
            {
                if (booleans[i])
                {
                    v |= p;
                }
                
                p >>= 1;

                //传输8位布尔数组到一个bitField字节中
                if (p == 0)
                {
                    p = 0x80;
                    bitField[byteIndex] = v;
                    byteIndex++;
                    v = 0;
                }
            }

            //如果布尔数组的长度不位8的倍数,最后的低n(n < 8)位传输到bitField的最后一个字节.
            if (p != 0x80)
                bitField[byteIndex] = v;

            //返回转换所得的字节数组
            return bitField;
        }

        /// <summary>
        /// 将字节数组转换为布尔数组
        /// </summary>
        /// <param name="bitField">待转换的字节数组</param>
        /// <param name="start">转换的起始位置</param>
        /// <param name="lengthBytes">转换的长度</param>
        /// <returns>转换所得布尔数组</returns>
        public static bool[] FromBitField(byte[] bitField, int start, int length)
        {
            
            //初始化布尔数组
            bool[] booleans = new bool[length];

            //初始化变量
            int i;
            int byteIndex = start;
            byte p = 0x80;

            //开始转换布尔数组
            for (i = 0; i < length; i++)
            {
                booleans[i] = (bitField[byteIndex] & p) != 0;
                p >>= 1;
                if (p == 0)
                {
                    p = 0x80;
                    byteIndex++;
                }
            }

            //返回转换所得的布尔数组
            return booleans;
        }
    }
}