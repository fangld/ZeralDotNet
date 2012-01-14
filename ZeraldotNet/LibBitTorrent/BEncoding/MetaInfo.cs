using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public abstract class MetaInfo
    {
        #region Properties

        /// <summary>
        /// The announce URL of the tracker
        /// </summary>
        public string Annouce { get; set; }

        /// <summary>
        ///  the creation time of the torrent, in standard UNIX epoch format
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Free-form textual comments of the author 
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// name and version of the program used to create the .torrent 
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        ///  the string encoding format used to generate the _pieceList part of the info dictionary in the .torrent metafile
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// number of bytes in each piece 
        /// </summary>
        public long PieceLength { get; set; }

        /// <summary>
        /// this field is an integer. If it is set to "1", the client MUST publish its presence to get other peers ONLY via the trackers explicitly described in the metainfo file. If this field is set to "0" or is not present, the client may obtain peer from other means, e.g. PEX peer exchange, dht. Here, "private" may be read as "no external peer source". 
        /// </summary>
        public bool Private { get; set; }

        #endregion

        #region Constructors

        public MetaInfo()
        {
            _announceArrayList= new List<IList<string>>();
            _pieceList = new List<byte[]>();
        }

        #endregion

        #region AnnounceArrayList

        /// <summary>
        /// this is an extention to the official specification, offering backwards-compatibility
        /// </summary>
        private IList<IList<string>> _announceArrayList;

        public IList<string> GetAnnounceList(int index)
        {
            return _announceArrayList[index];
        }

        public void AddAnnounceArray(IList<string> announceArray)
        {
            _announceArrayList.Add(announceArray);
        }

        #endregion

        #region Pieces

        /// <summary>
        /// string consisting of the concatenation of all 20-byte SHA1 hash values, one per piece 
        /// </summary>
        private List<byte[]> _pieceList;

        public byte[] GetPiece(int index)
        {
            return _pieceList[index];
        }

        public void SetPieces(byte[] sourcePieces)
        {
            for (int i =0; i < sourcePieces.Length;i+=20)
            {
                byte[] piece = new byte[20];
                Array.Copy(sourcePieces, i, piece, 0, 20);
                _pieceList.Add(piece);
            }
        }

        #endregion
    }
}
