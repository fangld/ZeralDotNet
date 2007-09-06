using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numeric;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class TorrentFileParser
    {
        private TorrentFile torrentFile;
        private DictionaryHandler rootNode;
        private DictionaryHandler infoNode;
        private BytestringHandler announceNode;

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
                    announceNode = (BytestringHandler)FileDecode.GetHandler("announce", rootNode,false);
                    this.torrentFile.AddAnnounce(announceNode.StringText);
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
                announceNode = (BytestringHandler)FileDecode.GetHandler("announce", rootNode, false);
                this.torrentFile.AddAnnounce(announceNode.StringText);
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

                BytestringHandler commentHandler = (BytestringHandler)FileDecode.GetHandler("comment", rootNode);
                if (commentHandler != null)
                    this.torrentFile.Comment = commentHandler.StringText;

                BytestringHandler createdByHandler = (BytestringHandler)FileDecode.GetHandler("created by", rootNode);
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
                IntHandler pieceLengthHandler = (IntHandler)FileDecode.GetHandler("piece length", infoNode);
                torrentFile.PieceLength = (int)pieceLengthHandler.Value;
                
                BytestringHandler piecesHandler = (BytestringHandler)FileDecode.GetHandler("pieces", infoNode);

                BytestringHandler nameHandler = (BytestringHandler)FileDecode.GetHandler("name", infoNode);

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
                BytestringHandler md5sumHandler = (BytestringHandler)FileDecode.GetHandler("md5sum", infoNode);
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
