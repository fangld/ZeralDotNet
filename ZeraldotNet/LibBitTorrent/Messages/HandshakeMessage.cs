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
        /// The flag that represents whether peer supports extension protocol
        /// </summary>
        public bool SupportExtension { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports DHT.
        /// </summary>
        public bool SupportDht { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports peer exchange.
        /// </summary>
        public bool SupportPeerExchange { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports fast peer.
        /// </summary>
        public bool SupportFastPeer { get; set; }

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { return 68; }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.Handshake; }
        }

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
        public override byte[] GetByteArray()
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
 
            //result[25] |= 0x02; //Enable Extension Negotiation Protocol
            //result[25] |= 0x01; //Enable Extension Negotiation Protocol

            if (SupportExtension)
            {
                result[25] |= 0x10; //Enable LTEP (Libtorrent Extension Protocol)
            }
            
            if (SupportDht)
            {
                result[27] |= 0x01; //Enable DHT protocol
            }

            if (SupportPeerExchange)
            {
                result[27] |= 0x02; //Enable XBT Peer Exchange
            }
            
            if (SupportFastPeer)
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
            SupportExtension = (buffer[25] & 0x10) != 0;
            SupportDht = (buffer[27] & 0x01) != 0;
            SupportPeerExchange = (buffer[27] & 0x02) != 0;
            SupportFastPeer = (buffer[27] & 0x04) != 0;
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

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.PeerId = GetPeerIdString();
            peer.InfoHash = InfoHash;
            peer.SupportExtension = SupportExtension;
            peer.SupportDht = SupportDht;
            peer.SupporteerExchange = SupportPeerExchange;
            peer.SupportFastPeer = SupportFastPeer;
            peer.IsHandshaked = true;
        }

       private string GetPeerIdString()
       {
           string result;
           if (PeerId[0] == (byte)'-')
           {
               if (PeerId[1] == (byte) 'U' && PeerId[2] == (byte) 'T')
               {
                   result = string.Format("uTorrent {0}", GetAzureusStyleVersion());
               }

               else if (PeerId[1] == (byte) 'A' && PeerId[2] == (byte) 'Z')
               {
                   result = string.Format("Vuze {0}", GetAzureusStyleVersion());
               }
               else if (PeerId[1] == (byte) 'B' && PeerId[2] == (byte) 'C')
               {
                   result = string.Format("BitComet {0}", GetAzureusStyleVersion());
               }
               else
               {
                   result = "Unknown";
               }
           }
           else if (PeerId[0] == 'M')
           {
               result = string.Format("BitTorrent {0}", GetMainlineStyleVersion());
           }
           else
           {
               result = "Unknown";
           }
           return result;
       }

        private string GetMainlineStyleVersion()
        {
            char version1 = (char)(PeerId[1]);
            char version2 = (char)(PeerId[3]);
            char version3 = (char)(PeerId[5]);

            string result = string.Format("{0}.{1}.{2}", version1, version2, version3);
            return result;
        }

        private string GetAzureusStyleVersion()
        {
            char version1 = (char)(PeerId[3]);
            char version2 = (char)(PeerId[4]);
            char version3 = (char)(PeerId[5]);
            char version4 = (char)(PeerId[6]);

            string result = string.Format("{0}.{1}.{2}.{3}", version1, version2, version3, version4);
            return result;
        }

        public override string ToString()
        {
            string result = string.Format("Handshake message: InfoHash:{0}, PeerId:{1}", InfoHash.ToHexString(),
                                          PeerId.ToHexString());
            return result;
        }
    }
}
