using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 封装了对磁盘文件的读写操作
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// 待下载文件的子文件
        /// </summary>
        private List<FileRange> fileRanges;

        /// <summary>
        /// 待下载文件的总长度
        /// </summary>
        private long totalLength;

        /// <summary>
        /// 设置和访问待下载文件的总长度
        /// </summary>
        public long TotalLength
        {
            get { return totalLength; }
            set { this.totalLength = value; }
        }

        /// <summary>
        /// 一个字典,用来保存所有被打开文件(无论是只读还是读写)的文件流
        /// </summary>
        private Dictionary<string, FileStream> handles;

        /// <summary>
        /// 一个字典,用来保存对应文件的当前长度
        /// </summary>
        private Dictionary<string, long> tops;

        /// <summary>
        /// 已经存在该文件标志
        /// </summary>
        private bool isExisted;

        /// <summary>
        /// 设置和访问已经存在该文件标志
        /// </summary>
        public bool IsExisted
        {
            get { return isExisted; }
        }

        ///// <summary>
        ///// 一个字典,用来保存对应文件是否是未分配的磁盘空间
        ///// </summary>
        //private Dictionary<string, long> unallocated;

        ///// <summary>
        ///// 一个字典,用来保存对应文件是否是未下载的磁盘空间
        ///// </summary>
        //private Dictionary<string, long> undownloaded;

        /// <summary>
        /// 初始化一个Storage类,包含文件结构(包含文件名称和长度),停止时间,状态的代表类型
        /// </summary>
        /// <param name="bitFiles">所读写的文件</param>
        /// <param name="allocPause">停止时间</param>
        /// <param name="statusFunc">状态的代表函数</param>
        public Storage(List<BitFile> bitFiles, double allocPause, StatusDelegate statusFunc)
        {
            fileRanges = new List<FileRange>();
            //unallocated = new Dictionary<string, long>();
            //undownloaded = new Dictionary<string, long>();
            
            long total = 0L;

            //soFar是实际存在的文件总长度
            long soFar = 0L;

            //将BitFile转换为FileRange
            BitFileToFileRange(bitFiles, ref total, ref soFar);

            totalLength = total;
            handles = new Dictionary<string, FileStream>();
            tops = new Dictionary<string, long>();
            isExisted = false;

            //设置文件的读写方式
            SetupFileStream(bitFiles);

            //如果需要下载的文件长度比实际的文件长度长，则分配磁盘空间
            if (total > soFar)
            {
                AllocationDiskSpace(bitFiles, allocPause, statusFunc, ref soFar);
            }
        }


        /// <summary>
        /// 分配磁盘空间
        /// </summary>
        /// <param name="bitFiles">所读写的文件</param>
        /// <param name="allocPause">停止时间</param>
        /// <param name="statusFunc">状态的代表函数</param>
        /// <param name="total">所有子文件的总长度</param>
        /// <param name="soFar">实际存在的子文件总长度</param>
        private void AllocationDiskSpace(List<BitFile> bitFiles, double allocPause, StatusDelegate statusFunc, ref long soFar)
        {
            long fileLength, length, offset;

            //1048576为2的20次方
            long interval = (long)Math.Max(1048576L, totalLength / 100);
            DateTime timeStart = DateTime.Now;
            bool hit = false;
            string fileName;

            foreach (BitFile singleBitFile in bitFiles)
            {
                fileName = singleBitFile.FileName;
                //如果磁盘上已经存在了文件，则获取文件的长度，否则长度为0
                length = File.Exists(fileName) ? new FileInfo(singleBitFile.FileName).Length : 0L;

                //如果磁盘上已经分配了一些空间
                if (isExisted)
                {
                    if (!handles[fileName].CanWrite)
                    {
                        handles[fileName].Close();
                        handles[fileName] = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
                    }
                }

                FileStream hfStream = handles[fileName];
                fileLength = singleBitFile.Length;

                if (length < fileLength)
                {
                    //为子文件分配磁盘空间
                    for (offset = length; offset < fileLength; offset += interval)
                    {
                        hfStream.Seek(offset, SeekOrigin.Begin);
                        hfStream.WriteByte(1);
                        TimeSpan timeNow = DateTime.Now.Subtract(timeStart);

                        //显示分配空间信息
                        AllocateInformation(allocPause, statusFunc, length, offset, soFar, ref hit, timeNow);
                    }

                    //为子文件分配最后的磁盘空间
                    if (fileLength > 0)
                    {
                        hfStream.Seek(fileLength - 1, SeekOrigin.Begin);
                        hfStream.WriteByte(1);
                        hfStream.Flush();
                        soFar += fileLength - length;
                    }
                }
            }

            if (statusFunc != null)
            {
                statusFunc(string.Empty, -1, -1, 1.0, -1);
            }
        }

        /// <summary>
        /// 显示分配空间信息
        /// </summary>
        /// <param name="allocPause">停止时间</param>
        /// <param name="statusFunc">状态的代表函数</param>
        /// <param name="length">已分配文件的初始长度</param>
        /// <param name="offset">已分配文件的偏移位置</param>
        /// <param name="soFar">实际存在的子文件总长度</param>
        /// <param name="hit">是否被第一次显示</param>
        /// <param name="timeNow">分配磁盘的时间间隔</param>
        private void AllocateInformation(double allocPause, StatusDelegate statusFunc, long length, long offset, long soFar, ref bool hit, TimeSpan timeNow)
        {
            //如果时间过长，显示分配空间信息
            if (timeNow.TotalSeconds > allocPause)
            {
                //如果没有显示“分配中”信息，则显示“分配中”信息
                if (!hit)
                {
                    if (statusFunc != null)
                    {
                        statusFunc("分配中", -1, -1, -1, -1);
                    }
                    hit = true;
                }

                //否则显示分配进度信息
                if (statusFunc != null)
                {
                    statusFunc(string.Empty, -1, -1, (double)(soFar + offset - length) / (double)totalLength, -1);
                }
            }
        }

        /// <summary>
        /// 设置文件的读写方式
        /// </summary>
        /// <param name="bitFiles">所读写的文件</param>
        private void SetupFileStream(List<BitFile> bitFiles)
        {
            foreach (BitFile singleBitFile in bitFiles)
            {
                //如果文件已经存在，将其打开并且设置为只读方式
                if (File.Exists(singleBitFile.FileName))
                {
                    handles[singleBitFile.FileName] = File.OpenRead(singleBitFile.FileName);
                    isExisted = true;
                }

                //如果文件已经存在，将其打开并且设置为读写方式
                else
                {
                    handles[singleBitFile.FileName] = File.Open(singleBitFile.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                }
            }
        }

        /// <summary>
        /// 将BitFile转换为FileRange
        /// </summary>
        /// <param name="bitFiles">代转换的BitFile</param>
        /// <param name="total">要下载文件的长度</param>
        /// <param name="soFar">实际文件的长度</param>
        private void BitFileToFileRange(List<BitFile> bitFiles, ref long total, ref long soFar)
        {
            long length;
            long fileLength;

            foreach (BitFile singleBitFile in bitFiles)
            {
                fileLength = singleBitFile.Length;
                //this.unallocated.Add(singleBitFile.FileName, fileLength);
                //this.undownloaded.Add(singleBitFile.FileName, fileLength);

                //如果所添加的文件的长度大于0
                if (singleBitFile.Length > 0)
                {
                    //根据bitFiles添加待下载文件的子文件
                    FileRange singleFileRange = new FileRange(singleBitFile.FileName, total, total += fileLength);
                    fileRanges.Add(singleFileRange);

                    if (File.Exists(singleBitFile.FileName))
                    {
                        FileInfo fileInfo = new FileInfo(singleBitFile.FileName);
                        length = fileInfo.Length;

                        //如果存在的文件比该文件大
                        if (length > fileLength)
                        {
                            throw new BitTorrentException(string.Format("存在的文件{0}的长度比待下载的文件大", singleBitFile.FileName));
                        }
                        soFar += length;
                    }
                }

                //如果所添加的文件的长度等于0
                else if (singleBitFile.Length == 0)
                {
                    if (File.Exists(singleBitFile.FileName))
                    {
                        FileInfo fileInfo = new FileInfo(singleBitFile.FileName);
                        if (fileInfo.Length > 0)
                        {
                            throw new BitTorrentException(string.Format("存在的文件{0}的长度比待下载的文件大", singleBitFile.FileName));
                        }
                    }

                    //如果文件长度为0,则创建一个空文件
                    else
                    {
                        File.OpenWrite(singleBitFile.FileName).Close();
                    }
                }

                //如果所添加的文件的长度小于0
                else
                {
                    throw new BitTorrentException(string.Format("文件{0}的长度小于0", singleBitFile.FileName));
                }
            }
        }

        private void FileRange(string p, long total, long p_3)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 判断起始位置为position，长度为length的文件片断是否已经分配了磁盘空间
        /// </summary>       
        /// <remarks>        
        /// 判断，在Storage初始化之前，是否已经在磁盘上分配了空间。
        /// 例如，大小为 1024k的文件，如果获得了 第1个片断（从256k到512k），
        /// 那么这时候，磁盘上文件的大小是 512k（也就是分配了512k），
        /// 尽管第0个片断（从0到256k）还没有获得，但磁盘上会保留这个“空洞”。
        /// </remarks>
        /// <param name="position">文件的起始位置</param>
        /// <param name="length">文件长度</param>
        /// <returns>已经分配了磁盘空间返回true，否则返回false</returns>
        public bool IsAllocated(long position, long length)
        {
            foreach (FileRange fr in Intervals(position, length))
            {
                if (tops[fr.FileName] < fr.End)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 将所有文件的读写方式改为只读方式
        /// </summary>
        public void SetReadonly()
        {
            FileStream old;

            foreach (string fileName in handles.Keys)
            {
                old = handles[fileName];
                if (!old.CanWrite && old.CanRead)
                {
                    old.Flush();
                    old.Close();
                    handles[fileName] = File.OpenRead(fileName);
                }
            }
        }

        /// <summary>
        /// 返回起始位置为position，长度为count的所有子文件
        /// </summary>
        /// <remarks>
        /// 这个函数意思是检查起始位置为position，长度为count的片断实际位置在哪里？
        /// 例如，假设有两个文件，aaa和bbb，大小分别是400和1000，那么position为300，amount为200的文件片断属于哪个文件了？
        /// 它分别属于两个文件，所以返回的是[("aaa", 300, 400), ("bbb", 0, 100)]，
        /// 也就是它既包含了aaa 文件中从300到400这段数据，也包含了bbb文件从0到100这段数据。
        /// </remarks>
        /// <param name="position">开始位置</param>
        /// <param name="count">文件长度</param>
        /// <returns>返回起始位置为position，长度为count的所有子文件</returns>
        private List<FileRange> Intervals(long position, long count)
        {
            long begin, end;
            string fileName;
            List<FileRange> result = new List<FileRange>();

            //结束的位置
            long stop = position + count;

            foreach (FileRange singleFileRange in fileRanges)
            {
                //当搜索的子文件还不是所需要的子文件的范围时，继续搜索
                if (singleFileRange.End <= position)
                    continue;

                //当搜索的子文件已经超出所需要的子文件范围时，停止搜索
                if (singleFileRange.Begin >= stop)
                    break;

                //fileRange 是一个三元组的列表，三元组格式是（文件名，在该文件的起始位置，在该文件的结束位置）。
                fileName = singleFileRange.FileName;

                //计算子文件的起始位置和结束位置
                begin = Math.Max(position, singleFileRange.Begin) - singleFileRange.Begin;
                end = Math.Min(singleFileRange.End, stop) - singleFileRange.Begin;

                //添加在position位置开始，长度count的子文件
                FileRange fileRange = new FileRange(fileName, begin, end);
                result.Add(fileRange);
            }

            return result;
        }

        /// <summary>
        /// 将起始位置为position，长度为count的数据从文件中读出来
        /// </summary>
        /// <param name="position">要读出的字节数组的起始位置</param>
        /// <param name="count">要读出的字节数组长度</param>
        /// <returns>所读出来的字节数值</returns>
        public byte[] Read(long position, long count)
        {
            byte[] result = new byte[count];
            //访问时候的偏移量
            int offset = 0;
            FileStream hfStream;

            foreach (FileRange singleFileRange in Intervals(position, count))
            {
                hfStream = handles[singleFileRange.FileName];
                hfStream.Seek(singleFileRange.Begin, SeekOrigin.Begin);
                offset += hfStream.Read(result, offset, (int)(singleFileRange.End - singleFileRange.Begin));
            }

            return result;
        }

        /// <summary>
        /// 将起始位置为position的字节数组bytes写到相应的磁盘文件中。
        /// </summary>
        /// <param name="position">要写入磁盘文件的字节数组的开始位置</param>
        /// <param name="bytes">要写入磁盘文件的字节数组</param>
        public void Write(long positon, byte[] bytes)
        {
            //要写入磁盘文件的字节数组的总长度
            int offset = 0;
            FileStream hfStream;

            foreach (FileRange singleFileRange in Intervals(positon, bytes.LongLength))
            {
                //如果该文件并不是以写的方式打开的，那么改成读写方式打开
                if (!handles[singleFileRange.FileName].CanWrite)
                {
                    hfStream = handles[singleFileRange.FileName];
                    hfStream.Close();
                    handles[singleFileRange.FileName] = File.Open(singleFileRange.FileName, FileMode.Open, FileAccess.ReadWrite);
                }
                hfStream = handles[singleFileRange.FileName];

                //通过 seek 函数移动文件指针，可以看出来，文件不是按照顺序来写的，因为所获取的文件片断是随机的，所以写也是随机的。
                //这里有一个疑问，假设获得了第二个文件片断，起始是 1000，大小是500，而第一个片断还没有获得，那么文件指针要移动到
                //1000 处，并写500个字节。这时候，文件的大小应该是 1500，尽管前面 1000 个字节是“空洞”。那么如果，直到结束，都没
                //有获得第一个片断，又如何检测出来了？（通过检查 total？）
                hfStream.Seek(singleFileRange.Begin, SeekOrigin.Begin);
                hfStream.Write(bytes, offset, (int)(singleFileRange.End - singleFileRange.Begin));
                offset += (int)(singleFileRange.End - singleFileRange.Begin);
            }
        }

        ///// <summary>
        ///// 分配磁盘空间
        ///// </summary>
        ///// <param name="position">要分配的字节数组的开始位置</param>
        ///// <param name="count">要分配的字节数组的大小</param>
        //private void Allocated(long offset, long count)
        //{
        //    foreach (FileRange singleFileRange in Intervals(offset, count))
        //    {
        //        this.unallocated[singleFileRange.FileName] -= singleFileRange.End - singleFileRange.Begin;
        //    }
        //}

        ///// <summary>
        ///// 下载文件到磁盘空间
        ///// </summary>
        ///// <param name="position">要下载的字节数组的开始位置</param>
        ///// <param name="count">要下载的字节数组的大小</param>
        //private void Downloaded(long offset, long count)
        //{
        //    foreach (FileRange singleFileRange in Intervals(offset, count))
        //    {
        //        this.undownloaded[singleFileRange.FileName] -= singleFileRange.End - singleFileRange.Begin;
        //    }
        //}

        /// <summary>
        /// 关闭所有打开文件
        /// </summary>
        public void Close()
        {
            foreach (FileStream fileStream in handles.Values)
            {
                fileStream.Close();
            }
        }
    }
}