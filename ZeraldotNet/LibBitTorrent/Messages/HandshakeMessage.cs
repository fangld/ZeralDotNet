using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private byte[] _infoHash;

        /// <summary>
        /// 节点的ID号
        /// </summary>
        private byte[] _peerId;

        private const int _infoHashLength = 20;

        private const int _peerIdLength=20;

        #endregion

        #region Properties

        public bool IsDht { get; set; }

        public bool IsPeerExchange { get; set; }

        public bool IsFastExtension { get; set; }

        #endregion

        #region Constructors

        public HandshakeMessage()
        {
            _infoHash = new byte[_infoHashLength];
            _peerId = new byte[_peerIdLength];
        }

        public HandshakeMessage(byte[] infoHash, byte[] peerId)
        {
            Debug.Assert(infoHash.Length == 20);
            Debug.Assert(peerId.Length == 20);
            //if (infoHash.Length != 20)
            //{
            //    throw new BitTorrentException("下载文件的SHA1码出错！");
            //}

            //if (peerId.Length != 20)
            //{
            //    throw new BitTorrentException("节点的ID号出错！");
            //}

            _infoHash = infoHash;
            _peerId = peerId;
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

            Buffer.BlockCopy(Globals.ProtocolName, 0, result, 1, Globals.ProtocolName.Length);
            
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
            Buffer.BlockCopy(_infoHash, 0, result, 28, _infoHash.Length);

            Buffer.BlockCopy(_peerId, 0, result, 48, _peerId.Length);

            return result;
        }

        public override bool Parse(byte[] buffer)
        {
            Buffer.BlockCopy(buffer, 28, _infoHash, 0, _infoHashLength);
            Buffer.BlockCopy(buffer, 48, _peerId, 0, _peerIdLength);
            return true;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Parse(byte[] buffer, int offset, int count)
        {
            if (count - offset < BytesLength)
            {
                return false;
            }
            
            byte[] bytes = new byte[BytesLength];
            _infoHash = new byte[20];
            _peerId = new byte[20];
            Buffer.BlockCopy(bytes, offset + 28, _infoHash, 0, 20);
            Buffer.BlockCopy(bytes, offset + 48, _peerId, 0, 20);
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
        //    infoHash = new byte[20];
        //    peerID = new byte[20];
        //    Array.Copy(bytes, 28, infoHash, 0, 20);
        //    Array.Copy(bytes, 48, peerID, 0, 20);
        //}

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="ms">待解码的内存流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Parse(MemoryStream ms)
        {
            if (ms.Length < BytesLength)
            {
                return false;
            } 
            byte[] bytes = new byte[BytesLength];
            ms.Read(bytes, 0, BytesLength);
            _infoHash = new byte[20];
            _peerId = new byte[20];
            Buffer.BlockCopy(bytes, 28, _infoHash, 0, 20);
            Buffer.BlockCopy(bytes, 48, _peerId, 0, 20);
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

        public override string ToString()
        {
            string result = string.Format("Handshake message: InfoHash:{0}, LocalPeerId:{1}", _infoHash.ToHexString(),
                                          _peerId.ToHexString());
            return result;
        }
    }
}
