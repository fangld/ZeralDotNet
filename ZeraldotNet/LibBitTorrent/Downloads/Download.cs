using System.IO;
using System.Collections.Generic;
using System.Net;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class Download
    {
        public static Parameters Parameters;
        public static IChoker Choker;
        public static StorageWrapper StorageWrapper;

        public static void StartDownload(Parameters parameters, Flag doneFlag, StatusDelegate statudFunction, ErrorDelegate errorFunction, FinishedDelegate finishedFunction)
        {
            if (parameters.ResponseFile.Length == 0 && parameters.Url.Length == 0)
            {
                throw new BitTorrentException("需要Response file 或者 Url");
            }

            Parameters = parameters;
            Stream stream = null;
            byte[] response;
            long length = 0;

            try
            {
                if (parameters.ResponseFile.Length != 0)
                {
                    stream = File.OpenRead(parameters.ResponseFile);
                    length = stream.Length;
                }

                else
                {
                    WebRequest webRequest = WebRequest.Create(parameters.Url);
                    WebResponse webResponse = webRequest.GetResponse();
                    stream = webResponse.GetResponseStream();
                    length = webResponse.ContentLength;
                }

                response = new byte[length];
                stream.Read(response, 0, (int)length);
            }

            catch
            {
                throw new BitTorrentException("Problem getting response info");
            }

            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            DictionaryHandler rootNode;
            try
            {
                rootNode = BEncode.Decode(response) as DictionaryHandler;
                BTFormat.CheckMessage(rootNode);
            }
            catch
            {
                throw new BitTorrentException("got bad file");
            }

            DictionaryHandler infoNode = rootNode["info"] as DictionaryHandler;
            List<BitFile> files = new List<BitFile>();
            string file;
            long fileLength;
            try
            {
                if (infoNode.ContainsKey("length"))
                {
                    fileLength = (infoNode["length"] as IntHandler).LongValue;
                    BytestringHandler nameNode = (infoNode["name"] as BytestringHandler);
                    if (nameNode == null)
                    {
                        return;
                    }
                    file = @"c:\torrent\" + nameNode.StringText;
                    Make(file, false);
                    files.Add(new BitFile(file, fileLength));
                }

                else
                {
                    fileLength = 0L;
                    ListHandler filesNode = infoNode["files"] as ListHandler;
                    foreach (Handler handler in filesNode)
                    {
                        DictionaryHandler fileNode = infoNode["files"] as DictionaryHandler;
                        fileLength += (fileNode["length"] as IntHandler).LongValue;
                    }
                }
            }
        }

        private static void Make(string filePath, bool forceDir)
        {
            if(!forceDir)
            {
                filePath = Path.GetDirectoryName(filePath);
            }

            if (filePath.Length != 0 && !Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }
    }
}
