using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The buffer pool of bytes
    /// </summary>
    public class BufferPool
    {
        #region Fields

        /// <summary>
        /// 内存缓冲字节
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// The index of buffer that start to writing
        /// </summary>
        private int _writeIndex;

        /// <summary>
        /// The index of buffer that start to reading
        /// </summary>
        private int _readIndex;

        private object _synObj;

        #endregion

        #region Properties

        /// <summary>
        /// 已写入长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int Capacity { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initial the buffer pool
        /// </summary>
        /// <param name="capacity">The capacity of buffer</param>
        public BufferPool(int capacity = 4)
        {
            _synObj = new object();
            _buffer = new byte[capacity];
            _writeIndex = 0;
            _readIndex = 0;
            Length = 0;
            Capacity = capacity;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 读取字节数组
        /// </summary>
        /// <param name="bytes">读取的字节数组</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">读取数量</param>
        public void Read(byte[] bytes, int offset, int count)
        {
            lock (_synObj)
            {
                if (Length < count)
                {
                    throw new BitTorrentException("The length is less than the required count of bytes.");
                }
                Buffer.BlockCopy(_buffer, _readIndex, bytes, offset, count);
                _readIndex += count;
                Length -= count;
            }
        }

        /// <summary>
        /// 获取第一个字节
        /// </summary>
        /// <returns></returns>
        public byte GetFirstByte()
        {
            lock (_synObj)
            {
                byte result = _buffer[_readIndex];
                return result;
            }
        }

        /// <summary>
        /// 写入字节流
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] bytes, int offset, int count)
        {
            lock (_synObj)
            {
                if (Capacity - _writeIndex - Length < count)
                {
                    if (Capacity - Length < count)
                    {
                        do
                        {
                            Capacity <<= 1;
                        } while (count + Length > Capacity);

                        Array.Resize(ref _buffer, Capacity);
                    }
                    Buffer.BlockCopy(_buffer, _writeIndex, _buffer, 0, Length);
                    _writeIndex = Length;
                }
                Buffer.BlockCopy(bytes, offset, _buffer, _writeIndex, count);
                Length += count;
                _writeIndex += count;

                //Console.WriteLine();
            }
        }

        /// <summary>
        /// 寻址
        /// </summary>
        /// <param name="offset"></param>
        public void Seek(int offset)
        {
            lock (_synObj)
            {
                if (_readIndex == 0 && offset < 0)
                {
                    throw new BitTorrentException("Seek wrong: the read index of buffer is less than zero.");
                }

                if (_readIndex >=_writeIndex && offset > 0)
                {
                    throw new BitTorrentException("Seek wrong: the read index of buffer is more than the write index.");
                }

                _readIndex += offset;
                Length -= offset;
            }
        }

        #endregion
    }
}
