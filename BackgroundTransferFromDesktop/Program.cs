using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BackgroundTransferFromDesktop
{
    class Program
    {
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            DoDownload();
            autoResetEvent.WaitOne();
        }

        private static async void DoDownload()
        {
            try
            {
                Uri uri = new Uri("http://kiewic.com");

                IStorageFile resultFile = await KnownFolders.PicturesLibrary.CreateFileAsync("blah.html", CreationCollisionOption.GenerateUniqueName);

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(uri, resultFile);
                await download.StartAsync();

                Debug.WriteLine(download.Progress.Status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
