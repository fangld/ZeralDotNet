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
using System.Timers;
using FileInfo = System.IO.FileInfo;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Trackers;

namespace TestProgram
{
    internal class Program
    {
        private const string winedtTorrentFile = @"E:\Bittorrent\Torrents\winedt70.exe.torrent";
        private const string winedtSaveAsDirectory = @"E:\Winedt70";
        private const string sumatraPDFTorrentFile = @"E:\Bittorrent\Torrents\SumatraPDF-2.1.1-install.exe.torrent";
        private const string sumatraPDFSaveAsDirectory = @"E:\SumatraPDF";
        private const string greenThemepackTorrentFile = @"E:\Bittorrent\Torrents\Green.themepack.torrent";
        private const string greenThemepackSaveAsDirectory = @"E:\GreenThemepack";
        private const string potPlayerTorrentFile = @"E:\Bittorrent\Torrents\PotPlayer.torrent";
        private const string potPlayerSaveAsDirectory = @"E:\PotPlayer";
        private const string driverTorrentFile = @"E:\Bittorrent\Torrents\Install_Win7_6111_10202010.torrent";
        private const string driverSaveAsDirectory = @"E:\Driver";

        private const string qqTorrentFile = @"E:\Bittorrent\Torrents\QQ2013Beta1.exe.torrent";
        private const string qqSaveAsDirectory = @"E:\QQ";

        

        
        private static void Main(string[] args)
        {
            //TestMetaInfoParser();
            //TestTracker();
            //DetermineRcvFileCorrent();
            TestConnectClient();
            Console.ReadLine();
        }

        private static void TestMetaInfoParser()
        {
            MetaInfo result = MetaInfo.Parse(qqTorrentFile);
            //Console.WriteLine(result.CreationDate.ToLocalTime());
            ShowMetaInfo(result);

            //torrentFileName = @"D:\Bittorrent\VS11_DP_CTP_ULT_ENU.torrent";
            //result = MetaInfo.Parse(torrentFileName);
            ////Console.WriteLine(result.CreationDate.ToLocalTime());
            //ShowMetaInfo(result);
        }

        private static void DetermineRcvFileCorrent()
        {
            string[] rcvStr = Directory.GetFiles(@"E:\PotPlayer\PotPlayer");

            for (int i = 0; i < rcvStr.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(rcvStr[i]);
                Console.WriteLine(fileInfo.Name);
                //string orgPath = string.Format(@"{0}\{1}", @"D:\Software\PotPlayer", fileInfo.Name);
                string orgPath = string.Format(@"{0}\{1}", @"C:\Users\Razor\Downloads\PotPlayer", fileInfo.Name);


                string rcvPath = string.Format(@"{0}\{1}", @"E:\PotPlayer\PotPlayer", fileInfo.Name);
                //string rcvPath = string.Format(@"{0}\{1}", @"D:\Software\PotPlayer", fileInfo.Name);


                FileStream orgFs = new FileStream(orgPath, FileMode.Open);
                FileStream rcvFs = new FileStream(rcvPath, FileMode.Open);

                byte[] orgBuffer = new byte[fileInfo.Length];
                byte[] rcvBuffer = new byte[fileInfo.Length];
                orgFs.Read(orgBuffer, 0, (int)fileInfo.Length);
                rcvFs.Read(rcvBuffer, 0, (int)fileInfo.Length);

                for (int j = 0; j < fileInfo.Length; j++)
                {
                    if (orgBuffer[j] != rcvBuffer[j])
                    {
                        Console.WriteLine("{0}: {1}, {2}", j, orgBuffer[j], rcvBuffer[j]);
                    }
                }
                orgFs.Close();
                rcvFs.Close();
            }

            //int bufferSize = Setting.BlockLength*571;

            //FileStream orgFs = new FileStream(@"D:\Latex\winedt70.exe", FileMode.OpenOrCreate);
            //FileStream rcvFs = new FileStream(@"E:\Winedt70\winedt70.exe", FileMode.OpenOrCreate);

            //byte[] orgBuffer = new byte[bufferSize];
            //byte[] rcvBuffer = new byte[bufferSize];

            //orgFs.Read(orgBuffer, 0, bufferSize);
            //rcvFs.Read(rcvBuffer, 0, bufferSize);

            //for (int i = 0; i < bufferSize; i++)
            //{
            //    if (orgBuffer[i] != rcvBuffer[i])
            //    {
            //        Console.WriteLine("index:{0}, org:{1}, rcv:{2}", i, orgBuffer[i], rcvBuffer[i]);
            //    }
            //}
            //Console.WriteLine("End");
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

            Console.WriteLine();
        }

        private static async void TestConnectClient()
        {
            Task task1 = new Task();
            task1.OnMessage += (sender, message) => Console.WriteLine(message);
            task1.OnFinished += (sender1, args1) =>
                                  {
                                      Console.WriteLine("Task1 is finished");
                                      //Task task2 = new Task();
                                      //task2.TorrentFileName = sumatraPDFTorrentFile;
                                      //task2.SaveAsDirectory = sumatraPDFSaveAsDirectory;
                                      //task2.OnFinished += (sender2, args2) => Console.WriteLine("Task2 is finished");
                                      //task2.Start();
                                  };
            //task1.Start(qqTorrentFile, qqSaveAsDirectory);
            task1.Start(potPlayerTorrentFile, potPlayerSaveAsDirectory);
            //task1.Stop();
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
            }
        }
    }
}