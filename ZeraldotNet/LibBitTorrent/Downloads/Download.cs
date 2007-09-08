using System.IO;
using System.Net;
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

            try
            {
                byte[] response;
                long length;
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
        }
    }
}
