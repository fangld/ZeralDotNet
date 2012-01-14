using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using ZeraldotNet.Utility;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public static class MetaInfoParser
    {
        public static MetaInfo Parse(string torrentFileName)
        {
            MetaInfo result = null;
            DictNode rootNode;
            byte[] source;

            if (File.Exists(torrentFileName))
            {
                source = File.ReadAllBytes(torrentFileName);
                rootNode = BEncoder.Decode(source) as DictNode;
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

            return result;
        }

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
                BytesNode md5SumNode = infoNode ["md5sum"] as BytesNode;
                Debug.Assert(md5SumNode != null);
                result.Md5Sum = md5SumNode.StringText;
            }
            else
            {
                result.Md5Sum = string.Empty;
            }

            return result;
        }

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
            foreach (DictNode node in filesNode)
            {
                FileInfo fileInfo = new FileInfo();
                IntNode lengthNode = node["length"] as IntNode;
                Debug.Assert(lengthNode != null);
                fileInfo.Length = lengthNode.Value;

                ListNode pathNode = node["path"] as ListNode;
                Debug.Assert(pathNode != null);
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < pathNode.Count - 1; i++)
                {
                    BytesNode subPathNode = pathNode[i] as BytesNode;
                    Debug.Assert(subPathNode != null);
                    sb.AppendFormat(@"\{0}", subPathNode.StringText);
                }
                BytesNode fileNameNode = pathNode[pathNode.Count - 1] as BytesNode;
                Debug.Assert(fileNameNode != null);
                sb.Append(fileNameNode.StringText);
                fileInfo.Path = sb.ToString();

                if (node.ContainsKey("md5sum"))
                {
                    BytesNode md5SumNode = node["md5sum"] as BytesNode;
                    Debug.Assert(md5SumNode != null);
                    fileInfo.Md5Sum = md5SumNode.StringText;
                }
                else
                {
                    fileInfo.Md5Sum = string.Empty;
                }
                result.AddFileInfo(fileInfo);
            }


            return result;
        }

        private static void DecodeRootNode(MetaInfo metaInfo, DictNode rootNode)
        {
            BytesNode annouceNode = rootNode["announce"] as BytesNode;
            Debug.Assert(annouceNode != null);
            metaInfo.Annouce = annouceNode.StringText;

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
                metaInfo.CreationDate = DateTimeExtension.FromUnixEpochFormat(creationDataNode.Value);
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
                metaInfo.Comment = string.Empty;
            }

            if (rootNode.ContainsKey("created by"))
            {
                BytesNode createdByNode = rootNode["created by"] as BytesNode;
                Debug.Assert(createdByNode != null);
                metaInfo.CreatedBy = createdByNode.StringText;
            }
            else
            {
                metaInfo.CreatedBy = string.Empty;
            }

            if (rootNode.ContainsKey("encoding"))
            {
                BytesNode encodingNode = rootNode["encoding"] as BytesNode;
                Debug.Assert(encodingNode != null);
                metaInfo.Encoding = encodingNode.StringText;
            }
            else
            {
                metaInfo.Encoding = "UTF-8";
            }
        }

        private static void DecodeInfoNode(MetaInfo metaInfo, DictNode infoNode)
        {
            IntNode pieceLengthNode = infoNode["piece length"] as IntNode;
            Debug.Assert(pieceLengthNode != null);
            metaInfo.PieceLength = pieceLengthNode.Value;

            BytesNode piecesNode = infoNode["pieces"] as BytesNode;
            Debug.Assert(piecesNode != null);
            metaInfo.SetPieces(piecesNode.ByteArray);

            if (infoNode.ContainsKey("private"))
            {
                IntNode privateNode = infoNode["private"] as IntNode;
                Debug.Assert(privateNode != null);
                metaInfo.Private = privateNode.Value == 1;
            }
        }
    }
}
