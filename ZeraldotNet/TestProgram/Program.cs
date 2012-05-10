using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Trackers;

namespace TestProgram
{
    internal class Program
    {
        private const string winedtTorrentFile = @"E:\Bittorrent\Torrents\winedt70.exe.torrent";
        private const string winedtSaveAsDirectory = @"E:\Winedt70";
        private const string sumatraPDFTorrentFile = @"E:\Bittorrent\Torrents\SumatraPDF-2.0.1-install.exe.torrent";
        private const string sumatraPDFSaveAsDirectory = @"E:\SumatraPDF";

        
        private static void Main(string[] args)
        {
            //TestMetaInfoParser();
            //TestTracker();
            //DetermineRcvFileCorrent();
            TestConnectClient();
            Console.ReadKey();
        }

        private static void TestMetaInfoParser()
        {
            MetaInfo result = MetaInfo.Parse(winedtTorrentFile);
            //Console.WriteLine(result.CreationDate.ToLocalTime());
            ShowMetaInfo(result);

            //torrentFileName = @"D:\Bittorrent\VS11_DP_CTP_ULT_ENU.torrent";
            //result = MetaInfo.Parse(torrentFileName);
            ////Console.WriteLine(result.CreationDate.ToLocalTime());
            //ShowMetaInfo(result);
        }

        private static void DetermineRcvFileCorrent()
        {
            int bufferSize = Setting.BlockSize*571;

            FileStream orgFs = new FileStream(@"D:\Latex\winedt70.exe", FileMode.OpenOrCreate);
            FileStream rcvFs = new FileStream(@"E:\Winedt70\winedt70.exe", FileMode.OpenOrCreate);

            byte[] orgBuffer = new byte[bufferSize];
            byte[] rcvBuffer = new byte[bufferSize];

            orgFs.Read(orgBuffer, 0, bufferSize);
            rcvFs.Read(rcvBuffer, 0, bufferSize);

            for (int i = 0; i < bufferSize; i++)
            {
                if (orgBuffer[i] != rcvBuffer[i])
                {
                    Console.WriteLine("index:{0}, org:{1}, rcv:{2}", i, orgBuffer[i], rcvBuffer[i]);
                }
            }
            Console.WriteLine("End");
        }

        private static void ShowMetaInfo(MetaInfo metaInfo)
        {
            //Announce
            Console.WriteLine("Announce:{0}", metaInfo.Announce);
            Console.WriteLine("-----------------------");

            //Announce array list
            Console.WriteLine("AnnounceArrayList:");
            for (int i = 0; i < metaInfo.AnnounceArrayListCount; i++)
            {
                Console.WriteLine("     {0}th array:", i + 1);
                IList<string> announceList = metaInfo.GetAnnounceList(i);
                for (int j = 0; j < announceList.Count; j++)
                {
                    Console.WriteLine("         {0}th announce:{1}", j + 1, announceList[j]);
                }
            }

            //byte[] pieceHash = metaInfo.InfoHash;
            //for (int i = 0; i < pieceHash.Length; i++)
            //{
            //    Console.Write("{0:X}{1:X}", (pieceHash[i] >> 4), pieceHash[i] & 0x0F);
            //}


            Console.WriteLine();
        }

        private static void TestEncoding()
        {
            Console.WriteLine(Encoding.UTF8.BodyName);
            Console.WriteLine(Encoding.ASCII.BodyName);
            Console.WriteLine(Encoding.Default.BodyName);
            Console.WriteLine(Encoding.Unicode.BodyName);
            Console.WriteLine(Encoding.UTF7.BodyName);
            Console.WriteLine(Encoding.UTF32.BodyName);
            Console.WriteLine(Encoding.GetEncoding("ASCII").BodyName);
        }

        private static async void TestConnectClient()
        {
            Task task1 = new Task();
            task1.TorrentFileName = winedtTorrentFile;
            task1.SaveAsDirectory = winedtSaveAsDirectory;
            task1.OnFinished += (sender1, args1) =>
                                  {
                                      Console.WriteLine("Task1 is finished");
                                      Task task2 = new Task();
                                      task2.TorrentFileName = sumatraPDFTorrentFile;
                                      task2.SaveAsDirectory = sumatraPDFSaveAsDirectory;
                                      task2.OnFinished += (sender2, args2) => Console.WriteLine("Task2 is finished");
                                      task2.Start();
                                  };
            task1.Start();


        }

        private static byte[] CreateBuffer()
        {
            var buffer = new byte[49 + 19];
            buffer[0] = 19;
            buffer[1] = (byte) 'B';
            buffer[2] = (byte) 'i';
            buffer[3] = (byte) 't';
            buffer[4] = (byte) 'T';
            buffer[5] = (byte) 'o';
            buffer[6] = (byte) 'r';
            buffer[7] = (byte) 'r';
            buffer[8] = (byte) 'e';
            buffer[9] = (byte) 'n';
            buffer[10] = (byte) 't';
            buffer[11] = (byte) ' ';
            buffer[12] = (byte) 'p';
            buffer[13] = (byte) 'r';
            buffer[14] = (byte) 'o';
            buffer[15] = (byte) 't';
            buffer[16] = (byte) 'o';
            buffer[17] = (byte) 'c';
            buffer[18] = (byte) 'o';
            buffer[19] = (byte) 'l';
            buffer[20] = 0;
            buffer[21] = 0;
            buffer[22] = 0;
            buffer[23] = 0;
            buffer[24] = 0;
            buffer[25] = 0;
            buffer[26] = 0;
            buffer[27] = 0;
            //string str = "88AD5F63DFC4E079E532765218EA81B74D837A01";
            //string str = "A40D685CE173EAEBBCB9EF1719A1893191A2DC78";
            string str = "8C072530D11984B2E6DB466D72C8E8B2F4F2C446"; //foobar
            StringBuilder sb = new StringBuilder();
            for (int i = 0, j = 28; i < str.Length; i += 2, j++)
            {
                string numberString = str.Substring(i, 2);
                buffer[j] = byte.Parse(numberString, NumberStyles.HexNumber);
            }

            byte[] peerId = Encoding.ASCII.GetBytes("-AZ2060-000000000000");
            Buffer.BlockCopy(peerId, 0, buffer, 48, 20);

            return buffer;
        }

        private static async void TestTracker()
        {
            string torrentFileName = @"D:\Bittorrent\winedt60.exe.torrent";
            MetaInfo metaInfo = MetaInfo.Parse(torrentFileName);

            Tracker tracker = new Tracker();
            tracker.Url = metaInfo.Announce;

            AnnounceRequest request = new AnnounceRequest();
            request.InfoHash = metaInfo.InfoHash;
            request.PeerId = Setting.GetPeerIdString();
            request.Compact = Setting.Compact;
            request.Port = Setting.Port;
            request.Uploaded = 0;
            request.Downloaded = 0;
            request.Event = EventMode.Started;

            AnnounceResponse response = await tracker.Announce(request);
            ShowAnnounceResponse(response);

            //WebRequest httpWebRequest = WebRequest.Create(uriString);

            //httpWebRequest.Method = "GET";

            //try
            //{
            //    using (WebResponse httpWebResponse = httpWebRequest.GetResponse())
            //    {
            //        Console.WriteLine("get response");
            //        Stream stream = httpWebResponse.GetResponseStream();
            //        Debug.Assert(stream != null);
            //        byte[] buffer = new byte[Setting.BufferSize];
            //        int count = 0;

            //        count = stream.Receive(buffer, 0, Setting.BufferSize);
            //        FileStream fs = new FileStream("d:\\a.dat", FileMode.OpenOrCreate);
            //        fs.Write(buffer, 0, count);
            //        fs.Flush();
            //        fs.Close();
            //        Console.WriteLine("count:{0}", count);
            //        Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, count));

            //        byte[] source = new byte[count];
            //        Buffer.BlockCopy(buffer, 0, source, 0, count);

            //        DictNode node = BEncoder.Parse(source) as DictNode;
            //        BytesNode peersNode = node["peers"] as BytesNode;
            //        //for (int i = 0; i < 6; i++)//peersNode.ByteArray.Length; i++)
            //        //{
            //        //    Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", i, (char)buffer[i], buffer[i]);
            //        //}
            //        byte[] bytes = peersNode.ByteArray;
            //        for (int i = 0; i < bytes.Length; i += 6)//peersNode.ByteArray.Length; i++)
            //        {

            //            Console.WriteLine("{0} {1} {2} {3} {4} {5}", bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5]);

            //            byte[] remoteAddress = new byte[4];
            //            byte[] localAddress = new byte[4];
            //            Buffer.BlockCopy(peersNode.ByteArray, i, remoteAddress, 0, 4);
            //            Buffer.BlockCopy(peersNode.ByteArray, i, localAddress, 0, 4);
            //            localAddress[3]--;

            //            //long address = (((long)(bytes[i])) << 24) + (bytes[i + 1] << 16) + (bytes[i + 2]<< 8) + bytes[i + 3];
            //            int port = ((int)bytes[i + 4]) * 256 + bytes[i + 5];
            //            if (port == 6881)
            //                return;

            //            IPAddress localIpAddress = new IPAddress(localAddress);
            //            IPAddress remoteIpAddress = new IPAddress(remoteAddress);

            //            IPEndPoint localEndPoint = new IPEndPoint(localIpAddress, 6882);
            //            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //            clientSocket.Bind(localEndPoint);
            //            clientSocket.ReceiveTimeout = 3000;
            //            clientSocket.ReceiveBufferSize = Setting.BufferSize;
            //            try
            //            {
            //                clientSocket.ConnectAsync(remoteIpAddress, port);
            //                Console.WriteLine("ConnectAsync {0}:{1} successful!", remoteIpAddress, port);

            //                byte[] sndBytes = CreateBuffer();                            
            //                clientSocket.Send(sndBytes);
            //                int bufferSize = Setting.BufferSize;
            //                byte[] rcvBuffer = new byte[bufferSize];
            //                int rcvSize;
            //                int offset = 0;
            //                while (true)//(rcvSize = clientSocket.Receive(rcvBuffer, offset, bufferSize, SocketFlags.None)) > 0)
            //                {
            //                    Console.WriteLine("Cycle start!");
            //                    IList socketList = new List<Socket> { clientSocket };
            //                    Socket.Select(socketList, null, null, 1000 * 1000);
            //                    Console.WriteLine("The number readed sockets is {0}", socketList.Count);
            //                    Console.WriteLine("Press any key to continue...");
            //                    Console.ReadLine();
            //                    if (socketList.Count == 0)
            //                        break;
            //                    rcvSize = clientSocket.Receive(rcvBuffer, offset, bufferSize, SocketFlags.None);
            //                    Console.WriteLine("Offset:{0}, ReadedSize:{1}, RemaingSize:{2}", offset, rcvSize, bufferSize);
            //                    if (rcvSize <= 0)
            //                        break;
            //                    for (int j = offset; j < offset + rcvSize; j++)
            //                    {
            //                        Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", j, (char)rcvBuffer[j], rcvBuffer[j]);
            //                    }
            //                    offset += rcvSize;
            //                    bufferSize -= rcvSize;
            //                }
            //                //Console.WriteLine(offset);
            //                //for (int j = 0; j < offset; j++)
            //                //{
            //                //    Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", j, (char)rcvBuffer[j], rcvBuffer[j]);
            //                //}
            //                clientSocket.Shutdown(SocketShutdown.Both);
            //                Console.WriteLine("Press any key to continue...");
            //                Console.ReadLine();
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("ConnectAsync {0}:{1} fail!", remoteIpAddress, port);
            //                Console.WriteLine(ex);
            //                Console.WriteLine("Press any key to continue...");
            //                Console.ReadLine();
            //            }
            //            finally
            //            {
            //                clientSocket.Close();
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }

        private static void ShowAnnounceResponse(AnnounceResponse response)
        {
            Console.WriteLine("Failure reason:{0}", response.FailureReason);
            Console.WriteLine("Warning message:{0}", response.WarningMessage);
            Console.WriteLine("Interval:{0}", response.Interval);
            Console.WriteLine("Min interval:{0}", response.MinInterval);
            Console.WriteLine("Tracker id:{0}", response.TrackerId);
            Console.WriteLine("Complete:{0}", response.Complete);
            Console.WriteLine("Incomplete:{0}", response.Incomplete);

            Console.WriteLine("Peer list:");
            for (int i = 0; i < response.Peers.Count; i++)
            {
                Peer peer = response.Peers[i];
                Console.WriteLine("{0}th peer ip address: {1}:{2}", i + 1, peer.Host, peer.Port);
                //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            }
        }
    }
}