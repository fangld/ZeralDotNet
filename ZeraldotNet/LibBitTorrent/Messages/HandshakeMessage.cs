using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class HandshakeMessage : Message
    {
        #region Fields

        /// <summary>
        /// 下载文件的SHA1码
        /// </summary>
        private byte[] downloadID;

        /// <summary>
        /// 节点的ID号
        /// </summary>
        private byte[] peerID;

        #endregion

        #region Properties

        public bool IsDht { get; set; }

        public bool IsPeerExchange { get; set; }

        public bool IsFastExtension { get; set; }

        #endregion

        #region Constructors

        public HandshakeMessage()
        {
            
        }

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

            Array.Copy(Globals.ProtocolName, 0, result, 1, Globals.ProtocolName.Length);
            
            int i;
            for (i = 20; i < 28; i++)
            {
                result[i] = 0;
            }

            //result[20] |= 0x80; //Enable Azureus Messaging Protocol

            //result[22] |= 0x08; //Enable BitTorrent Location-aware Protocol (no known implementations)
 
            //result[25] |= 0x10; //Enable LTEP (Libtorrent Extension Protocol)
            //result[25] |= 0x02; //Enable Extension Negotiation Protocol
            //result[25] |= 0x01; //Enable Extension Negotiation Protocol
            
            //result[27] |= 0x01; //Enable DHT protocol
            //result[27] |= 0x02; //Enable XBT Peer Exchange
            //result[27] |= 0x04; //Enable Fast Extension
            //result[27] |= 0x08; //Enable NAT Traversal
            Array.Copy(downloadID, 0, result, 28, downloadID.Length);

            Array.Copy(peerID, 0, result, 48, peerID.Length);

            return result;
        }

        public override bool Decode(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer, int offset, int count)
        {
            if (count - offset < BytesLength)
            {
                return false;
            }
            
            byte[] bytes = new byte[BytesLength];
            downloadID = new byte[20];
            peerID = new byte[20];
            Array.Copy(bytes, offset + 28, downloadID, 0, 20);
            Array.Copy(bytes, offset + 48, peerID, 0, 20);
            return true;
        }

        ///// <summary>
        ///// 网络信息的解码函数
        ///// </summary>
        ///// <param name="buffer">待解码的字节流</param>
        ///// <returns>返回是否解码成功</returns>
        //public override bool Parse(byte[] buffer)
        //{
        //    byte[] bytes = new byte[BytesLength];
        //    downloadID = new byte[20];
        //    peerID = new byte[20];
        //    Array.Copy(bytes, 28, downloadID, 0, 20);
        //    Array.Copy(bytes, 48, peerID, 0, 20);
        //}

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="ms">待解码的内存流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(MemoryStream ms)
        {
            if (ms.Length < BytesLength)
            {
                return false;
            } 
            byte[] bytes = new byte[BytesLength];
            ms.Read(bytes, 0, BytesLength);
            downloadID = new byte[20];
            peerID = new byte[20];
            Array.Copy(bytes, 28, downloadID, 0, 20);
            Array.Copy(bytes, 48, peerID, 0, 20);
            return true;
        }

        ///// <summary>
        ///// 网络信息的处理函数
        ///// </summary>
        ///// <param name="buffer">待处理的字节流</param>
        ///// <returns>返回是否处理成功</returns>
        //public override bool Handle(byte[] buffer, int offset)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        /// <summary>
        /// 网络信息的字节长度
        /// </summary>
        public override int BytesLength
        {
            get { return 68; }
        }

        public override MessageType Type
        {
            get { return MessageType.Handshake; }
        }
    }
}
