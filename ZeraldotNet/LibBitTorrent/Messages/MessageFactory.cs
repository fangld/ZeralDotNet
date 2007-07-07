using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息单件模式
    /// </summary>
    public class MessageFactory
    {
        #region Private Field

        /// <summary>
        /// choke网络信息
        /// </summary>
        private static readonly ChokeMessage chokeMessage;

        /// <summary>
        /// unchoke网络信息
        /// </summary>
        private static readonly UnchokeMessage unchokeMessage;

        /// <summary>
        /// interested网络信息
        /// </summary>
        private static readonly InterestedMessage interestedMessage;


        /// <summary>
        /// not interested网络信息
        /// </summary>
        private static readonly NotInterestedMessage notInterestedMessage;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static MessageFactory()
        {
            chokeMessage = new ChokeMessage();
            unchokeMessage = new UnchokeMessage();
            interestedMessage = new InterestedMessage();
            notInterestedMessage = new NotInterestedMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 返回choke网络信息
        /// </summary>
        /// <returns>返回choke网络信息</returns>
        public static ChokeMessage GetChokeMessage()
        {
            return chokeMessage;
        }

        /// <summary>
        /// 返回unchoke网络信息
        /// </summary>
        /// <returns>返回unchoke网络信息</returns>
        public static UnchokeMessage GetUnchokeMessage()
        {
            return unchokeMessage;
        }

        /// <summary>
        /// 返回interested网络信息
        /// </summary>
        /// <returns>返回interested网络信息</returns>
        public static InterestedMessage GetInterestedMessage()
        {
            return interestedMessage;
        }

        /// <summary>
        /// 返回not interested网络信息
        /// </summary>
        /// <returns>返回not interested网络信息</returns>
        public static NotInterestedMessage GetNotInterestedMessage()
        {
            return notInterestedMessage;
        }

        #endregion
    }
}
