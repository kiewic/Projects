using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Tasks
{
    public sealed class SongProcessingTask2 : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private RedditItem item = new RedditItem();
        private string taskIdString;
        private string instanceIdString;
        private int iteration;

        public SongProcessingTask2()
        {
            item.Title = "This is a test.";
            item.Thumbnail = "http://kiewic.com//Content/Home/Icons/MsapplicationSquare150x150logo.png";
            item.Url = "http://kiewic.com";
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                deferral = taskInstance.GetDeferral();

                taskIdString = taskInstance.Task.TaskId.ToString();
                instanceIdString = taskInstance.InstanceId.ToString();

                await RunCoreAsync(taskInstance);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private string FormatException(Exception ex)
        {
            return String.Format("0x{0:X8} {1}", ex.HResult, ex.Message);
        }

        public Windows.Foundation.IAsyncAction RunAsync(IBackgroundTaskInstance taskInstance)
        {
            taskIdString = "nomnomnom"; // This should be constant. There is only one task registered.
            instanceIdString = Guid.NewGuid().ToString(); // This should be different on each instance.
            return RunCoreAsync(taskInstance).AsAsyncAction();
        }

        private async Task RunCoreAsync(IBackgroundTaskInstance taskInstance)
        {
            // It seems we can trigger multiple progress from a background task.
            taskInstance.Progress = 3;

            iteration = GetIterationNumber(taskIdString);

            await CallRedditApi();
            await StartDownload();

            IfNewItemInvokeToast();

            SetIterationNumber(taskIdString);
        }

        private void SetIterationNumber(string taskIdString)
        {
            IPropertySet localValues = ApplicationData.Current.LocalSettings.Values;
            localValues[taskIdString] = iteration;
        }

        private int GetIterationNumber(string taskIdString)
        {
            IPropertySet localValues = ApplicationData.Current.LocalSettings.Values;

            if (localValues.ContainsKey(taskIdString))
            {
                return (int)localValues[taskIdString] + 1;
            }

            return 1;
        }

        private void SaveDownloadOperationInfo(Guid transferGuid)
        {
            ApplicationData.Current.LocalSettings.Values.Add(instanceIdString, transferGuid);
        }

        private async Task StartDownload()
        {
            try
            {
                string extension = Path.GetExtension(item.Url);

                if (String.IsNullOrEmpty(extension))
                {
                    extension = ".png";
                }

                DateTime now = DateTime.Now;
                string fileName = Path.GetFileName(String.Format(
                    "{0:D4}_{1:D2}_{2:D2}_at_{3:D2}_{4:D2}_{5:D2}{6}",
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    extension));

                Debug.Assert(!String.IsNullOrEmpty(fileName));

                IStorageFile file = (await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName)) as IStorageFile;
                if (file != null)
                {
                    // File already exists, do not get it again.
                    return;
                }

                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                Debug.WriteLine("Download will be placed at {0}", file.Path);

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation transfer = downloader.CreateDownload(new Uri(item.Url), file);

                //// System.InvalidOperationException: WinRT information: This method must be called on a UI thread.
                //var result = await BackgroundDownloader.RequestUnconstrainedDownloadsAsync(new DownloadOperation[] { transfer });
                //Debug.WriteLine(result);

                var operation = transfer.StartAsync();

                SaveDownloadOperationInfo(transfer.Guid);
            }
            catch (Exception ex)
            {
                item.Title = FormatException(ex);
            }
        }

        private async Task CallRedditApi()
        {
            try
            {
                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                HttpClient client = new HttpClient(filter);

                // API documentation at http://www.reddit.com/dev/api
                string jsonString = await client.GetStringAsync(new Uri("http://www.reddit.com/r/aww/hot.json"));

                JsonObject jsonObject = JsonObject.Parse(jsonString);

                JsonArray jsonArray = jsonObject.GetNamedObject("data").GetNamedArray("children");
                jsonObject = GetFirstValidChildren(jsonArray);

                if (jsonObject != null)
                {
                    JsonObject data = jsonObject.GetNamedObject("data");

                    if (data.Keys.Contains("thumbnail"))
                    {
                        item.Thumbnail = data.GetNamedString("thumbnail");
                        item.Key = item.Thumbnail;
                    }
                    if (data.Keys.Contains("url"))
                    {
                        item.Url = data.GetNamedString("url");
                    }
                    if (data.Keys.Contains("title"))
                    {
                        item.Title = data.GetNamedString("title");
                    }
                }
            }
            catch (Exception ex)
            {
                item.Title = FormatException(ex);
            }
        }

        private JsonObject GetFirstValidChildren(JsonArray jsonArray)
        {
            foreach (IJsonValue jsonValue in jsonArray)
            {
                JsonObject jsonObject = jsonValue.GetObject();
                if (IsValidChildren(jsonObject))
                {
                    return jsonObject;
                }
            }

            throw new Exception("No valid children.");
        }

        private bool IsValidChildren(JsonObject jsonObject)
        {
            JsonObject data = jsonObject.GetNamedObject("data");
            if (data.Keys.Contains("is_self") && data.GetNamedBoolean("is_self") == false)
            {
                return true;
            }
            return false;
        }

        private void IfNewItemInvokeToast()
        {
            if (item.Key != item.LastKey)
            {
                item.LastKey = item.Key;
                InvokeSimpleToast();
            }
            else
            {
                // TODO: Temporarely show th etoast anyway.
                InvokeSimpleToast();

                Debug.WriteLine("Nothing new to show.");
            }
        }

        public void InvokeSimpleToast()
        {
            // GetTemplateContent returns a Windows.Data.Xml.Dom.XmlDocument object containing
            // the toast XML
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // You can use the methods from the XML document to specify all of the
            // required parameters for the toast
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements.Item(0).AppendChild(toastXml.CreateTextNode(String.Format("Maintanance Task {0}", iteration)));
            stringElements.Item(1).AppendChild(toastXml.CreateTextNode(item.Title));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");

            XmlAttribute launchAttribute = toastXml.CreateAttribute("launch");
            launchAttribute.Value = item.Encode();
            toastNode.Attributes.SetNamedItem(launchAttribute);

            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            //((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/kiewic.png");
            if (!String.IsNullOrEmpty(item.Thumbnail))
            {
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", item.Thumbnail);
            }
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "red graphic");

            // Create a toast from the Xml, then create a ToastNotifier object to show the toast
            Debug.WriteLine(toastXml.GetXml());
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
