using System;
using System.Collections.Generic;
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
        private FileRange[] _fileRanges;
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


            IList<FileInfo> fileInfos = multiFileMetaInfo.GetFileInfoList();
            
            _fileStreamDict = new Dictionary<string, FileStream>(fileInfos.Count);
            _fileRanges = new FileRange[fileInfos.Count];

            long begin = 0;
            for (int i = 0; i < fileInfos.Count; i++)
            {
                long length = fileInfos[i].Length;
                string filePath = string.Format(@"{0}\{1}", rootDirectory, fileInfos[i].Path);
                //Directory.CreateDirectory()
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
                _fileStreamDict.Add(fileInfos[i].Path, fs);

                _fileRanges[i] = new FileRange
                    {
                        Begin = begin,
                        End = begin + length,
                        Path = fileInfos[i].Path
                    };
                begin += length;
            }
        }

        #endregion

        #region Methods

        private int Search(long offset)
        {
            int result = 0;
            for (int i = 0; i < _fileRanges.Length; i++)
            {
                if (_fileRanges[i].End > offset)
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
                //if (offset == 5046272 || offset == 5062656 || offset == 5079040 || offset == 5095424)
                //{
                //    Array.Copy(buffer, 0, testBytes, offset - 5046272, buffer.Length);
                //}

                int firstIndex = Search(offset);
                int i = firstIndex;
                int bufferOffset = 0;
                int bufferRemaining = buffer.Length;
                do
                {
                    FileRange fileRange = _fileRanges[i];
                    long fsOffset = i == firstIndex ? offset - fileRange.Begin : 0;
                    int fsCount = Math.Min((int)(fileRange.End - fsOffset), bufferRemaining);
                    string path = fileRange.Path;
                    FileStream fs = _fileStreamDict[path];
                    lock (fs)
                    {
                        fs.Seek(fsOffset, SeekOrigin.Begin);
                        fs.Write(buffer, bufferOffset, fsCount);
                        fs.Flush();
                    }
                    i++;
                    bufferOffset += fsCount;
                    bufferRemaining -= fsCount;

                    //if (bufferOffset == buffer.Length)
                    //{
                    //    break;
                    //}
                } while (bufferRemaining != 0);
            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }
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
                    FileRange fileRange = _fileRanges[i];
                    long fsOffset = i == firstIndex ? offset - fileRange.Begin : 0;
                    int fsCount = Math.Min((int)(fileRange.End - fsOffset), bufferRemaining);
                    string path = fileRange.Path;
                    FileStream fs = _fileStreamDict[path];
                    lock (fs)
                    {
                        fs.Seek(fsOffset, SeekOrigin.Begin);
                        fs.Read(buffer, bufferOffset, fsCount);
                        //fs.Flush();
                    }
                    firstIndex++;
                    bufferOffset += fsCount;
                    bufferRemaining -= fsCount;
                    //if (bufferOffset == buffer.Length)
                    //{
                    //    break;
                    //}
                } while (bufferRemaining != 0);

                //for(int i = Search(offset), bufferOffset = 0, bufferCount = count; bufferOffset != count; i++)
                //{
                //    FileRange fileRange = _fileRanges[i];
                //    long fsOffset = Math.Max(offset - fileRange.Begin, 0);
                //    int fsCount = Math.Min((int)(fileRange.End - fsOffset), bufferCount);
                //    string path = fileRange.Path;
                //    FileStream fs = _fileStreamDict[path];
                //    lock (fs)
                //    {
                //        fs.Seek(fsOffset, SeekOrigin.Begin);
                //        fs.Read(buffer, bufferOffset, fsCount);
                //    }
                //    bufferOffset += fsCount;
                //    bufferCount -= fsCount;
                //}

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