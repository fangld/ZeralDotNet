using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace TestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            TestMetaInfoParser();
            //TestTracker();
            //TestConnectClient();
        }

        private static void TestMetaInfoParser()
        {
            string torrentFileName = @"D:\Bittorrent\winedt60.exe.torrent";
            MetaInfo result = MetaInfoParser.Parse(torrentFileName);
            Console.WriteLine(result.CreationDate.ToLocalTime());

            torrentFileName = @"D:\Bittorrent\VS11_DP_CTP_ULT_ENU.torrent";
            result = MetaInfoParser.Parse(torrentFileName);
            Console.WriteLine(result.CreationDate.ToLocalTime());
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

        private static void TestConnectClient()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //EndPoint
            socket.Connect("localhost", 60626);

            //socket.Connect("localhost", 2712);

            #region buffer

            byte[] buffer = new byte[49 + 19];
            buffer[0] = 19;
            buffer[1] = (byte)'B';
            buffer[2] = (byte)'i';
            buffer[3] = (byte)'t';
            buffer[4] = (byte)'T';
            buffer[5] = (byte)'o';
            buffer[6] = (byte)'r';
            buffer[7] = (byte)'r';
            buffer[8] = (byte)'e';
            buffer[9] = (byte)'n';
            buffer[10] = (byte)'t';
            buffer[11] = (byte)' ';
            buffer[12] = (byte)'p';
            buffer[13] = (byte)'r';
            buffer[14] = (byte)'o';
            buffer[15] = (byte)'t';
            buffer[16] = (byte)'o';
            buffer[17] = (byte)'c';
            buffer[18] = (byte)'o';
            buffer[19] = (byte)'l';
            buffer[20] = 0;
            buffer[21] = 0;
            buffer[22] = 0;
            buffer[23] = 0;
            buffer[24] = 0;
            buffer[25] = 0;
            buffer[26] = 0;
            buffer[27] = 0;

            #endregion
            //string str = "88AD5F63DFC4E079E532765218EA81B74D837A01";
            //string str = "A40D685CE173EAEBBCB9EF1719A1893191A2DC78";
            string str = "%8C%07%250%D1%19%84%B2%E6%DBFmr%C8%E8%B2%F4%F2%C4F"; //foobar
            StringBuilder sb = new StringBuilder();
            for (int i = 0, j = 28; i < str.Length; i += 2, j++)
            {
                string numberString = str.Substring(i, 2);
                buffer[j] = byte.Parse(numberString, NumberStyles.HexNumber);
            }

            byte[] peerId = Encoding.ASCII.GetBytes("-AZ2060-000000000000");
            Buffer.BlockCopy(peerId, 0, buffer, 48, 20);
            for (int i = 0; i < buffer.Length; i++)
            {
                Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", i, (char)buffer[i], buffer[i]);
            }

            socket.Send(buffer);

            byte[] rcvBuffer = new byte[1024];
            int rcvLen = socket.Receive(rcvBuffer);
            Console.WriteLine(rcvLen);
            for (int i = 0; i < rcvLen; i++)
            {
                Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", i, (char)rcvBuffer[i], rcvBuffer[i]);
            }
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static byte[] CreateBuffer()
        {
            var buffer = new byte[49 + 19];
            buffer[0] = 19;
            buffer[1] = (byte)'B';
            buffer[2] = (byte)'i';
            buffer[3] = (byte)'t';
            buffer[4] = (byte)'T';
            buffer[5] = (byte)'o';
            buffer[6] = (byte)'r';
            buffer[7] = (byte)'r';
            buffer[8] = (byte)'e';
            buffer[9] = (byte)'n';
            buffer[10] = (byte)'t';
            buffer[11] = (byte)' ';
            buffer[12] = (byte)'p';
            buffer[13] = (byte)'r';
            buffer[14] = (byte)'o';
            buffer[15] = (byte)'t';
            buffer[16] = (byte)'o';
            buffer[17] = (byte)'c';
            buffer[18] = (byte)'o';
            buffer[19] = (byte)'l';
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

        private static void TestTracker()
        {
            //GetString();

            string hashInfoUriFormat = GetString("C2331AC95D3273D65F4DA643C98A6CBFD40E8B87");
            string uriString =
                string.Format(
                    "http://127.0.0.1:8080/announce?info_hash={0}&peer_id=-AZ2060-000000000000&port=6881&uploaded=0&downloaded=0&left=10&compact=1&event=started", hashInfoUriFormat);


            ////WebRequest httpWebRequest = WebRequest.Create("http://localhost:6969/announce?info_hash=~%B74%ED%C3%14%DCG%2A%9FR%DB.%DF%5C%B3%D88%F3%87");
            //WebRequest httpWebRequest = WebRequest.Create("http://192.168.1.100:6969/announce?info_hash=%88%AD_c%DF%C4%E0y%E52vR%18%EA%81%B7M%83z%01&peer_id=-AZ2060-000000000000&port=6881&uploaded=0&downloaded=0&left=10&compact=1&event=started");
            WebRequest httpWebRequest = WebRequest.Create(uriString);
            //WebRequest httpWebRequest = WebRequest.Create("http://127.0.0.1:6969/scrape?info_hash=%88%AD_c%DF%C4%E0y%E52vR%18%EA%81%B7M%83z%1");

            httpWebRequest.Method = "GET";

            try
            {
                using (WebResponse httpWebResponse = httpWebRequest.GetResponse())
                {
                    Console.WriteLine("get response");
                    Stream stream = httpWebResponse.GetResponseStream();
                    byte[] buffer = new byte[1024];
                    int count = 0;
                    
                    count = stream.Read(buffer, 0, 1024);
                    FileStream fs = new FileStream("d:\\a.dat", FileMode.OpenOrCreate);
                    fs.Write(buffer, 0, count);
                    fs.Flush();
                    fs.Close();
                    Console.WriteLine("count:{0}", count);
                    Console.WriteLine(Encoding.Default.GetString(buffer, 0, count));

                    byte[] source = new byte[count];
                    Buffer.BlockCopy(buffer, 0, source, 0, count);

                    DictNode node = BEncoder.Decode(source) as DictNode;
                    BytesNode peersNode = node["peers"] as BytesNode;
                    //for (int i = 0; i < 6; i++)//peersNode.ByteArray.Length; i++)
                    //{
                    //    Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", i, (char)buffer[i], buffer[i]);
                    //}
                    byte[] bytes = peersNode.ByteArray;
                    for (int i = 0; i < bytes.Length; i += 6)//peersNode.ByteArray.Length; i++)
                    {
                        Console.WriteLine("{0} {1} {2} {3} {4} {5}", bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5]);

                        byte[] address = new byte[4];
                        Buffer.BlockCopy(peersNode.ByteArray, i, address, 0, 4);
                        long t = (((long)(bytes[i])) << 24);
                        //long address = (((long)(bytes[i])) << 24) + (bytes[i + 1] << 16) + (bytes[i + 2]<< 8) + bytes[i + 3];
                        int port = ((int)bytes[i + 4]) * 256 + bytes[i + 5];
                        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        clientSocket.ReceiveTimeout = 1000;
                        clientSocket.ReceiveBufferSize = 4096;
                        IPAddress ipAddress = new IPAddress(address);
                        IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
                        try
                        {
                            clientSocket.Connect(ipAddress, port);
                            Console.WriteLine("Connect {0}:{1} successful!", ipAddress, port);
                            
                            byte[] sndBytes = CreateBuffer();                            
                            clientSocket.Send(sndBytes);
                            int bufferSize = 4096;
                            byte[] rcvBuffer = new byte[bufferSize];
                            int rcvSize;
                            int offset = 0;
                            while (true)//(rcvSize = clientSocket.Receive(rcvBuffer, offset, bufferSize, SocketFlags.None)) > 0)
                            {
                                Console.WriteLine("Cycle start!");
                                IList socketList = new List<Socket>() { clientSocket };
                                Socket.Select(socketList, null, null, 1000 * 1000);
                                Console.WriteLine("The number readed sockets is {0}", socketList.Count);
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadLine();
                                if (socketList.Count == 0)
                                    break;
                                rcvSize = clientSocket.Receive(rcvBuffer, offset, bufferSize, SocketFlags.None);
                                Console.WriteLine("Offset:{0}, ReadedSize:{1}, RemaingSize:{2}", offset, rcvSize, bufferSize);
                                if (rcvSize <= 0)
                                    break;
                                for (int j = offset; j < offset + rcvSize; j++)
                                {
                                    Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", j, (char)rcvBuffer[j], rcvBuffer[j]);
                                }
                                offset += rcvSize;
                                bufferSize -= rcvSize;
                            }
                            //Console.WriteLine(offset);
                            //for (int j = 0; j < offset; j++)
                            //{
                            //    Console.WriteLine("index:{0}, char:{1}, byte:{2:X2}", j, (char)rcvBuffer[j], rcvBuffer[j]);
                            //}
                            clientSocket.Shutdown(SocketShutdown.Both);
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadLine();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Connect {0}:{1} fail!", ipAddress, port);
                            Console.WriteLine(ex);
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadLine();
                        }
                        finally
                        {
                            clientSocket.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static string GetString(string hashinfo)
        {
            byte[] bytes = new byte[20];
            StringBuilder sb = new StringBuilder();
            for (int i = 0, j = 0; i < hashinfo.Length; i += 2, j++)
            {
                string numberString = hashinfo.Substring(i, 2);
                bytes[j] = byte.Parse(numberString, NumberStyles.HexNumber);
                if ((bytes[j] >= '0' && bytes[j] <= '9') || (bytes[j] >= 'a' && bytes[j] <= 'z') || (bytes[j] >= 'A' && bytes[j] <= 'Z') || bytes[j] == '.' || bytes[j] == '-' || bytes[j] == '_' || bytes[j] == '~')
                {
                    sb.Append((char)bytes[j]);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", bytes[j]);
                }
                Console.WriteLine(bytes[j].ToString("X2"));
            }
            Console.WriteLine(sb.ToString());
            return sb.ToString();
        }
    }
}