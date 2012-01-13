using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public static class MetaInfoParser
    {
        public static MetaInfo Parse(string torrentFileName)
        {
            MetaInfo result =null;
            DictNode rootNode;
            byte[] source;

            if (File.Exists(torrentFileName))
            {
                source = File.ReadAllBytes(torrentFileName);
                rootNode = BEncoder.Decode(source) as DictNode;
            }
            else
            {
                return null;
            }
            
            if (rootNode != null)
            {
                DictNode infoNode = rootNode["info"] as DictNode;
                if (infoNode.ContainsKey("length"))
                {
                    result = GetSingleFileMetaInfo(rootNode, infoNode);
                }
                else
                {
                    result = GetMultiFileMetaInfo(rootNode, infoNode);
                }
            }

            return result;
        }

        private static SingleFileMetaInfo GetSingleFileMetaInfo(DictNode rootNode, DictNode infoNode)
        {
            SingleFileMetaInfo result = new SingleFileMetaInfo();

            DecodeRootNode(result, rootNode);
            DecodeInfoNode(result, infoNode);

            BytesNode nameNode = (BytesNode) infoNode["name"];
            result.Name = nameNode.StringText;

            IntNode lengthNode = (IntNode) infoNode["length"];
            result.Length = lengthNode.Value;

            if (infoNode.ContainsKey("md5sum"))
            {
                BytesNode md5SumNode = (BytesNode) infoNode ["md5sum"];
                result.Md5Sum = md5SumNode.StringText;
            }

            return result;
        }

        private static MultiFileMetaInfo GetMultiFileMetaInfo(DictNode rootNode, DictNode infoNode)
        {
            MultiFileMetaInfo result = new MultiFileMetaInfo();

            DecodeRootNode(result, rootNode);
            DecodeInfoNode(result, infoNode);

            BytesNode nameNode = (BytesNode)infoNode["name"];
            result.Name = nameNode.StringText;

            IntNode lengthNode = (IntNode)infoNode["length"];
            result.Length = lengthNode.Value;

            if (infoNode.ContainsKey("md5sum"))
            {
                BytesNode md5SumNode = (BytesNode)infoNode["md5sum"];
                result.Md5Sum = md5SumNode.StringText;
            }

            return result;
        }

        private static void DecodeRootNode(MetaInfo metaInfo, DictNode rootNode)
        {
            BytesNode annouceNode = (BytesNode)rootNode["annouce"];
            metaInfo.Annouce = annouceNode.StringText;

            if (rootNode.ContainsKey("announce-list"))
            {
                ListNode announceListNode = (ListNode)rootNode["announce-list"];
                foreach (BytesNode node in announceListNode)
                {
                    metaInfo.AddAnnounce(node.StringText);
                }
            }

            if (rootNode.ContainsKey("creation date"))
            {
                IntNode creationDataNode = (IntNode)rootNode["creation date"];
                metaInfo.CreationDate = DateTime.FromFileTime(creationDataNode.Value);
            }

            if (rootNode.ContainsKey("comment"))
            {
                BytesNode commentNode = (BytesNode) rootNode["comment"];
                metaInfo.Comment = commentNode.StringText;
            }

            if (rootNode.ContainsKey("created by"))
            {
                BytesNode createdByNode = (BytesNode)rootNode["created by"];
                metaInfo.CreatedBy = createdByNode.StringText;
            }

            if (rootNode.ContainsKey("encoding"))
            {
                BytesNode createdByNode = (BytesNode)rootNode["encoding"];
                metaInfo.Encoding = createdByNode.StringText;
            }
        }

        private static void DecodeInfoNode(MetaInfo metaInfo, DictNode infoNode)
        {
            IntNode pieceLengthNode = (IntNode) infoNode["piece length"];
            metaInfo.PieceLength = pieceLengthNode.Value;

            ListNode piecesNode = (ListNode) infoNode["pieces"];
            foreach (BytesNode node in piecesNode)
            {
                metaInfo.AddPiece(node.ByteArray);
            }

            if (infoNode.ContainsKey("private"))
            {
                IntNode privateNode = (IntNode)infoNode["private"];
                metaInfo.Private = privateNode.Value == 1;
            }
        }
    }
}
