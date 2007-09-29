using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class FileDecode
    {       

        private static Dictionary<string, Type> handlerTypeArray = new Dictionary<string,Type>();
        private static Dictionary<string, bool> isOptional = new Dictionary<string, bool>();

        static FileDecode()
        {
            handlerTypeArray = new Dictionary<string, Type>(14);
            isOptional = new Dictionary<string, bool>(14);

            intType = Type.GetType("IntHandler");
            dictType = Type.GetType("DictionaryHandler");
            listType = Type.GetType("ListHandler");
            byteType = Type.GetType("ByteArrayHandler");

            handlerTypeArray.Add("announce", byteType);
            isOptional.Add("announce", false);

            handlerTypeArray.Add("info", dictType);
            isOptional.Add("info", false);

            handlerTypeArray.Add("announce-list", listType);
            isOptional.Add("announce-list", true);

            handlerTypeArray.Add("creation date", intType);
            isOptional.Add("creation date", true);

            handlerTypeArray.Add("comment", byteType);
            isOptional.Add("comment", true);

            handlerTypeArray.Add("created by", byteType);
            isOptional.Add("created by", true);

            handlerTypeArray.Add("piece length", intType);
            isOptional.Add("piece length", false);

            handlerTypeArray.Add("pieces", byteType);
            isOptional.Add("pieces", false);

            handlerTypeArray.Add("private", intType);
            isOptional.Add("private", true);

            handlerTypeArray.Add("name", byteType);
            isOptional.Add("name", false);

            handlerTypeArray.Add("length", intType);
            isOptional.Add("length", false);

            handlerTypeArray.Add("md5sum", byteType);
            isOptional.Add("md5num", true);

            handlerTypeArray.Add("files", listType);
            isOptional.Add("files", true);

            handlerTypeArray.Add("path", listType);
            isOptional.Add("path", true);
        }

        private static Type intType;
        private static Type dictType;
        private static Type listType;
        private static Type byteType;

        public static BEncodedNode GetHandler(string key, DictNode rootNode)
        {
        return GetHandler(key, rootNode, isOptional[key]);
        }

        public static BEncodedNode GetHandler(string key, DictNode rootNode, bool isOptional)
        {
            Type handlerType = handlerTypeArray[key];

            BEncodedNode result;
            if (rootNode.ContainsKey(key))
            {
                result = rootNode[key];
                if (result.GetType() == handlerType)
                {
                    throw new BitTorrentException(String.Format("{0}{1}{2}", "文件的", key, "部分错误"));
                }
            }
            else if (isOptional)
            {
                result = null;
            }
            else
            {
                throw new BitTorrentException(String.Format("{0}{1}{2}", "文件的", key, "部分错误"));
            }
            return result;
        }
    }
}
