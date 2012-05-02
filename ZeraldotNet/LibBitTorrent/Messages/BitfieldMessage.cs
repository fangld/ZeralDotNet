using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string MessageString = "BitField";
        
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

        public BitfieldMessage(int booleansLength)
        {
            _booleans = new bool[booleansLength];
            Array.Clear(_booleans, 0, booleansLength);
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
        /// Transfer the bitfield to booleans
        /// </summary>
        /// <param name="bitField">The bytes that received from the network</param>
        /// <param name="startIndex">The start index of bitfield</param>
        private void FromBitField(byte[] bitField, int startIndex)
        {
            int fullBitLength = bitField.Length - 2;
            int spareBitIndex = startIndex + fullBitLength;

            //Set the full 8-bit booleans
            Parallel.For(startIndex, spareBitIndex, index =>
                                                        {
                                                            int booleanIndex = ((index - 1) << 3);
                                                            Parallel.For(0, 8,
                                                                         offset =>
                                                                         _booleans[booleanIndex + offset] =
                                                                         ((bitField[index] & andBitArray[offset]) ==
                                                                          andBitArray[offset]));
                                                        });

            //Set the spare bit booleans
            int spareBitBooleansIndex = ((spareBitIndex - 1) << 3);
            Parallel.For(0, _booleans.Length - spareBitBooleansIndex,
                         offset =>
                         _booleans[offset + spareBitBooleansIndex] =
                         ((bitField[spareBitIndex] & andBitArray[offset]) == andBitArray[offset]));

        }

        /// <summary>
        /// 将布尔数组转换为字节数组
        /// </summary>
        /// <param name="booleans">待转换的布尔数组</param>
        /// <returns>转换所得的字节数组</returns>
        private static byte[] ToBitField(bool[] booleans)
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

        public void SetBooleans(int index)
        {
            Debug.Assert(_booleans != null);
            Debug.Assert(index >= 0 || index < _booleans.Length);
            _booleans[index] = true;
        }

        public bool[] GetBooleans()
        {
            return _booleans;
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
            FromBitField(buffer, 1);
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
            //return MessageString;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Bitfield message: Length:{0}", _booleans.Length);
            sb.AppendLine();
            int i;
            for (i = 0; i < _booleans.Length - 8;)
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

            for (; i < _booleans.Length; i++)
            {
                sb.Append(_booleans[i] ? 1 : 0);
            }

            return sb.ToString();
        }

        #endregion
    }
}
