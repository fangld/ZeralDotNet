using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public class FilePool
    {
        /// <summary>
        /// 一个字典,用来保存所有被打开文件(无论是只读还是读写)的文件流
        /// </summary>
        private Dictionary<string, FileStream> handles;

        /// <summary>
        /// 一个列表，用来保存所以被打开的文件名称
        /// </summary>
        private ICollection<string> allFiles;

        /// <summary>
        /// 一个集合，用来保存所以被打开的文件名称
        /// </summary>
        private ICollection<string> handleBuffer;

        /// <summary>
        /// 一个字典,用来保存对应文件是否是以写方式打开(读写也是一种写方式)
        /// </summary>
        private Dictionary<string, bool> writeHandles;

        private int maxFilesCount;

        public int MaxFilesCount
        {
            get { return this.maxFilesCount; }
            set { this.maxFilesCount = value; }
        }

        public FilePool(int maxFilesCount)
        {
            this.allFiles = new List<string>();
            this.handleBuffer = null;
            this.handles = new Dictionary<string, FileStream>();
            this.writeHandles = new Dictionary<string, bool>();
            this.SetMaxFilesOpen(maxFilesCount);
        }

        public void CloseAll()
        {
            foreach (FileStream hfStream in handles.Values)
            {
                hfStream.Close();
            }

            this.handles.Clear();
            this.writeHandles.Clear();
        }

        public void SetMaxFilesOpen(int maxFilesCount)
        {
            if (maxFilesCount <= 0)
                maxFilesCount = 100;
            MaxFilesCount = maxFilesCount;
            this.CloseAll();
            if (this.allFiles.Count > MaxFilesCount)
            {
                this.handleBuffer.Clear();
            }
            else
            {
                this.handleBuffer = null;
            }
        }

        public void AddFiles(List<string> files)
        {
            foreach (string fileName in files)
            {
                if (files.Contains(fileName))
                {
                    throw new BitTorrentException(string.Format("文件{0}已经属于另一个运行中的torrent", fileName));
                }
            }

            foreach (string fileName in files)
            {
            }

            if (this.handleBuffer.Count == 0 && this.allFiles.Count > this.maxFilesCount)
            {
                this.handleBuffer = this.handles.Keys;
            }
        }

        public void RemoveFiles(List<string> files)
        {
            foreach (string fileName in files)
            {
                allFiles.Remove(fileName);
            }
            if (this.handleBuffer.Count != 0 && this.allFiles.Count <= this.maxFilesCount)
            {
                this.handleBuffer.Clear();
            }
        }
    }
}
