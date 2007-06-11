using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class TorrentFile
    {
        /// <summary>
        /// Torrent文件名
        /// </summary>
        private string torrentFileName;

        /// <summary>
        /// 访问和设置Torrent文件名
        /// </summary>
        public string TorrentFileName
        {
            get { return this.torrentFileName; }
            set { this.torrentFileName = value; }
        }

        /// <summary>
        /// 文件注释
        /// </summary>
        private string comment;

        /// <summary>
        /// 访问和设置文件注释
        /// </summary>
        public string Comment
        {
            get { return this.comment; }
            set { this.comment = value; }
        }

        /// <summary>
        /// 创建该文件的程序名称
        /// </summary>
        private string createdBy;

        /// <summary>
        /// 访问和设置创建该文件的程序名称
        /// </summary>
        public string CreatedBy
        {
            get { return this.createdBy; }
            set { this.createdBy = value; }
        }

        /// <summary>
        /// 创建该文件的时间
        /// </summary>
        private int creationDate;

        /// <summary>
        /// 访问和设置创建该文件的时间
        /// </summary>
        public int CreationDate
        {
            get { return this.creationDate; }
            set { this.creationDate = value; }
        }

        /// <summary>
        /// 文件的分块大小
        /// </summary>
        private int pieceLength;

        /// <summary>
        /// 访问和设置文件的分块大小
        /// </summary>
        public int PieceLength
        {
            get { return this.pieceLength; }
            set { this.pieceLength = value; }
        }

        private List<byte[]> pieces;

        public byte[] GetPiece(int index)
        {
            return pieces[index];
        }

        public void SetPiece(byte[] sourcePiece, int index)
        {
            if (sourcePiece.Length == 20)
            {
                sourcePiece.CopyTo(pieces[index], 0);
            }
            else
            {
                throw new BitTorrentException("设置的Piece的长度不为20");
            }
        }

        private List<string> announce;

        public string GetAnnounce(int index)
        {
            return announce[index];
        }

        public void SetAnnounce(string announceName, int index)
        {
            announce[index] = announceName;
        }

        public void AddAnnounce(string announceName)
        {
            announce.Add(announceName);
        }

        /// <summary>
        /// 是否支持保密协议
        /// </summary>
        private bool isPrivate;

        /// <summary>
        /// 访问和设置是否支持保密协议
        /// </summary>
        private bool IsPrivate
        {
            get { return this.isPrivate; }
            set { this.isPrivate = value; }
        }
    }
}
