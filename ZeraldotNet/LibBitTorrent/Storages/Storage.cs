﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public class Storage
    {
        #region Fields

        private FileStream _fileStream;

        /// <summary>
        /// The synchronized object
        /// </summary>
        private readonly object _synchronizedObject;

        #endregion

        #region Constructors

        public Storage(MetaInfo metaInfo, string saveAsDirectory)
        {
            _synchronizedObject = new object();
            if (metaInfo is SingleFileMetaInfo)
            {
                SingleFileMetaInfo singleFileMetaInfo = metaInfo as SingleFileMetaInfo;

                if (!Directory.Exists(saveAsDirectory))
                {
                    Directory.CreateDirectory(saveAsDirectory);
                }

                string path = string.Format(@"{0}\{1}", saveAsDirectory, singleFileMetaInfo.Name);
                _fileStream = File.Open(path, FileMode.OpenOrCreate);
            }
            else
            {
                
            }
        }

        #endregion

        #region Methods

        public void Write(byte[] buffer, long offset)
        {
            lock (_synchronizedObject)
            {
                _fileStream.Seek(offset, SeekOrigin.Begin);
                _fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        public int Read(byte[] buffer, long offset, int length)
        {
            int result;
            lock (_synchronizedObject)
            {
                _fileStream.Seek(offset, SeekOrigin.Begin);
                result = _fileStream.Read(buffer, 0, length);
            }
            return result;
        }

        public void Close()
        {
            _fileStream.Close();
        }

        #endregion
    }
}
