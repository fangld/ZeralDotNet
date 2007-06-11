using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numeric;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent
{
    public class TorrentFileParser
    {
        private TorrentFile torrentFile;
        private DictionaryHandler rootNode;
        private DictionaryHandler infoNode;
        private ByteArrayHandler announceNode;

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
                rootNode = BEncode.Decode(source) as DictionaryHandler;
                if (rootNode == null)
                {
                    announceNode = (ByteArrayHandler)FileDecode.GetHandler("announce", rootNode,false);
                    this.torrentFile.AddAnnounce(announceNode.StringValue);
                    infoNode = (DictionaryHandler)FileDecode.GetHandler("info", rootNode,false);
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
                announceNode = (ByteArrayHandler)FileDecode.GetHandler("announce", rootNode, false);
                this.torrentFile.AddAnnounce(announceNode.StringValue);
                infoNode = (DictionaryHandler)FileDecode.GetHandler("info", rootNode, false);
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
                IntHandler creationDateHandler = (IntHandler)FileDecode.GetHandler("creation date", rootNode);
                if (creationDateHandler != null)
                    this.torrentFile.CreationDate = (int)creationDateHandler.Value;

                ByteArrayHandler commentHandler = (ByteArrayHandler)FileDecode.GetHandler("comment", rootNode);
                if (commentHandler != null)
                    this.torrentFile.Comment = commentHandler.StringValue;

                ByteArrayHandler createdByHandler = (ByteArrayHandler)FileDecode.GetHandler("created by", rootNode);
                if (createdByHandler != null)
                    this.torrentFile.CreatedBy = createdByHandler.StringValue;
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
                IntHandler pieceLengthHandler = (IntHandler)FileDecode.GetHandler("piece length", infoNode);
                torrentFile.PieceLength = (int)pieceLengthHandler.Value;
                
                ByteArrayHandler piecesHandler = (ByteArrayHandler)FileDecode.GetHandler("pieces", infoNode);

                ByteArrayHandler nameHandler = (ByteArrayHandler)FileDecode.GetHandler("name", infoNode);

                IntHandler lengthHandler = (IntHandler)FileDecode.GetHandler("length", infoNode, false);
                if (lengthHandler != null)
                {
                    GetSingleFileInfoOptional();
                }

                else
                {
                    ListHandler filesHandler = (ListHandler)FileDecode.GetHandler("files", infoNode);
                    
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
                IntHandler privateHandler = (IntHandler)FileDecode.GetHandler("private", infoNode);
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
                ByteArrayHandler md5sumHandler = (ByteArrayHandler)FileDecode.GetHandler("md5sum", infoNode);
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
