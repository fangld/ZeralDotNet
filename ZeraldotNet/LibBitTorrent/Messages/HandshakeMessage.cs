using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Handshake message
    /// </summary>
    public class HandshakeMessage : Message
    {
        #region Fields

        /// <summary>
        /// The length of infohash
        /// </summary>
        private const int InfoHashLength = 20;

        /// <summary>
        /// The length of peer id
        /// </summary>
        private const int PeerIdLength=20;

        #endregion

        #region Properties

        /// <summary>
        /// The SHA1 hash of the torrent
        /// </summary>
        public byte[] InfoHash { get; set; }

        /// <summary>
        /// The peer id
        /// </summary>
        public byte[] PeerId { get; set; }

        /// <summary>
        /// The flag that represents whether peer support DHT.
        /// </summary>
        public bool IsDht { get; set; }

        /// <summary>
        /// The flag that represents whether peer support peer exchange.
        /// </summary>
        public bool IsPeerExchange { get; set; }

        /// <summary>
        /// The flag that represents whether peer support fast extension.
        /// </summary>
        public bool IsFastExtension { get; set; }

        #endregion

        #region Constructors

        public HandshakeMessage()
        {
            InfoHash = new byte[InfoHashLength];
            PeerId = new byte[PeerIdLength];
        }

        public HandshakeMessage(byte[] infoHash, byte[] peerId)
        {
            Debug.Assert(infoHash.Length == InfoHashLength);
            Debug.Assert(peerId.Length == PeerIdLength);

            InfoHash = infoHash;
            PeerId = peerId;
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
            
            if (IsDht)
            {
                result[27] |= 0x01; //Enable DHT protocol
            }

            if (IsPeerExchange)
            {
                result[27] |= 0x02; //Enable XBT Peer Exchange
            }
            
            if (IsFastExtension)
            {
                result[27] |= 0x04; //Enable Fast Extension
            }

            //result[27] |= 0x08; //Enable NAT Traversal
            Buffer.BlockCopy(InfoHash, 0, result, 28, InfoHash.Length);

            Buffer.BlockCopy(PeerId, 0, result, 48, PeerId.Length);

            return result;
        }

        public override bool Parse(byte[] buffer)
        {
            Buffer.BlockCopy(buffer, 28, InfoHash, 0, InfoHashLength);
            Buffer.BlockCopy(buffer, 48, PeerId, 0, PeerIdLength);
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
            InfoHash = new byte[20];
            PeerId = new byte[20];
            Buffer.BlockCopy(bytes, offset + 28, InfoHash, 0, 20);
            Buffer.BlockCopy(bytes, offset + 48, PeerId, 0, 20);
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
            InfoHash = new byte[20];
            PeerId = new byte[20];
            Buffer.BlockCopy(bytes, 28, InfoHash, 0, 20);
            Buffer.BlockCopy(bytes, 48, PeerId, 0, 20);
            return true;
        }

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.PeerId = PeerId;
            peer.InfoHash = InfoHash;
            peer.IsDht = IsDht;
            peer.IsPeerExchange = IsPeerExchange;
            peer.IsFastExtension = IsFastExtension;
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
        /// The length of message
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
            string result = string.Format("Handshake message: InfoHash:{0}, PeerId:{1}", InfoHash.ToHexString(),
                                          PeerId.ToHexString());
            return result;
        }
    }
}
