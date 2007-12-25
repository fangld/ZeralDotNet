using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestEndGameDownloaders
{
    class DummyNormalDownloader : NormalDownloader
    {
        public DummyNormalDownloader(IStorageWrapper storageWrapper, int pieceNumber, List<SingleDownload> downloads)
            : base(storageWrapper, pieceNumber, downloads) { }
    }
}
