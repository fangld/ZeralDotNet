using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class TorrentFileParser
    {
        private TorrentFile torrentFile;
        private DictNode rootNode;
        private DictNode infoNode;
        private BytesNode announceNode;

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">Torrent文件名</param>
        public TorrentFileParser(string torrentFileName)
        {
            rootNode = null;
            infoNode = null;
            announceNode = null;
            torrentFile = new TorrentFile();
            torrentFile.TorrentFileName = torrentFileName;
        }
        #endregion

        /// <summary>
        /// 开始解析文件
        /// </summary>
        /// <returns>返回是否解析文件成功</returns>
        public bool Parse()
        {
            byte[] source;

            if (File.Exists(torrentFile.TorrentFileName))
            {
                source = File.ReadAllBytes(torrentFile.TorrentFileName);
                rootNode = BEncode.Decode(source) as DictNode;
                if (rootNode == null)
                {
                    announceNode = (BytesNode)FileDecode.GetHandler("announce", rootNode,false);
                    this.torrentFile.AddAnnounce(announceNode.StringText);
                    infoNode = (DictNode)FileDecode.GetHandler("info", rootNode,false);
                }
                else
                {
                    throw new BitTorrentException("文件错误");
                }
            }
            else
            {
                throw new BitTorrentException("不存在这个文件");
            }
            return true;
        }

        public bool GetRootNecessary()
        {
            try
            {
                announceNode = (BytesNode)FileDecode.GetHandler("announce", rootNode, false);
                this.torrentFile.AddAnnounce(announceNode.StringText);
                infoNode = (DictNode)FileDecode.GetHandler("info", rootNode, false);
            }
            catch (BitTorrentException)
            {
                return false;
            }
            return true;
        }


        public bool GetRootOptional()
        {
            try
            {
                IntNode creationDateHandler = (IntNode)FileDecode.GetHandler("creation date", rootNode);
                if (creationDateHandler != null)
                    this.torrentFile.CreationDate = (int)creationDateHandler.LongValue;

                BytesNode commentHandler = (BytesNode)FileDecode.GetHandler("comment", rootNode);
                if (commentHandler != null)
                    this.torrentFile.Comment = commentHandler.StringText;

                BytesNode createdByHandler = (BytesNode)FileDecode.GetHandler("created by", rootNode);
                if (createdByHandler != null)
                    this.torrentFile.CreatedBy = createdByHandler.StringText;
            }

            catch (BitTorrentException)
            {
                return false;
            }

            return true;
        }

        public bool GetInfoNecessary()
        {
            try
            {
                IntNode pieceLengthHandler = (IntNode)FileDecode.GetHandler("piece length", infoNode);
                torrentFile.PieceLength = (int)pieceLengthHandler.LongValue;
                
                BytesNode piecesHandler = (BytesNode)FileDecode.GetHandler("pieces", infoNode);

                BytesNode nameHandler = (BytesNode)FileDecode.GetHandler("name", infoNode);

                IntNode lengthHandler = (IntNode)FileDecode.GetHandler("length", infoNode, false);
                if (lengthHandler != null)
                {
                    GetSingleFileInfoOptional();
                }

                else
                {
                    ListNode filesHandler = (ListNode)FileDecode.GetHandler("files", infoNode);
                    
                }

                throw new NotImplementedException("This is not implement!");
            }

            catch (BitTorrentException)
            {
                return false;
            }

            return true;
        }

        public bool GetInfoOptional()
        {
            try
            {
                IntNode privateHandler = (IntNode)FileDecode.GetHandler("private", infoNode);
            }
            catch (BitTorrentException)
            {
                return false;
            }
            return true;
        }

        private bool GetSingleFileInfoOptional()
        {
            try
            {
                BytesNode md5sumHandler = (BytesNode)FileDecode.GetHandler("md5sum", infoNode);
            }
            catch (BitTorrentException)
            {
                return false;
            }
            return true;
        }

        private bool GetMultiFileInfoOptional()
        {
            throw new NotImplementedException("This is not implement!");
        }
    }
}
