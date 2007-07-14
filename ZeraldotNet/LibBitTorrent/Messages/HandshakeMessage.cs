using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class HandshakeMessage : Message
    {
        #region Private Field

        /// <summary>
        /// 下载文件的SHA1码
        /// </summary>
        private byte[] downloadID;

        /// <summary>
        /// 节点的ID号
        /// </summary>
        private byte[] peerID;

        #endregion

        #region Constructors

        public HandshakeMessage(byte[] downloadID, byte[] peerID)
        {
            if (downloadID.Length != 20)
            {
                throw new BitTorrentException("下载文件的SHA1码出错！");
            }

            if (peerID.Length != 20)
            {
                throw new BitTorrentException("节点的ID号出错！");
            }

            this.downloadID = downloadID;
            this.peerID = peerID;
        }

        #endregion

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            byte[] result = new byte[68];
            result[0] = 19;

            Globals.CopyBytes(Globals.protocolName, result, 1);

            int i;
            for (i = 20; i < 28; i++)
            {
                result[i] = 0;
            }

            Globals.CopyBytes(downloadID, result, 28);

            Globals.CopyBytes(peerID, result, 48);

            return result;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 网络信息的长度
        /// </summary>
        public override int BytesLength
        {
            get { return 68; }
        }
    }
}
