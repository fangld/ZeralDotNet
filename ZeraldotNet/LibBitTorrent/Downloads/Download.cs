using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Chokers;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class Download
    {
        public static Parameters Parameters;
        public static IChoker Choker;
        public static StorageWrapper StorageWrapper;

        public static void download(Parameters parameters, Flag doneFlag, StatusDelegate statudFunction, ErrorDelegate errorFunction, FinishedDelegate finishedFunction)
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
        }
    }
}
