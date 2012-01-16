using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 内存缓冲池
    /// </summary>
    public class BufferPool
    {
        #region Fields

        /// <summary>
        /// 内存缓冲字节
        /// </summary>
        private byte[] _buffer;

        #endregion

        #region Properties

        /// <summary>
        /// 当前位置
        /// </summary>
        public int Position { get; set; }

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
        /// 构造函数
        /// </summary>
        /// <param name="capacity">最大长度</param>
        public BufferPool(int capacity)
        {
            _buffer = new byte[capacity];
            Position = 0;
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
            Array.Copy(_buffer, Position, bytes, offset, count);
            Position += count;
            Length -= count;
        }

        /// <summary>
        /// 获取第一个字节
        /// </summary>
        /// <returns></returns>
        public byte GetFirstByte()
        {
            byte result = _buffer[Position];
            return result;
        }

        /// <summary>
        /// 写入字节流
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] bytes, int offset, int count)
        {
            if (Capacity - Position - Length < count)
            {
                if (Capacity - Length < count)
                {
                    do
                    {
                        Capacity <<= 1;
                    } while (count + Length > Capacity);

                    Array.Resize(ref _buffer, Capacity);
                }
                Array.Copy(_buffer, Position, _buffer, 0, Length);
                Position = 0;
            }
            Length += count;
            Array.Copy(bytes, offset, _buffer, Position, count);
        }

        /// <summary>
        /// 寻址
        /// </summary>
        /// <param name="offset"></param>
        public void Seek(int offset)
        {
            this.Position += offset;
            this.Length -= offset;
        }

        #endregion
    }
}
