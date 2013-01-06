using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public abstract class Storage : IDisposable
    {
        //public byte[] Buffer;

        //public MetaInfo MetaInfo;

        //public Dictionary<int, TestPiece> HashFailBuffers;

        #region Methods

        public static Storage Create(MetaInfo metaInfo, string saveAsDirectory)
        {
            Storage result;
            if (metaInfo.Mode == MetaInfoMode.SingleFile)
            {
                result = new SingleFileStorage(metaInfo, saveAsDirectory);
            }
            else
            {
                result = new MultiFileStorage(metaInfo, saveAsDirectory);
            }
            //result.MetaInfo = metaInfo;
            //result.Buffer = new byte[metaInfo.SumLength];
            //result.HashFailBuffers = new Dictionary<int, TestPiece>();
            return result;
        }

        //public void MoveHashFail(int index)
        //{
        //    if (index != MetaInfo.PieceListCount - 1)
        //    {
        //        byte[] bytes = new byte[MetaInfo.PieceLength];
        //        Array.Copy(Buffer, index * MetaInfo.PieceLength, bytes, 0, MetaInfo.PieceLength);
        //        byte[] hash = Globals.GetSha1Hash(bytes);//Globals.Sha1.ComputeHash(bytes);
        //        TestPiece tp = new TestPiece();
        //        tp.Index = index;
        //        tp.Hash = hash;
        //        tp.Buffer = bytes;
        //        if (HashFailBuffers.ContainsKey(index))
        //        {
        //            HashFailBuffers[index] = tp;
        //        }
        //        else
        //        {
        //            HashFailBuffers.Add(index, tp);                    
        //        }
        //    }
        //}

        public abstract void Write(byte[] buffer, long offset);

        public abstract int Read(byte[] buffer, long offset, int count);

        public abstract void Close();

        public abstract void Dispose();

        public abstract void SetReadOnly();

        #endregion
    }
}
