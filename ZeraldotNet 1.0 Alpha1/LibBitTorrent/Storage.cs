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
            get
            {
                return totalLength;
            }
        }

        /// <summary>
        /// 一个字典,用来保存所有被打开文件(无论是只读还是读写)的文件流
        /// </summary>
        private Dictionary<string, FileStream> handles;

        /// <summary>
        /// 一个字典,用来保存对应文件是否是以写方式打开(读写也是一种写方式)
        /// </summary>
        private Dictionary<string, bool> writeHandles;

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
            get
            {
                return isExisted;
            }
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
        /// <param name="bitFiles"></param>
        /// <param name="allocPause"></param>
        /// <param name="statusFunc"></param>
        public Storage(List<BitFile> bitFiles, double allocPause, StatusDelegate statusFunc)
        {
            fileRanges = new List<FileRange>();
            //unallocated = new Dictionary<string, long>();
            //undownloaded = new Dictionary<string, long>();
            long total = 0L;
            long length;
            long fileLength;

            //soFar是实际存在的文件总长度
            long soFar = 0L;

            foreach (BitFile singleBitFile in bitFiles)
            {
                fileLength = singleBitFile.Length;
                //this.unallocated.Add(singleBitFile.FileName, fileLength);
                //this.undownloaded.Add(singleBitFile.FileName, fileLength);

                if (singleBitFile.Length != 0)
                {
                    FileRange singleFileRange = new FileRange(singleBitFile.FileName,total, total + fileLength);

                    fileRanges.Add(singleFileRange);
                    total += fileLength;
                    if (File.Exists(singleBitFile.FileName))
                    {
                        FileInfo fileInfo = new FileInfo(singleBitFile.FileName);
                        length = fileInfo.Length;
                        if (length > fileLength)
                        {
                            throw new BitTorrentException(string.Format("存在的文件{0}的大小比待下载的文件大", singleBitFile.FileName));
                        }
                        soFar += length;
                    }
                }

                else
                {
                    if (File.Exists(singleBitFile.FileName))
                    {
                        FileInfo fileInfo = new FileInfo(singleBitFile.FileName);
                        if (fileInfo.Length > 0)
                            throw new BitTorrentException(string.Format("存在的文件{0}的大小比待下载的文件大", singleBitFile.FileName));
                    }

                    //如果文件长度为0,则创建一个空文件
                    else
                    {
                        File.OpenWrite(singleBitFile.FileName).Close();
                    }
                }
            }

            totalLength = total;
            handles = new Dictionary<string, FileStream>();
            writeHandles = new Dictionary<string, bool>();
            tops = new Dictionary<string, long>();
            isExisted = false;

            //对于每一个文件...
            foreach (BitFile singleBitFile in bitFiles)
            {
                //如果文件已经存在
                if (File.Exists(singleBitFile.FileName))
                {
                    handles[singleBitFile.FileName] = File.OpenRead(singleBitFile.FileName);
                    isExisted = true;
                }

                else
                {
                    handles[singleBitFile.FileName] = File.Open(singleBitFile.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    writeHandles[singleBitFile.FileName] = true;
                }
            }

            if (total > soFar)
            {
                //1048576为2的20次方
                long interval = (long)Math.Max(1048576L, total / 100);
                DateTime timeStart = DateTime.Now;
                bool hit = false;
                foreach (BitFile singleBitFile in bitFiles)
                {
                    length = 0L;

                    if (File.Exists(singleBitFile.FileName))
                    {
                        FileInfo fileInfo = new FileInfo(singleBitFile.FileName);
                        length = fileInfo.Length;
                    }

                    if (isExisted)
                    {
                        handles[singleBitFile.FileName].Close();
                        handles[singleBitFile.FileName] = File.Open(singleBitFile.FileName, FileMode.Open, FileAccess.ReadWrite);
                        writeHandles[singleBitFile.FileName] = true;
                    }

                    FileStream hfStream = handles[singleBitFile.FileName];
                    fileLength = singleBitFile.Length;
                    for (long i = interval; i < fileLength; i += interval)
                    {
                        hfStream.Seek(i, SeekOrigin.Begin);
                        hfStream.WriteByte(1);
                        TimeSpan timeNow = DateTime.Now.Subtract(timeStart);

                        if (timeNow.TotalSeconds > allocPause)
                        {
                            if (!hit)
                            {
                                if (statusFunc != null)
                                    statusFunc("分配中", -1, -1, -1, -1);
                                hit = true;
                            }

                            if (statusFunc != null)
                                statusFunc(string.Empty, -1, -1, (double)(soFar + i - length) / (double)total, -1);
                        }
                    }

                    if (fileLength > 0)
                    {
                        hfStream.Seek(fileLength - 1, SeekOrigin.Begin);
                        hfStream.WriteByte(1);
                        hfStream.Flush();
                        soFar += fileLength - length;
                    }
                }

                if (statusFunc != null)
                    statusFunc(string.Empty, -1, -1, 1.0, -1);
            }
        }

        private void FileRange(string p, long total, long p_3)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 判断文件是否已经分配了磁盘空间
        /// </summary>       
        /// <remarks>        
        /// 判断起始位置为 position，长度为 length的文件片断，
        /// 在 Storage 初始化之前，是否已经在磁盘上分配了空间。
        /// 例如，大小为 1024k的文件，如果获得了 第1个片断（从256k到512k），
        /// 那么这时候，磁盘上文件的大小是 512k（也就是分配了512k），
        /// 尽管第0个片断（从0到256k）还没有获得，但磁盘上会保留这个“空洞”。
        /// </remarks>
        /// <param name="position">文件初始位置</param>
        /// <param name="length">文件长度</param>
        /// <returns>已经分配了磁盘空间返回true,否则返回false</returns>
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
            foreach (string fileName in writeHandles.Keys)
            {
                FileStream old = handles[fileName];
                old.Flush();
                old.Close();
                handles[fileName] = File.OpenRead(fileName);
            }
        }

        /// <summary>
        /// 计算在position位置开始，长度amount的文件范围
        /// </summary>
        /// <remarks>
        /// 这个函数意思是检查 起始位置 为 position，大小为 count 的片断实际位置在哪里？
        /// 例如，假设有两个文件，aaa和bbb，大小分别是 400 和 1000，那么 position 为300，amount为200的文件片断属于哪个文件了？它分别属于两个文件，所以返回的是 
        /// [("aaa", 300, 400), ("bbb", 0, 100)]，
        /// 也就是它既包含了 aaa 文件中从 300 到 400这段数据，也包含了 bbb 文件从 0 到 100 这段数据。
        /// </remarks>
        /// <param name="position">开始位置</param>
        /// <param name="count">文件长度</param>
        /// <returns>返回文件范围</returns>
        private List<FileRange> Intervals(long position, long amount)
        {
            long begin, end;
            string fileName;
            List<FileRange> result = new List<FileRange>();
            long stop = position + amount;
            foreach (FileRange singleFileRange in fileRanges)
            {
                if (singleFileRange.End <= position)
                    continue;
                if (singleFileRange.Begin >= stop)
                    break;

                //fileRange 是一个三元组的列表，三元组格式是（文件名，在该文件的起始位置，在该文件的结束位置）。
                fileName = singleFileRange.FileName;
                begin = Math.Max(position, singleFileRange.Begin) - singleFileRange.Begin;
                end = Math.Min(singleFileRange.End, stop) - singleFileRange.Begin;
                FileRange fileRange = new FileRange(fileName, begin, end);
                result.Add(fileRange);
            }
            return result;
        }

        /// <summary>
        /// 把从offset开始，count长的数据从文件中读出来
        /// </summary>
        /// <param name="position">要读出的字节数组的开始位置</param>
        /// <param name="count">要读出的字节数组大小</param>
        /// <returns>所读出来的字节数值</returns>
        public byte[] Read(long position, long count)
        {
            byte[] result = new byte[count];
            int offset = 0;
            foreach (FileRange singleFileRange in Intervals(position, count))
            {
                FileStream hfStream = handles[singleFileRange.FileName];
                hfStream.Seek(singleFileRange.Begin, SeekOrigin.Begin);
                offset += hfStream.Read(result, offset, (int)(singleFileRange.End - singleFileRange.Begin));
            }
            return result;
        }

        /// <summary>
        /// 把一段字符串写到相应的磁盘文件中。
        /// </summary>
        /// <param name="position">要写入磁盘文件的字节数组的开始位置</param>
        /// <param name="bytes">要写入磁盘文件的字节数组</param>
        public void Write(long positon, byte[] bytes)
        {
            int total = 0;

            foreach (FileRange singleFileRange in Intervals(positon, bytes.LongLength))
            {
                FileStream hfStream;
                //如果该文件并不是以写的方式打开的，那么改成读写方式打开
                if (!writeHandles.ContainsKey(singleFileRange.FileName))
                {
                    hfStream = handles[singleFileRange.FileName];
                    hfStream.Close();
                    handles[singleFileRange.FileName] = File.Open(singleFileRange.FileName, FileMode.Open, FileAccess.ReadWrite);
                    writeHandles[singleFileRange.FileName] = true;
                }
                hfStream = handles[singleFileRange.FileName];

                //通过 seek 函数移动文件指针，可以看出来，文件不是按照顺序来写的，因为所获取的文件片断是随机的，所以写也是随机的。
                //这里有一个疑问，假设获得了第二个文件片断，起始是 1000，大小是500，而第一个片断还没有获得，那么文件指针要移动到
                //1000 处，并写500个字节。这时候，文件的大小应该是 1500，尽管前面 1000 个字节是“空洞”。那么如果，直到结束，都没
                //有获得第一个片断，又如何检测出来了？（通过检查 total？）
                hfStream.Seek(singleFileRange.Begin, SeekOrigin.Begin);
                hfStream.Write(bytes, total, (int)(singleFileRange.End - singleFileRange.Begin));



                total += (int)(singleFileRange.End - singleFileRange.Begin);
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