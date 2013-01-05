using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileInfo = ZeraldotNet.LibBitTorrent.BEncoding.FileInfo;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    internal class MultiFileStorage : Storage
    {
        #region Fields

        private Dictionary<string, FileStream> _fileStreamDict;
        private FileInfo[] _fileInfos;
        //private static byte[] testBytes;

        #endregion

        #region Constructors

        //static MultiFileStorage()
        //{
        //    testBytes = new byte[65536];
        //}

        public MultiFileStorage(MetaInfo metaInfo, string saveAsDirectory)
        {
            MultiFileMetaInfo multiFileMetaInfo = metaInfo as MultiFileMetaInfo;

            string rootDirectory = string.Format(@"{0}\{1}", saveAsDirectory, multiFileMetaInfo.Name);
            
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            _fileInfos = multiFileMetaInfo.GetFileInfoList().ToArray();
            
            _fileStreamDict = new Dictionary<string, FileStream>(_fileInfos.Length);

            for (int i = 0; i < _fileInfos.Length; i++)
            {
                long length = _fileInfos[i].Length;
                string filePath = string.Format(@"{0}\{1}", rootDirectory, _fileInfos[i].Path);

                int lastIndex;
                if ((lastIndex = filePath.LastIndexOf('\\')) != -1)
                {
                    string dirPath = filePath.Substring(0, lastIndex);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                }

                FileStream fs = File.Open(filePath, FileMode.OpenOrCreate);
                fs.SetLength(length);
                _fileStreamDict.Add(_fileInfos[i].Path, fs);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Search the file range that its range contains offset
        /// </summary>
        /// <param name="offset">The offset in all files</param>
        /// <returns>Return the first index of matched file ranges</returns>
        private int Search(long offset)
        {
            int result = 0;
            for (int i = 0; i < _fileInfos.Length; i++)
            {
                if (_fileInfos[i].End > offset)
                {
                    result = i;
                    break;
                }
            }
            return result;

            //int beginIndex = 0;
            //int endIndex = _fileRanges.Count();
            //int minIndex;
            //do
            //{
            //     minIndex = ((beginIndex + endIndex) >> 1);
            //    if (_fileRanges[minIndex].Begin > offset)
            //    {
            //        endIndex = minIndex;
            //        continue;
            //    }

            //    if(_fileRanges[minIndex].End < offset)
            //    {
            //        beginIndex = minIndex;
            //        continue;
            //    }

            //    if (_fileRanges[minIndex].Begin <= offset && _fileRanges[minIndex].End > offset)
            //    {
            //        break;
            //    }
            //} while (true);
            //return minIndex;
        }

        public override void Write(byte[] buffer, long offset)
        {
            try
            {
                //Array.Copy(buffer, 0, Buffer, offset, buffer.Length);
                int firstIndex = Search(offset);
                int i = firstIndex;
                int bufferOffset = 0;
                int bufferRemaining = buffer.Length;
                do
                {
                    FileInfo fileInfo = _fileInfos[i];
                    long fsOffset = i == firstIndex ? offset - fileInfo.Begin : 0;
                    int fsCount = Math.Min((int)(fileInfo.Length - fsOffset), bufferRemaining);
                    string path = fileInfo.Path;
                    FileStream fs = _fileStreamDict[path];
                    lock (fs)
                    {
                        fs.Seek(fsOffset, SeekOrigin.Begin);
                        fs.Write(buffer, bufferOffset, fsCount);
                        fs.Flush();
                        //byte[] validBuffer = new byte[fsCount];
                        //fs.Seek(fsOffset, SeekOrigin.Begin);
                        //fs.Read(validBuffer, 0, fsCount);
                        //for (int j = 0; j < fsCount; j++)
                        //{
                        //    if (offset == 73 * 65536 + 16384 * 3 && j + bufferOffset >= 8269 && j + bufferOffset < 8271)
                        //    {
                        //        Console.WriteLine("Write: {0}: {1}, {2}, {3}", _fileRanges[i].Path, j, buffer[bufferOffset + j], validBuffer[j]);
                        //    }

                        //    int x = 0;


                        //    if (buffer[bufferOffset + j] != validBuffer[j])
                        //    {

                        //        x++;
                        //    }
                        //}
                    }
                    i++;
                    bufferOffset += fsCount;
                    bufferRemaining -= fsCount;

                } while (bufferRemaining != 0);
            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }

            //byte[] readBuffer = new byte[buffer.Length];
            //Read(readBuffer, offset, buffer.Length);
            //for (int i = 0; i < buffer.Length; i++)
            //{
            //    if (readBuffer[i] != buffer[i])
            //    {
            //        Console.WriteLine("{0}: {1}, {2}", i, readBuffer[i], buffer[i]);
            //    }
            //}
        }

        public override int Read(byte[] buffer, long offset, int count)
        {
            int result = 0;
            try
            {
                //if (offset == 5046272)
                //{
                //    Array.Copy(testBytes, buffer, count);
                //}

                int firstIndex = Search(offset);
                int i = firstIndex;
                int bufferOffset = 0;
                int bufferRemaining = buffer.Length;
                do
                {
                    FileInfo fileInfo = _fileInfos[i];
                    long fsOffset = i == firstIndex ? offset - fileInfo.Begin : 0;
                    int fsCount = Math.Min((int)(fileInfo.Length - fsOffset), bufferRemaining);
                    string path = fileInfo.Path;
                    FileStream fs = _fileStreamDict[path];
                    lock (fs)
                    {
                        fs.Seek(fsOffset, SeekOrigin.Begin);
                        int readCount = fs.Read(buffer, bufferOffset, fsCount);
                        Debug.Assert(readCount == fsCount);

                        //for (int j = 0; j < fsCount; j++)
                        //{
                        //    if (offset == 73 * 65536 && j + bufferOffset >= 8269 + 16384 * 3 && j + bufferOffset < 8271+ 16384 * 3)
                        //    {
                        //        Console.WriteLine("{0}: {1}, {2}", _fileRanges[i].Path, j, buffer[bufferOffset + j]);
                        //    }
                        //}

                        //fs.Flush();
                    }
                    i++;
                    bufferOffset += fsCount;
                    bufferRemaining -= fsCount;

                } while (bufferRemaining != 0);

                //lock (_fileStream)
                //{
                //    _fileStream.Seek(offset, SeekOrigin.Begin);
                //    result = _fileStream.Read(buffer, 0, count);
                //}
            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }
            return result;
        }

        public override void Close()
        {
            lock (_fileStreamDict)
            {
                foreach (FileStream fs in _fileStreamDict.Values)
                {
                    fs.Close();
                }
            }
        }

        public override void Dispose()
        {
            lock (_fileStreamDict)
            {
                foreach (FileStream fs in _fileStreamDict.Values)
                {
                    fs.Dispose();
                }
            }
        }

        #endregion
    }
}