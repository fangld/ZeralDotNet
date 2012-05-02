using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// The array of bytes
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
        
        /// <summary>
        /// The synchronized object
        /// </summary>
        private readonly object _synchronizedObject;


        private object _debugObj;
        private FileStream _debugStream;
        private StreamWriter _debugWriter;

        #endregion

        #region Properties

        /// <summary>
        /// 已写入长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The capacity of the buffer
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
            _synchronizedObject = new object();

            _buffer = new byte[capacity];
            _writeIndex = 0;
            _readIndex = 0;
            Length = 0;
            Capacity = capacity;

            _debugObj = new object();
            _debugStream = new FileStream(@"E:\Bittorrent\debug.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                          FileShare.ReadWrite);
            _debugWriter = new StreamWriter(_debugStream);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Read required buffer to bytes
        /// </summary>
        /// <param name="bytes">The bytes that is readed the required buffer</param>
        /// <param name="offset">The offset</param>
        /// <param name="count">The required count</param>
        public void Read(byte[] bytes, int offset, int count)
        {
            lock (_synchronizedObject)
            {
                Debug("Before read offset:{0}, count:{1}", offset, count);

                if (Length < count)
                {
                    throw new BitTorrentException("The length is less than the required count of bytes.");
                }
                if (_readIndex + count < Capacity)
                {
                    Buffer.BlockCopy(_buffer, _readIndex, bytes, offset, count);
                    _readIndex += count;
                }
                else
                {
                    int firstCount = Capacity - _readIndex;
                    Buffer.BlockCopy(_buffer, _readIndex, bytes, offset, firstCount);
                    offset += firstCount;
                    int secondCount = count - firstCount;
                    Buffer.BlockCopy(_buffer, 0, bytes, offset, secondCount);
                    _readIndex = secondCount;
                }
                Length -= count;


                Debug("After read offset:{0}, count:{1}", offset, count);

            }
        }

        /// <summary>
        /// Get the first byte
        /// </summary>
        /// <returns></returns>
        public byte GetFirstByte()
        {
            lock (_synchronizedObject)
            {
                byte result = _buffer[_readIndex];
                return result;
            }
        }

        /// <summary>
        /// Write bytes to buffer
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] bytes, int offset, int count)
        {
            lock (_synchronizedObject)
            {
                Debug("Before write offset:{0}, count:{1}", offset, count);

                //Rearrange the buffer
                if (_writeIndex + count + Length > Capacity)
                {
                    Buffer.BlockCopy(_buffer, _readIndex, _buffer, 0, Length);
                    _writeIndex = Length;
                    _readIndex = 0;

                    //Extend the count of buffer
                    if (count + Length > Capacity)
                    {
                        do
                        {
                            Capacity <<= 1;
                        } while (count + Length > Capacity);

                        Array.Resize(ref _buffer, Capacity);
                    }
                }
                
                Buffer.BlockCopy(bytes, offset, _buffer, _writeIndex, count);
                Length += count;
                _writeIndex += count;

                Debug("After write offset:{0}, count:{1}", offset, count);
            }
        }

        /// <summary>
        /// Seek to the next position by the offset
        /// </summary>
        /// <param name="offset">The required offset</param>
        public void Seek(int offset)
        {
            lock (_synchronizedObject)
            {
                if (_readIndex == 0 && offset < 0)
                {
                    throw new BitTorrentException("Seek wrong: the read index of buffer is less than zero.");
                }

                if (_readIndex >=_writeIndex && offset > 0)
                {
                    throw new BitTorrentException("Seek wrong: the read index of buffer is more than the write index.");
                }

                Debug("Before seek offset:{0}", offset);

                _readIndex += offset;
                Length -= offset;

                Debug("After seek offset:{0}", offset);
            }
        }

#if DEBUG

        private void Debug(string invokedFunction, params  object[] objects)
        {
            lock (_debugObj)
            {
                string comment = string.Format(invokedFunction, objects);
                //Console.WriteLine("{0}: ReadIndex:{1}, WriteIndex:{2}, Length:{3}", comment, _readIndex, _writeIndex,
                //                  Length);
                _debugWriter.WriteLine("{0}: ReadIndex:{1}, WriteIndex:{2}, Length:{3}", comment, _readIndex,
                                       _writeIndex, Length);
                _debugWriter.Flush();
            }
        }

#endif

        #endregion
    }
}
