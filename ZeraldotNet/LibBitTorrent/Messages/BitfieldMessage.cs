using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class BitfieldMessage : Message
    {
        #region Fields

        private static byte[] andBitArray = new byte[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        
        /// <summary>
        /// 网络信息的字节长度
        /// </summary>
        private int _bytesLength;

        /// <summary>
        /// 片断的BitField信息
        /// </summary>
        private bool[] _booleans;

        #endregion

        #region Constructors

        public BitfieldMessage()
        {
            
        }

        public BitfieldMessage(bool[] booleans)
        {
            int count = booleans.Length;
            _booleans = new bool[count];
            Array.Copy(booleans, _booleans, count);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初始化网络信息的字节长度
        /// </summary>
        /// <param name="pieceNumber">片断的数量</param>
        public void InitialBytesLength(int pieceNumber)
        {
            _bytesLength = pieceNumber >> 3;
            _bytesLength++;
            if ((pieceNumber & 7) != 0)
            {
                _bytesLength++;
            }
        }

        /// <summary>
        /// 将字节数组转换为布尔数组
        /// </summary>
        /// <param name="bitField">待转换的字节数组</param>
        /// <param name="start">转换的起始位置</param>
        /// <param name="length">转换的长度</param>
        /// <returns>转换所得布尔数组</returns>
        private bool[] FromBitField(byte[] bitField, int start, int length)
        {
            //初始化布尔数组
            bool[] result = new bool[length];
            int fullBitLength = bitField.Length - 1;

            Parallel.For(0, fullBitLength, index =>
            {
                int booleanIndex = (index << 3);
                Parallel.For(0, 7,
                             offset =>
                             result[booleanIndex + offset] =
                             ((bitField[index] & andBitArray[offset]) ==
                              andBitArray[offset]));
            });

            int spareBitIndex = (fullBitLength << 3);

            Parallel.For(spareBitIndex, length - spareBitIndex,
                         offset =>
                         result[spareBitIndex + offset] =
                         ((bitField[spareBitIndex] & andBitArray[offset]) == andBitArray[offset]));

            return result;
        }

        /// <summary>
        /// 将布尔数组转换为字节数组
        /// </summary>
        /// <param name="booleans">待转换的布尔数组</param>
        /// <returns>转换所得的字节数组</returns>
        public static byte[] ToBitField(bool[] booleans)
        {
            int booleansLength = booleans.Length;
            int fullBitIndex = (booleansLength | 0x7FFFFFF8);

            //如果booleans数组等于零,返回空字节数组
            if (booleansLength == 0)
                return new byte[0];

            //计算字节数组长度
            int bytesLength = booleansLength >> 3;

            if ((booleansLength & 7) != 0)
                bytesLength++;

            //初始化字节数组
            byte[] result = new byte[bytesLength];
            Parallel.For(0, bytesLength - 1, index =>
                                                 {
                                                     byte bitByte = 0;
                                                     byte currentBit = 0x80;

                                                     for (int i = 0; i < 8; i++)
                                                     {
                                                         if (booleans[i])
                                                         {
                                                             bitByte |= currentBit;
                                                         }
                                                         currentBit >>= 1;
                                                     }

                                                     result[index] = bitByte;
                                                 }
                );

            //如果布尔数组的长度不位8的倍数,最后的低n(n < 8)位传输到bitField的最后一个字节.
            byte spareBitByte = 0;
            byte currentSpareBit = 0x80;

            for (int i = fullBitIndex; i < booleansLength; i++)
            {
                if (booleans[i])
                {
                    spareBitByte |= currentSpareBit;
                }
                currentSpareBit >>= 1;
            }
            result[bytesLength - 1] = spareBitByte;

            
            //返回转换所得的字节数组
            return result;
        }

        public void SetBooleans(bool[] booleans)
        {
            _booleans = booleans;
        }
        
        #endregion

        #region Overriden Methods

        public override byte[] Encode()
        {
            byte[] bitFieldBytes = ToBitField(_booleans);

            _bytesLength = bitFieldBytes.Length + 1;

            byte[] result = new byte[_bytesLength + 4];

            SetBytesLength(result, _bytesLength);

            //信息ID为5
            result[4] = (byte)MessageType.BitField;

            //写入BitField
            Array.Copy(bitFieldBytes, 0, result, 5, _bytesLength - 1);
            return result;
        }

        public override bool Parse(byte[] buffer)
        {
            int booleansLength = ((buffer.Length - 1) << 3);
            _booleans = FromBitField(buffer, 1, booleansLength);
            return true;
        }

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            int length = GetLength(buffer, offset);
            if (buffer[offset + 4] == (byte)MessageType.BitField)
            {
                
            }
            return false;
        }

        public override bool Parse(MemoryStream ms)
        {
            throw new NotImplementedException();
        }

        public override int BytesLength
        {
            get { return _bytesLength; }
        }

        public override MessageType Type
        {
            get { return MessageType.BitField; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Bitfield message: Length:{0}", _booleans.Length);
            sb.AppendLine();
            for (int i = 0; i < _booleans.Length;)
            {
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(_booleans[i++] ? 1 : 0);
                sb.Append(" | ");
            }
            return sb.ToString();
        }

        #endregion
    }
}
