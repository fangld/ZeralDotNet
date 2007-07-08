using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// 网络信息单件模式
    /// </summary>
    public class DummyMessageFactory
    {
        #region Private Field

        /// <summary>
        /// choke网络信息
        /// </summary>
        private static readonly DummyChokeMessage chokeMessage;

        /// <summary>
        /// unchoke网络信息
        /// </summary>
        private static readonly DummyUnchokeMessage unchokeMessage;

        /// <summary>
        /// interested网络信息
        /// </summary>
        private static readonly DummyInterestedMessage interestedMessage;


        /// <summary>
        /// not interested网络信息
        /// </summary>
        private static readonly DummyNotInterestedMessage notInterestedMessage;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static DummyMessageFactory()
        {
            chokeMessage = new DummyChokeMessage();
            unchokeMessage = new DummyUnchokeMessage();
            interestedMessage = new DummyInterestedMessage();
            notInterestedMessage = new DummyNotInterestedMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 返回choke网络信息
        /// </summary>
        /// <returns>返回choke网络信息</returns>
        public static DummyChokeMessage GetChokeMessage()
        {
            return chokeMessage;
        }

        /// <summary>
        /// 返回unchoke网络信息
        /// </summary>
        /// <returns>返回unchoke网络信息</returns>
        public static DummyUnchokeMessage GetUnchokeMessage()
        {
            return unchokeMessage;
        }

        /// <summary>
        /// 返回interested网络信息
        /// </summary>
        /// <returns>返回interested网络信息</returns>
        public static DummyInterestedMessage GetInterestedMessage()
        {
            return interestedMessage;
        }

        /// <summary>
        /// 返回not interested网络信息
        /// </summary>
        /// <returns>返回not interested网络信息</returns>
        public static DummyNotInterestedMessage GetNotInterestedMessage()
        {
            return notInterestedMessage;
        }

        /// <summary>
        /// 返回have网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <returns>返回have网络信息</returns>
        public static DummyHaveMessage GetHaveMessage(int index)
        {
            return new DummyHaveMessage(index);
        }

        /// <summary>
        /// 返回bitfield网络信息
        /// </summary>
        /// <param name="bitfield">已经下载的文件片断</param>
        /// <returns>返回bitfield网络信息</returns>
        public static DummyBitfieldMessage GetBitfieldMessage(bool[] bitfield)
        {
            return new DummyBitfieldMessage(bitfield);
        }

        /// <summary>
        /// 返回request网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        /// <returns>返回request网络信息</returns>
        public static DummyRequestMessage GetRequestMessage(int index, int begin, int length)
        {
            return new DummyRequestMessage(index, begin, length);
        }

        /// <summary>
        /// 返回piece网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="pieces">子片断的数据</param>
        /// <returns>返回piece网络信息</returns>
        public static DummyPieceMessage GetPieceMessage(int index, int begin, byte[] pieces)
        {
            return new DummyPieceMessage(index, begin, pieces);
        }

        /// <summary>
        /// 返回cancel网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        /// <returns>返回cancel网络信息</returns>
        public static DummyCancelMessage GetCancelMessage(int index, int begin, int length)
        {
            return new DummyCancelMessage(index, begin, length);
        }

        /// <summary>
        /// 返回port网络信息
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        /// <returns>返回port网络信息</returns>
        public static DummyPortMessage GetPortMessage(ushort port)
        {
            return new DummyPortMessage(port);
        }

        #endregion
    }
}
