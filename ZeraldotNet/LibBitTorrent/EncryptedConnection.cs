using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZeraldotNet.LibBitTorrent.ReadFunctions;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 封装连接类
    /// </summary>
    public class EncryptedConnection
    {
        #region Private Field

        /// <summary>
        /// 封装连接管理类
        /// </summary>
        private Encrypter encrypter;

        /// <summary>
        /// 本地ID号
        /// </summary>
        private byte[] id;

        /// <summary>
        /// 是否已经本地初始化
        /// </summary>
        private bool isLocallyInitiated;

        /// <summary>
        /// 是否已经完成下载
        /// </summary>
        private bool complete;

        /// <summary>
        /// 单套接字类
        /// </summary>
        private SingleSocket connection;

        /// <summary>
        /// 是否关闭
        /// </summary>
        private bool closed;

        /// <summary>
        /// 网络字节流缓冲
        /// </summary>
        private MemoryStream buffer;

        /// <summary>
        /// 当前的分析类
        /// </summary>
        private ReadFunction currentFunction;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置本地ID号
        /// </summary>
        public byte[] ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// 访问是否已经本地初始化
        /// </summary>
        public bool IsLocallyInitiated
        {
            get { return this.isLocallyInitiated; }
        }

        /// <summary>
        /// 访问和设置是否已经完成下载
        /// </summary>
        public bool Complete
        {
            get { return this.complete; }
            set { this.complete = value; }
        }

        /// <summary>
        /// 访问和设置是否关闭
        /// </summary>
        public bool Closed
        {
            get { return this.closed; }
            set { this.closed = value; }
        }

        /// <summary>
        /// 访问是否缓冲区为0
        /// </summary>
        public bool IsFlushed
        {
            get { return this.connection.IsFlushed; }
        }

        /// <summary>
        /// 访问对方IP地址
        /// </summary>
        public string IP
        {
            get { return this.connection.IP; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encrypter">封装连接类</param>
        /// <param name="connection"></param>
        /// <param name="id">对方下载工具的ID号</param>
        public EncryptedConnection(Encrypter encrypter, SingleSocket connection, byte[] id)
        {
            this.encrypter = encrypter;
            this.connection = connection;
            this.ID = id;
            this.isLocallyInitiated = (id != null);
            this.Complete = false;
            this.Closed = false;
            this.buffer = new MemoryStream();
            BuildReadFunctionChain();
            SendHandshakeMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 发送Handshake网络信息
        /// </summary>
        private void SendHandshakeMessage()
        {
            //发送协议长度
            connection.Write(new byte[] { Globals.protocolNameLength });

            //发送协议名
            connection.Write(Globals.protocolName);

            //发送保留的8个字节
            connection.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            //发送待下载文件的SHA1码
            connection.Write(encrypter.DownloadID);

            //发送下载工具的SHA1码
            connection.Write(encrypter.MyID);
        }

        /// <summary>
        /// 构造字节流的分析链
        /// </summary>
        private void BuildReadFunctionChain()
        {
            ReadFunction readMessage = new ReadMessage(0, this.encrypter, this);
            ReadFunction readLength = new ReadLength(readMessage, this.encrypter);
            readMessage.Next = readLength;
            ReadFunction readPeerID = new ReadPeerID(readLength, this.encrypter, this);
            ReadFunction readDownloadID = new ReadDownloadID(readPeerID, this.encrypter);
            ReadFunction readReserved = new ReadReserved(readDownloadID);
            ReadFunction readHeader = new ReadHeader(readReserved);
            ReadFunction readHeaderLength = new ReadHeaderLength(readHeader);
            this.currentFunction = readHeaderLength;
        }

        /// <summary>
        /// 读入字节流函数
        /// </summary>
        /// <param name="bytes">读入的字节流</param>
        public void DataCameIn(byte[] bytes)
        {
            int startIndex, bytesLength;
            byte[] memoryBytes;
            do
            {
                //如果关闭，则退出
                if (this.closed)
                {
                    return;
                }

                //计算开始读取位置
                startIndex = currentFunction.Length - (int)this.buffer.Position;

                //如果开始读取位置比数据流的长度要大，则将数据流写入缓冲区，等待下一个数据流
                bytesLength = bytes.Length;
                if (startIndex > bytesLength)
                {
                    this.buffer.Write(bytes, 0, bytesLength);
                    return;
                }

                //否则，将数据流写入缓冲区，进入分析数据流
                this.buffer.Write(bytes, 0, startIndex);

                //删除数据流前startIndex个字节
                Globals.DeleteBytes(bytes, startIndex);

                //清除缓冲区
                memoryBytes = this.buffer.ToArray();
                this.buffer.Close();
                this.buffer = new MemoryStream();

                //如果读取出现错误，则退出
                if (!currentFunction.ReadBytes(memoryBytes))
                {
                    this.Close();
                    return;
                }

                //转到下一个分析协议类
                this.currentFunction = this.currentFunction.Next;
            } while (true);
        }

        /// <summary>
        /// 服务器函数
        /// </summary>
        public void Server()
        {
            this.closed = true;
            encrypter.Remove(connection);
            if (this.complete)
            {
                encrypter.Connecter.LoseConnection(this);
            }
        }

        /// <summary>
        /// 发送网络信息
        /// </summary>
        /// <param name="message">待发送的单个字节</param>
        public void SendMessage(byte message)
        {
            SendMessage(new byte[] { message });
        }

        /// <summary>
        /// 发送网络信息
        /// </summary>
        /// <param name="message">待发送的字节流</param>
        public void SendMessage(byte[] message)
        {
            byte[] lengthBytes = new byte[4];
            Globals.Int32ToBytes(message.Length, lengthBytes, 0);
            connection.Write(lengthBytes);
            connection.Write(message);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (!closed)
            {
                connection.Close();
                this.Server();
            }
        }

        #endregion
    }
}
