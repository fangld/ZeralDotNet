using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.PiecePickers;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent.ReRequesters;
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

            DictNode rootNode;
            try
            {
                rootNode = BEncoder.Decode(response) as DictNode;
                BTFormat.CheckMessage(rootNode);
            }
            catch
            {
                throw new BitTorrentException("got bad file");
            }

            DictNode infoNode = rootNode["info"] as DictNode;
            List<BitFile> files = new List<BitFile>();
            string file;
            long fileLength;
            try
            {
                if (infoNode.ContainsKey("length"))
                {
                    fileLength = (infoNode["length"] as IntNode).Value;
                    BytesNode nameNode = (infoNode["name"] as BytesNode);
                    if (nameNode == null)
                    {
                        return;
                    }
                    file = @"k:\torrent\" + nameNode.StringText;
                    Make(file, false);
                    files.Add(new BitFile(file, fileLength));
                }

                else
                {
                    fileLength = 0L;
                    ListNode filesNode = infoNode["files"] as ListNode;
                    foreach (BEncodedNode handler in filesNode)
                    {
                        DictNode fileNode = infoNode["files"] as DictNode;
                        fileLength += (fileNode["length"] as IntNode).Value;
                    }
                    //访问文件夹
                    BytesNode nameNode = infoNode["name"] as BytesNode;
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
                        foreach (BEncodedNode handler in filesNode)
                        {
                            DictNode fileNode = handler as DictNode;
                            ListNode pathNode = fileNode["path"] as ListNode;
                            if (File.Exists(Path.Combine(file, (pathNode[0] as BytesNode).StringText)))
                            {
                                existed = true;
                                break;
                            }
                        }

                        if (!existed)
                        {
                            file = Path.Combine(file, (infoNode["name"] as BytesNode).StringText);
                        }
                    }
                    Make(file, true);

                    // alert the UI to any possible change in path
                    //TODO: if (pathFunc != null)
                    // pathFunc(file)

                    foreach (BEncodedNode handler in filesNode)
                    {
                        DictNode fileNode = handler as DictNode;
                        ListNode pathNode = fileNode["path"] as ListNode;
                        string n = file;
                        foreach (BEncodedNode stringHandler in pathNode)
                        {
                            n = Path.Combine(n, (stringHandler as BytesNode).StringText);
                        }
                        files.Add(new BitFile(n, (fileNode["length"] as IntNode).Value));
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
            finishedHelper.ErrorFunction = errorFunction;
            finishedHelper.FinishedFunction = finishedFunction;
            finishedHelper.DoneFlag = finishFlag;

            string sID = DateTime.Now.ToLongDateString() + "www.wallywood.co.uk";
            byte[] myID = Globals.Sha1.ComputeHash(Encoding.Default.GetBytes(sID));

            byte[] piece = (infoNode["pieces"] as BytesNode).ByteArray;
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
                    storage = new Storage(files, parameters.AllocatePause, statusFunction);
                    finishedHelper.Storage = storage;
                }
                catch(Exception ex)
                {
                    errorFunction("trouble accessing files - " + ex.Message);
                }
                IntNode pieceLengthNode = infoNode["piece length"] as IntNode;
                StorageWrapper = new StorageWrapper(storage, parameters.DownloadSliceSize, pieces, (int)pieceLengthNode.Value,
                    finishedHelper.Finished, finishedHelper.Failed, statusFunction, finishFlag, parameters.CheckHashes,
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

            RawServer rawServer = new RawServer(finishFlag, parameters.TimeoutCheckInterval, parameters.Timeout, false);
            if (parameters.MaxPort < parameters.MinPort)
            {
                int temp = parameters.MinPort;
                parameters.MinPort = parameters.MaxPort;
                parameters.MaxPort = parameters.MinPort;
            }

            ushort listenPort;
            for (listenPort = parameters.MinPort; listenPort <= parameters.MaxPort; listenPort++)
            {
                try
                {
                    rawServer.Bind(listenPort, parameters.Bind, false);
                    break;
                }
                catch(SocketException)
                {
                    //TODO: Error Code
                }
            }

            //TODO: Check whether nothing bound
            Choker = new Choker(parameters.MaxUploads, rawServer.AddTask, finishFlag);
            Measure uploadMeasure = new Measure(parameters.MaxRatePeriod, parameters.UploadRateFudge);
            Measure downloadMeasure = new Measure(parameters.MaxRatePeriod);
            RateMeasure rateMeasure = new RateMeasure(StorageWrapper.LeftLength);
            Downloader downloader =
                new NormalDownloader(StorageWrapper, new PiecePicker(pieces.Count), parameters.RequestBackLog,
                                     parameters.MaxRatePeriod, pieces.Count, downloadMeasure, parameters.SnubTime,
                                     rateMeasure.DataCameIn);

            Connecter connecter =
                new Connecter(downloader, Choker, pieces.Count, StorageWrapper.IsEverythingPending, uploadMeasure,
                              parameters.MaxUploadRate << 10, rawServer.AddTask);

            byte[] infoHash = Globals.Sha1.ComputeHash(BEncoder.ByteArrayEncode(infoNode));

            Encrypter encrypter = new Encrypter(connecter, rawServer, myID, parameters.MaxMessageLength, rawServer.AddTask,
                parameters.KeepAliveInterval, infoHash, parameters.MaxInitiate);
            ReRequester reRequester =
                new ReRequester((rootNode["announce"] as BytesNode).StringText, parameters.RerequestInterval,
                                rawServer.AddTask, connecter.GetConnectionsCount, parameters.MinPeers,
                                encrypter.StartConnect, rawServer.AddExternalTask,
                                StorageWrapper.GetLeftLength, uploadMeasure.GetTotalLength, downloadMeasure.GetTotalLength,
                                listenPort, parameters.IP,
                                myID, infoHash, parameters.HttpTimeout, null, parameters.MaxInitiate, finishFlag);

            DownloaderFeedback downloaderFeedback =
                new DownloaderFeedback(Choker, rawServer.AddTask, statusFunction, uploadMeasure.GetUpdatedRate,
                                       downloadMeasure.GetUpdatedRate, rateMeasure.GetTimeLeft, rateMeasure.GetLeftTime,
                                       fileLength, finishFlag, parameters.DisplayInterval,
                                       parameters.Spew);

            statusFunction("connection to peers", -1, -1, -1, -1);
            //TODO: finishedHelper.errorfunc	

            finishedHelper.FinishFlag = finishFlag;
            finishedHelper.ReRequester = reRequester;
            finishedHelper.RateMeasure = rateMeasure;

            reRequester.d(0);
            rawServer.ListenForever(encrypter);
            reRequester.Announce(2, null);
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
