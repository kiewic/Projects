using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Syndication;

namespace Tasks
{
    public sealed class FeedManager
    {
        public IAsyncAction AddFeedAsync(string uriString)
        {
            return AddFeedAsyncInternal(uriString).AsAsyncAction();
        }

        internal async Task AddFeedAsyncInternal(string uriString)
        {
            Uri uri = new Uri(FixUriString(uriString));
            bool invalidXml = false;
            try
            {
                await GetAndSaveFeedAsync(uri);
            }
            catch (Exception ex)
            {
                // Swallow invalid XML exceptions.
                if (ex.HResult != unchecked((int)0x83750002))
                {
                    Debug.WriteLine(ex);
                    Logger.Append("GetFeedAsync", ex);
                    throw;
                }

                invalidXml = true;
            }

            try
            {
                if (invalidXml)
                {
                    await GetAndSaveFeedFromWebsiteAsync(uri);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Append("GetFeedFromWebsiteAsync", ex);
                throw;
            }

            await Logger.CommitAsync();
        }

        private string FixUriString(string uriString)
        {
            uriString = uriString.Trim();
            if (!uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                uriString = "http://" + uriString;
            }
            return uriString;
        }

        private async Task GetAndSaveFeedAsync(Uri uri)
        {
            SyndicationFeed feed = await GetFeedAsync(uri);
            await SaveFeedAsync(uri, feed);
        }

        private async Task<SyndicationFeed> GetFeedAsync(Uri uri)
        {
            SyndicationClient client = new SyndicationClient();
            client.BypassCacheOnRetrieve = true;
            return await client.RetrieveFeedAsync(uri);
        }

        private async Task GetAndSaveFeedFromWebsiteAsync(Uri uri)
        {
            List<Uri> newUris = new List<Uri>();

            HttpClient client = new HttpClient();
            string stringContent = await client.GetStringAsync(uri);

            // Example: <link rel="alternate" type="application/atom+xml" title="Feed for question &#39;Windows Store App - how to get watermark in Search Charm textbox&#39;" href="/feeds/question/12804437">
            MatchCollection matches = Regex.Matches(stringContent, "<link.*rel\\s*=\\s*\"alternate\".*href=\"(.*)\".*>", RegexOptions.IgnoreCase);

            // TODO: Log if URI does not contain any matches.

            foreach (Match match in matches)
            {
                // TODO: Log every match.

                Debug.WriteLine("{0} at {1}", match.Value, match.Index);

                if (match.Groups.Count >= 2)
                {
                    Uri newUri;
                    if (Uri.TryCreate(uri, match.Groups[1].Value, out newUri))
                    {
                        Debug.WriteLine(newUri);
                        newUris.Add(newUri);
                    }
                }
            }

            for (int index = 0; index < newUris.Count; index++)
            {
                try
                {
                    await GetAndSaveFeedAsync(newUris[index]);
                    return;
                }
                catch (Exception ex)
                {
                    // TODO: Log URI and exception.

                    Debug.WriteLine(ex);

                    // Only throw exception if it is the last URI in the list.
                    if (index >= newUris.Count - 1)
                    {
                        throw;
                    }
                }
            }

            throw new Exception("The URL is not a RSS feed and it does not contain links to RSS feeds.");
        }

        private async Task SaveFeedAsync(Uri uri, SyndicationFeed feed)
        {
            FeedInfo feedInfo = new FeedInfo(uri, feed);

            bool pinned = await feedInfo.CreateTileAsync();
            if (pinned)
            {
                feedInfo.Save();
            }

            feedInfo.UpdateTile(feed);
        }

        internal async Task UpdateTilesAsync()
        {
            ApplicationDataCompositeValue feeds = LocalValues.GetFeeds();
            List<Task> tasks = new List<Task>();

            foreach (KeyValuePair<string, object> pair in feeds)
            {
                try
                {
                    FeedInfo feedInfo = new FeedInfo(pair.Key, pair.Value as string);
                    Logger.Append("Feed found", feedInfo);

                    Task<SyndicationFeed> getFeedTask = GetFeedAsync(feedInfo.FeedUri);
                    Logger.Append("Feed downloaded", feedInfo);

                    Task continueTask = getFeedTask.ContinueWith(OnGetFeed, feedInfo);

                    tasks.Add(continueTask);
                }
                catch (Exception ex)
                {
                    // Don't let a single feed ruins the rest of the feeds.
                    Debug.WriteLine(ex);
                    Logger.Append("Feed exeption", ex);
                }
            }

            await Task.WhenAll(tasks);
        }

        internal void OnGetFeed(Task<SyndicationFeed> getFeedTask, object stateObject)
        {
            try
            {
                FeedInfo feedInfo =  stateObject as FeedInfo;
                feedInfo.UpdateTile(getFeedTask.Result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
