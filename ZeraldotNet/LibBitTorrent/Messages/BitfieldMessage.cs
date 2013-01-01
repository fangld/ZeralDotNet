using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Bitfield message
    /// </summary>
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
        private bool[] _bitfield;

        #endregion

        #region Properties

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { return _bytesLength; }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.BitField; }
        }

        #endregion

        #region Constructors

        public BitfieldMessage(int bitfieldLength)
        {
            _bitfield = new bool[bitfieldLength];
            Array.Clear(_bitfield, 0, bitfieldLength);
        }

        public BitfieldMessage(bool[] bitfield)
        {
            int count = bitfield.Length;
            _bitfield = new bool[count];
            Array.Copy(bitfield, _bitfield, count);
        }

        #endregion

        #region Methods

        /// <summary>
        /// GetByteArray the bitfield to bitfield
        /// </summary>
        /// <param name="byteArray">The bytes that received from the network</param>
        /// <param name="startIndex">The start index of bitfield</param>
        private void FromByteArray(byte[] byteArray, int startIndex)
        {
            int fullBitLength = byteArray.Length - 2;
            int spareBitIndex = startIndex + fullBitLength;

            //Set the full 8-bit bitfield
            for (int index = startIndex; index < spareBitIndex; index++)
            {
                int booleanIndex = ((index - 1) << 3);

                for (int offset = 0; offset < 8; offset++)
                {
                    _bitfield[booleanIndex + offset] = (byteArray[index] & andBitArray[offset]) == andBitArray[offset];
                }                  
            }
            
            //Set the spare bit bitfield
            int spareBitBitfieldIndex = ((spareBitIndex - 1) << 3);
            int spareLength = _bitfield.Length - spareBitBitfieldIndex;
            for (int offset = 0; offset < spareLength; offset++)
            {
                _bitfield[offset + spareBitBitfieldIndex] =(byteArray[spareBitIndex] & andBitArray[offset]) == andBitArray[offset];
            }
        }

        /// <summary>
        /// GetByteArray bit field to the byte array
        /// </summary>
        /// <param name="bitfield">the bit field</param>
        /// <returns>the corresponding array contains byte</returns>
        private static byte[] ToByteArray(bool[] bitfield)
        {
            int bitfieldLength = bitfield.Length;
            int fullBitIndex = (bitfieldLength & 0x7FFFFFF8);

            //if the length of bitfield is 0, return the array contains byte whose length also is 0.
            if (bitfieldLength == 0)
                return new byte[0];

            //Compute the length of the array contains byte
            int bytesLength = bitfieldLength >> 3;

            if ((bitfieldLength & 7) != 0)
                bytesLength++;

            //Initial the array contains byte
            byte[] result = new byte[bytesLength];

            for (int index = 0; index < bytesLength - 1; index++)
            {
                byte bitByte = 0;
                byte currentBit = 0x80;

                for (int offset = 0; offset < 8; offset++)
                {
                    if (bitfield[(index << 3) + offset])
                    {
                        bitByte |= currentBit;
                    }
                    currentBit >>= 1;
                }
                result[index] = bitByte;
            }

            //if the length of bitfield is not a multiple of 8, set the least n(n < 8) bit in the last byte.
            byte spareBitByte = 0;
            byte currentSpareBit = 0x80;

            for (int i = fullBitIndex; i < bitfieldLength; i++)
            {
                if (bitfield[i])
                {
                    spareBitByte |= currentSpareBit;
                }
                currentSpareBit >>= 1;
            }
            result[bytesLength - 1] = spareBitByte;

            return result;
        }

        public void SetBitfield(bool[] bitfield)
        {
            _bitfield = bitfield;
        }

        public void SetBitfield(int index)
        {
            Debug.Assert(_bitfield != null);
            Debug.Assert(index >= 0 || index < _bitfield.Length);
            _bitfield[index] = true;
        }

        public bool[] GetBitfield()
        {
            return _bitfield;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Get the array of byte that corresponds bitfield message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public override byte[] GetByteArray()
        {
            byte[] bitfieldBytes = ToByteArray(_bitfield);

            _bytesLength = bitfieldBytes.Length + 1;

            byte[] result = new byte[_bytesLength + 4];

            SetBytesLength(result, _bytesLength);

            //Message id is 5
            result[4] = (byte)MessageType.BitField;

            //Write bitfield
            Array.Copy(bitfieldBytes, 0, result, 5, _bytesLength - 1);
            return result;
        }

        /// <summary>
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
        public override bool Parse(byte[] buffer)
        {
            FromByteArray(buffer, 1);
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

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.CopyFromBitfield(_bitfield);
        }

        public override string ToString()
        {
            //return MessageString;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Bitfield message: Length:{0}", _bitfield.Length);
            sb.AppendLine();
            int i;
            for (i = 0; i < _bitfield.Length - 8;)
            {
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(_bitfield[i++] ? 1 : 0);
                sb.Append(" | ");
            }

            for (; i < _bitfield.Length; i++)
            {
                sb.Append(_bitfield[i] ? 1 : 0);
            }

            return sb.ToString();
        }

        #endregion
    }
}
