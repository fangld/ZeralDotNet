using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class Download
    {
        public static Parameters Parameters;
        public static IChoker Choker;
        public static StorageWrapper StorageWrapper;

        public static void StartDownload(Parameters parameters, Flag doneFlag, StatusDelegate statusFunction, ErrorDelegate errorFunction, FinishedDelegate finishedFunction)
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
                stream.Read(response, 0, (int) length);
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
                    //访问文件夹
                    BytestringHandler nameNode = infoNode["name"] as BytestringHandler;
                    if (nameNode == null)
                    {
                        return;
                    }
                    file = @"C:\torrent\" + nameNode.StringText;
                    // if this path exists, and no files from the info dict exist, we assume it's a new download and
                    // the user wants to create a new directory with the default name
                    bool existed = false;
                    if (Directory.Exists(file))
                    {
                        foreach (Handler handler in filesNode)
                        {
                            DictionaryHandler fileNode = handler as DictionaryHandler;
                            ListHandler pathNode = fileNode["path"] as ListHandler;
                            if (File.Exists(Path.Combine(file, (pathNode[0] as BytestringHandler).StringText)))
                            {
                                existed = true;
                                break;
                            }
                        }

                        if (!existed)
                        {
                            file = Path.Combine(file, (infoNode["name"] as BytestringHandler).StringText);
                        }
                    }
                    Make(file, true);

                    // alert the UI to any possible change in path
                    //TODO: if (pathFunc != null)
                    // pathFunc(file)

                    foreach (Handler handler in filesNode)
                    {
                        DictionaryHandler fileNode = handler as DictionaryHandler;
                        ListHandler pathNode = fileNode["path"] as ListHandler;
                        string n = file;
                        foreach (Handler stringHandler in pathNode)
                        {
                            n = Path.Combine(n, (stringHandler as BytestringHandler).StringText);
                        }
                        files.Add(new BitFile(n, (fileNode["length"] as IntHandler).LongValue));
                        Make(n, false);
                    }
                }
            }
            catch
            {
                throw new BitTorrentException("Couldn't allocate directory...");
            }

            Flag finishFlag = new Flag();
            FinishedHelper finishedHelper = new FinishedHelper();
            finishedHelper.FinishedFunction = finishedFunction;
            finishedHelper.FinishFlag = finishFlag;

            string sID = DateTime.Now.ToLongDateString() + "www.wallywood.co.uk";
            byte[] myID = Globals.Sha1.ComputeHash(Encoding.Default.GetBytes(sID));

            byte[] piece = (infoNode["pieces"] as BytestringHandler).ByteArray;
            List<byte[]> pieces = new List<byte[]>();
            for (int i = 0; i < piece.Length; i += 20)
            {
                byte[] temp = new byte[20];
                Buffer.BlockCopy(piece, i, temp, 0, 20);
                pieces.Add(temp);
            }

            Storage storage = null;

            try
            {
                try
                {
                    storage = new Storage(files, Parameters.AllocatePause, statusFunction);
                    finishedHelper.Storage = storage;
                }
                catch(Exception ex)
                {
                    errorFunction("trouble accessing files - " + ex.Message);
                }
                IntHandler pieceLengthNode = infoNode["piece length"] as IntHandler;
                StorageWrapper = new StorageWrapper(storage, Parameters.DownloadSliceSize, pieces, pieceLengthNode.IntValue,
                    finishedHelper.Finished, finishedHelper.Failed, statusFunction, finishFlag, Parameters.CheckHashes,
                    finishedHelper.DataFlunked);
            }
            // Catch ValueError
            // failed("bad data")
            // catch IO Error
            catch(Exception ex)
            {
                finishedHelper.Failed("Problem - " + ex.Message);
            }

            if (finishFlag.IsSet)
            {
                return;
            }

            //RawServer rawServer = new RawServer(fi);
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
