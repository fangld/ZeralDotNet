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

        #endregion
    }
}
