using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// Interested网络信息类
    /// </summary>
    public class InterestedMessage : ChokeMessage
    {
        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为2
            return Encode(BitTorrentMessageType.Interested);
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        { 
            //信息ID为2
            return Decode(buffer, BitTorrentMessageType.Interested);
        }

        #endregion
    }
}
