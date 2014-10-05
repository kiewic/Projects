using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BackgroundTaskApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IAsyncOperation<IUICommand> showOperation;

        public MainPage()
        {
            Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetRedditValuesFromToast(e.Parameter);
        }

        private void DisplayException(Exception ex)
        {
            var dialog = new MessageDialog(ex.ToString(), ex.Message);
            if (showOperation != null)
            {
                showOperation.Cancel();
            }
            showOperation = dialog.ShowAsync();
        }

        #region BT

        private DownloadOperation slowDownload;
        private CancellationTokenSource downloadCancellationTokenSource;

        public void SetRedditValuesFromToast(object parameter)
        {
            RedditItem item = new RedditItem();
            string jsonString = parameter as string;
            if (!String.IsNullOrEmpty(jsonString))
            {
                item.Decode(jsonString);
                RedditTitleBlock.Text = item.Title;
                RedditLinkButton.NavigateUri = new Uri(item.Url);
            }
        }

        private void RedditLinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var asyncInfo = Launcher.LaunchUriAsync(RedditLinkButton.NavigateUri);
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void InvokeToast_Click(object sender, RoutedEventArgs e)
        {
            SongProcessingTask2 task = new SongProcessingTask2();
            task.InvokeSimpleToast();
        }

        private async void RunBackgroundTaskCode_Click(object sender, RoutedEventArgs e)
        {
            SongProcessingTask2 task = new SongProcessingTask2();
            await task.RunAsync(new TaskInstanceMock());
        }

        private async void GetCurrentDownloads_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                CurrentBlock.Text = String.Format("Count: {0}", downloads.Count);
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private async void AttachDownloads_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentBlock.Text = String.Empty;
                IReadOnlyList<DownloadOperation> downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                foreach (var download in downloads)
                {
                    AttachDownloadOperationHandles(download);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void AttachDownloadOperationHandles(DownloadOperation download)
        {
            CurrentBlock.Text += String.Format(
                "Download {0} has status {1}.\r\n",
                download.Guid,
                download.Progress.Status);

            var asyncInfo = download.AttachAsync();
            asyncInfo.Progress = new AsyncOperationProgressHandler<DownloadOperation, DownloadOperation>(OnAttachedDownloadProgress);
            asyncInfo.Completed = new AsyncOperationWithProgressCompletedHandler<DownloadOperation, DownloadOperation>(OnAttachedDownloadCompleted);
        }

        private void OnAttachedDownloadProgress(IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> asyncInfo, DownloadOperation progressInfo)
        {
            string message = String.Empty;
            try
            {
                DownloadOperation download = asyncInfo.GetResults();

                message = String.Format(
                    "Download {0} progress with status {1}.",
                    progressInfo.Guid,
                    progressInfo.Progress.Status);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var runAsyncInfo = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CurrentBlock.Text += String.Format("{0}\r\n", message);
            });
        }

        private void OnAttachedDownloadCompleted(IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> asyncInfo, AsyncStatus asyncStatus)
        {
            string message = String.Empty;
            try
            {
                DownloadOperation download = asyncInfo.GetResults();

                message = String.Format(
                    "Download {0} completed with status {1}.",
                    download.Guid,
                    download.Progress.Status);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var runAsyncInfo = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CurrentBlock.Text += String.Format("{0}\r\n", message);
            });
        }

        private BackgroundDownloader CreateDownloader()
        {
            BackgroundDownloader downloader = new BackgroundDownloader();
            downloader.SetRequestHeader("Cookie", "time=" + DateTime.Now);
            return downloader;
        }

        private async void CreateButDoNotStartDownload_Click(object sender, RoutedEventArgs e)
        {
            BackgroundDownloader downloader = CreateDownloader();
            downloader.ProxyCredential = new Windows.Security.Credentials.PasswordCredential();
            //downloader.ProxyCredential.UserName = "HOLA";
            Debug.WriteLine(downloader.ProxyCredential.UserName.Length);
            Debug.WriteLine(downloader.ProxyCredential.UserName);
            Debug.WriteLine(downloader.ProxyCredential.Password.Length);
            Debug.WriteLine(downloader.ProxyCredential.Password);

            IStorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("neverStarted.txt", CreationCollisionOption.GenerateUniqueName);
            var aDownload = downloader.CreateDownload(new Uri("http://example.com"), file);
            Debug.WriteLine(aDownload.Guid);
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            BackgroundDownloader downloader = CreateDownloader();
            try
            {
                IStorageFile file = await KnownFolders.PicturesLibrary.CreateFileAsync("downloadFile.txt", CreationCollisionOption.GenerateUniqueName);

                Uri uri = new Uri(UriBox.Text);
                slowDownload = downloader.CreateDownload(uri, file);
                downloadCancellationTokenSource = new CancellationTokenSource();

                ProgressBlock.Text = String.Empty;
                DownloadBlock.Text = String.Format("Guid: {0}\r\nPath: {1}", slowDownload.Guid, file.Path);
                HeadersBlock.Text = String.Empty;

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(OnDownloadProgress);
                var task = slowDownload.StartAsync().AsTask(downloadCancellationTokenSource.Token, progressCallback);
                var notAwaited = task.ContinueWith(OnDownloadCompleted, slowDownload);
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        public void OnDownloadProgress(DownloadOperation download)
        {
            // Keep a local copy of current progress.
            BackgroundDownloadProgress progress = download.Progress;

            var notAwiated = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressBlock.Text = String.Format(
                    "Guid: {0}\r\n{1}: {2:N0} of {3:N0}\r\nHasRestarted: {4}\r\nHasResponseChanged: {5}",
                    download.Guid,
                    progress.Status,
                    progress.BytesReceived,
                    progress.TotalBytesToReceive,
                    progress.HasRestarted,
                    progress.HasResponseChanged);
            });
        }

        private void OnDownloadCompleted(Task<DownloadOperation> task, object arg)
        {
            DownloadOperation slowDownload = arg as DownloadOperation;

            // Append headers.
            StringBuilder builder = new StringBuilder();
            ResponseInformation response = slowDownload.GetResponseInformation();

            // If server never replied, 'response' is null.
            if (response != null)
            {
                builder.Append(String.Format("ActualUri: {0}\r\n", response.ActualUri));
                builder.Append(String.Format("StatusCode: {0}\r\n", response.StatusCode));
                builder.Append(String.Format("IsResumable: {0}\r\n", response.IsResumable));
                foreach (var headerPair in response.Headers)
                {
                    builder.Append(String.Format("{0}: {1}\r\n", headerPair.Key, headerPair.Value));
                }
            }

            // Format excpetion info.
            string exceptionInfo = String.Empty;
            if (task.Exception != null)
            {
                exceptionInfo = String.Format(
                    "0x{0:X8} {1}",
                    task.Exception.HResult,
                    task.Exception.Message);
            }

            // Format download info.
            string downloadInfo = String.Format(
                "Guid: {0}\r\nStatus: {1}\r\nException: {2}",
                slowDownload.Guid,
                task.Status,
                exceptionInfo);

            var notAwaited = Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {
                DownloadBlock.Text = downloadInfo;
                HeadersBlock.Text = builder.ToString();
            });
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (slowDownload != null)
                {
                    slowDownload.Pause();
                }
            }
            catch (InvalidOperationException ex)
            {
                DisplayException(ex);
            }
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (slowDownload != null)
                {
                    slowDownload.Resume();
                }
            }
            catch (InvalidOperationException ex)
            {
                DisplayException(ex);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (downloadCancellationTokenSource != null)
            {
                downloadCancellationTokenSource.Cancel();
                downloadCancellationTokenSource = null;
            }
        }

        #endregion

        #region HttpClient

        private void GetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(false, String.Empty, String.Empty, String.Empty);
        }

        private void IgnoreCertErrorsGetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(true, String.Empty, String.Empty, String.Empty);
        }

        private void ClientCertGetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(true, String.Empty, String.Empty, "tempClientCert");
        }

        private void NoCacheGetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(false, "Cache-Control", "no-cache", string.Empty);
        }

        private void NoStoreGetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(false, "Cache-Control", "no-store", String.Empty);
        }

        private void OnlyIfCachedGetAsync_Click(object sender, RoutedEventArgs e)
        {
            DoGet(false, "Cache-Control", "only-if-cached", String.Empty);
        }

        private async void DoGet(bool ignoreCertErrors, string key, string value, string friendlyName)
        {
            ExceptionBlock.Text = String.Empty;
            CertErrorsBlock.Text = String.Empty;
            ResponseContentBlock.Text = String.Empty;

            HttpRequestMessage request = null;
            try
            {
                request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(UriBox2.Text));

                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();

                if (ignoreCertErrors)
                {
                    filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                    filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationFailure);
                }

                if (!String.IsNullOrEmpty(friendlyName))
                {
                    CertificateQuery query = new CertificateQuery();
                    query.FriendlyName = friendlyName;
                    IReadOnlyCollection<Certificate> certs = await CertificateStores.FindAllAsync(query);
                    filter.ClientCertificate = certs.ElementAt(0);
                }

                HttpClient client = new HttpClient(filter);
                
                if (!String.IsNullOrEmpty(key))
                {
                    client.DefaultRequestHeaders.Add(key, value);
                }

                HttpResponseMessage response = await client.SendRequestAsync(request);

                ExceptionBlock.Text = response.ReasonPhrase;
                ResponseContentBlock.Text = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                ExceptionBlock.Text = ex.ToString();

                // Something like: 'Untrusted, InvalidName, RevocationFailure'
                CertErrorsBlock.Text = String.Join(
                    ", ",
                    request.TransportInformation.ServerCertificateErrors);
            }
        }

        #endregion

        #region Certs

        private async void InstallClientCert_Click(object sender, RoutedEventArgs e)
        {
            InstallClientCertCompleted.Visibility = Visibility.Collapsed;

            try
            {

                Uri uri = new Uri("ms-appx:///Assets/tempClientCert.pfx");
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                IBuffer buffer = await FileIO.ReadBufferAsync(file);

                //byte[] bytes;
                //CryptographicBuffer.CopyToByteArray(buffer, out bytes);

                string pfxData = CryptographicBuffer.EncodeToBase64String(buffer);

                // UserCertificateEnrollmentManager requires 'Shared User certificates' capability.
                //await CertificateEnrollmentManager.UserCertificateEnrollmentManager.ImportPfxDataAsync(...);

                await CertificateEnrollmentManager.ImportPfxDataAsync(
                    pfxData,
                    String.Empty, // password
                    ExportOption.Exportable,
                    KeyProtectionLevel.NoConsent,
                    InstallOptions.None,
                    "tempClientCert");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

            InstallClientCertCompleted.Visibility = Visibility.Visible;
        }

        private async void QueryCerts_Click(object sender, RoutedEventArgs e)
        {
            QueryCertsCompleted.Visibility = Visibility.Collapsed;

            try
            {
                IReadOnlyCollection<Certificate> certs = await CertificateStores.FindAllAsync();

                StringBuilder builder = new StringBuilder();

                // Append count.
                builder.AppendFormat("Count: {0}\r\n", certs.Count);

                foreach (var cert in certs)
                {
                    builder.Append("--------------------\r\n");
                    builder.AppendFormat("EnhancedKeyUsages: {0}\r\n", String.Join(", ", cert.EnhancedKeyUsages));
                    builder.AppendFormat("FriendlyName: {0}\r\n", cert.FriendlyName);
                    builder.AppendFormat("HasPrivateKey: {0}\r\n", cert.HasPrivateKey);
                    builder.AppendFormat("IsStronglyProtected: {0}\r\n", cert.IsStronglyProtected);
                    builder.AppendFormat("Issuer: {0}\r\n", cert.Issuer);
                    builder.AppendFormat("SerialNumber: {0}\r\n", BitConverter.ToString(cert.SerialNumber));
                    builder.AppendFormat("Subject: {0}\r\n", cert.Subject);
                    builder.AppendFormat("ValidFrom: {0}\r\n", cert.ValidFrom);
                    builder.AppendFormat("ValidTo: {0}\r\n", cert.ValidTo);

                    string thumbprint = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(cert.GetHashValue()));
                    builder.AppendFormat("Thumbprint: {0}\r\n", thumbprint);
                }

                CertsBlock.Text = builder.ToString();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

            QueryCertsCompleted.Visibility = Visibility.Visible;
        }

        private async void DeleteCerts_Click(object sender, RoutedEventArgs e)
        {
            DeleteCertsCompleted.Visibility = Visibility.Collapsed;

            try
            {
                IReadOnlyCollection<Certificate> certs = await CertificateStores.FindAllAsync();

                StringBuilder builder = new StringBuilder();

                // Append count.
                builder.AppendFormat("Count: {0}\r\n", certs.Count);

                foreach (var cert in certs)
                {
                    builder.Append("--------------------\r\n");
                    builder.AppendFormat("Issuer: {0}\r\n", cert.Issuer);
                    builder.AppendFormat("Subject: {0}\r\n", cert.Subject);

                    string thumbprint = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(cert.GetHashValue()));
                    builder.AppendFormat("Thumbprint: {0}\r\n", thumbprint);

                    // Not working:
                    CertificateStores.IntermediateCertificationAuthorities.Delete(cert);
                }

                CertsBlock.Text = builder.ToString();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

            DeleteCertsCompleted.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
