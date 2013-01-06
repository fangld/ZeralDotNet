using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZeraldotNet.Utility;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public enum MetaInfoMode
    {
        SingleFile,
        MultiFile
    }

    /// <summary>
    /// Meta info
    /// </summary>
    public abstract class MetaInfo
    {
        #region Properties

        /// <summary>
        /// The main announce URL of the tracker
        /// </summary>
        public string Announce { get; set; }

        /// <summary>
        /// The creation time of the torrent (standard UNIX epoch format)
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Free-form textual comments of the author 
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Name and version of the program used to create the torrent 
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// The string encoding format used to generate the pieceList part of the info dictionary in the .torrent metafile
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// The number of bytes in each piece 
        /// </summary>
        public int PieceLength { get; set; }

        /// <summary>
        /// If it is set to "1", the client MUST publish its presence to get other peers ONLY via the trackers explicitly described in the metainfo file. If this field is set to "0" or is not present, the client may obtain peer from other means, e.g. PEX peer exchange, dht. Here, "private" may be read as "no external peer source". 
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AnnounceArrayListCount
        {
            get { return _announceArrayList.Count; }
        }

        /// <summary>
        /// The count of pieces
        /// </summary>
        public int PieceListCount
        {
            get { return _hashArray.Length; }
        }

        /// <summary>
        /// The hash values of file information
        /// </summary>
        public byte[] InfoHash { get; set; }

        /// <summary>
        /// the mode of metainfo
        /// </summary>
        public abstract MetaInfoMode Mode { get; }

        /// <summary>
        /// Return the length of files
        /// </summary>
        public abstract long SumLength { get; }

        #endregion

        #region Constructors

        public MetaInfo()
        {
            _announceArrayList = new List<IList<string>>();
            //_hashList = new List<byte[]>();
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
        /// Store the hash values of each piece
        /// </summary>
        private byte[][] _hashArray;

        /// <summary>
        /// Get the hash values of the index-th piece
        /// </summary>
        /// <param name="index">the index of piece</param>
        /// <returns>Return the SHA1 hash values of the index-th piece</returns>
        public byte[] GetHashValues(int index)
        {
            return _hashArray[index];
        }

        /// <summary>
        /// Set the hash values of all pieces
        /// </summary>
        /// <param name="fullHashValues">The successive hash values of all pieces</param>
        public void SetFullHashValues(byte[] fullHashValues)
        {
            int pieceCount = fullHashValues.Length/Globals.Sha1HashLength;
            _hashArray = new byte[pieceCount][];

            for (int i = 0, offset = 0; i < pieceCount; i++, offset += 20)
            {
                byte[] pieceHash = new byte[Globals.Sha1HashLength];
                Buffer.BlockCopy(fullHashValues, offset, pieceHash, 0, Globals.Sha1HashLength);
                _hashArray[i] = pieceHash;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parse meta-info from torrent file.
        /// </summary>
        /// <param name="fileName">The filename of torrent</param>
        /// <returns>Return the metainfo</returns>
        public static MetaInfo Parse(string fileName)
        {
            MetaInfo result = null;
            DictNode rootNode;
            byte[] source;

            if (File.Exists(fileName))
            {
                source = File.ReadAllBytes(fileName);
                rootNode = BEncodingFactory.Decode(source) as DictNode;
                Debug.Assert(rootNode != null);
            }
            else
            {
                return null;
            }

            DictNode infoNode = rootNode["info"] as DictNode;
            Debug.Assert(infoNode != null);

            if (infoNode.ContainsKey("length"))
            {
                result = GetSingleFileMetaInfo(rootNode, infoNode);
            }
            else
            {
                result = GetMultiFileMetaInfo(rootNode, infoNode);
            }

            result.InfoHash = Globals.GetSha1Hash(BEncodingFactory.ByteArrayEncode(rootNode["info"]));//Globals.Sha1.ComputeHash(BEncodingFactory.ByteArrayEncode(rootNode["info"]));

            return result;
        }

        /// <summary>
        /// Get the single file mode meta information
        /// </summary>
        /// <param name="rootNode">the root node of torrent</param>
        /// <param name="infoNode">the info node of torrent</param>
        /// <returns>Return the single file mode meta information</returns>
        private static SingleFileMetaInfo GetSingleFileMetaInfo(DictNode rootNode, DictNode infoNode)
        {
            SingleFileMetaInfo result = new SingleFileMetaInfo();

            DecodeRootNode(result, rootNode);
            DecodeInfoNode(result, infoNode);

            BytesNode nameNode = infoNode["name"] as BytesNode;
            Debug.Assert(nameNode != null);
            result.Name = nameNode.StringText;

            IntNode lengthNode = infoNode["length"] as IntNode;
            Debug.Assert(lengthNode != null);
            result.Length = lengthNode.Value;

            if (infoNode.ContainsKey("md5sum"))
            {
                BytesNode md5SumNode = infoNode["md5sum"] as BytesNode;
                Debug.Assert(md5SumNode != null);
                result.Md5Sum = md5SumNode.StringText;
            }
            else
            {
                result.Md5Sum = String.Empty;
            }

            return result;
        }

        /// <summary>
        /// Get the multi file mode meta information
        /// </summary>
        /// <param name="rootNode">the root node of torrent</param>
        /// <param name="infoNode">the info node of torrent</param>
        /// <returns>Return the multi file mode meta information</returns>
        private static MultiFileMetaInfo GetMultiFileMetaInfo(DictNode rootNode, DictNode infoNode)
        {
            MultiFileMetaInfo result = new MultiFileMetaInfo();

            DecodeRootNode(result, rootNode);
            DecodeInfoNode(result, infoNode);

            BytesNode nameNode = infoNode["name"] as BytesNode;
            Debug.Assert(nameNode != null);
            result.Name = nameNode.StringText;

            ListNode filesNode = infoNode["files"] as ListNode;
            Debug.Assert(filesNode != null);
            long begin = 0;
            FileInfo[] fileInfoArray = new FileInfo[filesNode.Count];
            for (int i = 0; i < filesNode.Count; i++)
            {
                DictNode node = filesNode[i] as DictNode;

                IntNode lengthNode = node["length"] as IntNode;
                Debug.Assert(lengthNode != null);
                FileInfo fileInfo = new FileInfo();
                fileInfo.Length = lengthNode.Value;
                fileInfo.Begin = begin;
                fileInfo.End = begin + fileInfo.Length;
                begin = fileInfo.End;

                ListNode pathNode = node["path"] as ListNode;
                Debug.Assert(pathNode != null);
                StringBuilder sb = new StringBuilder();

                BytesNode firstPathNode = pathNode[0] as BytesNode;
                Debug.Assert(firstPathNode != null);
                sb.Append(firstPathNode.StringText);

                for (int j = 1; j < pathNode.Count; j++)
                {
                    BytesNode subPathNode = pathNode[j] as BytesNode;
                    Debug.Assert(subPathNode != null);
                    sb.AppendFormat(@"\{0}", subPathNode.StringText);
                }

                fileInfo.Path = sb.ToString();

                if (!node.ContainsKey("md5sum"))
                {
                    fileInfo.Md5Sum = String.Empty;
                }
                else
                {
                    BytesNode md5SumNode = node["md5sum"] as BytesNode;
                    Debug.Assert(md5SumNode != null);
                    fileInfo.Md5Sum = md5SumNode.StringText;
                }

                fileInfoArray[i] = fileInfo;
            }

            result.SetFileInfoArray(fileInfoArray);

            return result;
        }

        private static void DecodeRootNode(MetaInfo metaInfo, DictNode rootNode)
        {
            BytesNode annouceNode = rootNode["announce"] as BytesNode;
            Debug.Assert(annouceNode != null);
            metaInfo.Announce = annouceNode.StringText;

            if (rootNode.ContainsKey("announce-list"))
            {
                ListNode announceListNode = rootNode["announce-list"] as ListNode;
                Debug.Assert(announceListNode != null);
                foreach (ListNode announceArrayNode in announceListNode)
                {
                    Debug.Assert(announceArrayNode != null);
                    IList<string> announceArray = new List<string>();
                    foreach (BytesNode node in announceArrayNode)
                    {
                        Debug.Assert(node != null);
                        announceArray.Add(node.StringText);
                    }
                    metaInfo.AddAnnounceArray(announceArray);
                }
            }

            if (rootNode.ContainsKey("creation date"))
            {
                IntNode creationDataNode = rootNode["creation date"] as IntNode;
                Debug.Assert(creationDataNode != null);
                metaInfo.CreationDate = creationDataNode.Value.FromUnixEpochFormat();
            }
            else
            {
                metaInfo.CreationDate = new DateTime(1970, 1, 1);
            }

            if (rootNode.ContainsKey("comment"))
            {
                BytesNode commentNode = rootNode["comment"] as BytesNode;
                Debug.Assert(commentNode != null);
                metaInfo.Comment = commentNode.StringText;
            }
            else
            {
                metaInfo.Comment = String.Empty;
            }

            if (rootNode.ContainsKey("created by"))
            {
                BytesNode createdByNode = rootNode["created by"] as BytesNode;
                Debug.Assert(createdByNode != null);
                metaInfo.CreatedBy = createdByNode.StringText;
            }
            else
            {
                metaInfo.CreatedBy = String.Empty;
            }

            if (rootNode.ContainsKey("encoding"))
            {
                BytesNode encodingNode = rootNode["encoding"] as BytesNode;
                Debug.Assert(encodingNode != null);
                metaInfo.Encoding = encodingNode.StringText;
            }
            else
            {
                metaInfo.Encoding = "ASCII";
            }
        }

        private static void DecodeInfoNode(MetaInfo metaInfo, DictNode infoNode)
        {
            IntNode pieceLengthNode = infoNode["piece length"] as IntNode;
            Debug.Assert(pieceLengthNode != null);
            metaInfo.PieceLength = pieceLengthNode.IntValue;

            BytesNode piecesNode = infoNode["pieces"] as BytesNode;
            Debug.Assert(piecesNode != null);
            metaInfo.SetFullHashValues(piecesNode.ByteArray);

            if (infoNode.ContainsKey("private"))
            {
                IntNode privateNode = infoNode["private"] as IntNode;
                Debug.Assert(privateNode != null);
                metaInfo.Private = privateNode.Value == 1;
            }
        }

        #endregion
    }
}