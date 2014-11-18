using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Web.Syndication;
using Windows.UI.StartScreen;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class FeedInfo
    {
        private const int MaxNotifications = 3;

        // Use this lock to avoid multiple threads adding or removing feeds simultaneously.
        private static object feedsLock = new Object();

        public Uri FeedUri { get; set; }
        public Uri LinkUri { get; set; }
        public Uri ImageUri { get; set; }
        public Guid Id { get; set; }
        public string Title { get; set; }

        internal FeedInfo(Uri uri, SyndicationFeed feed)
        {
            Id = Guid.NewGuid();
            FeedUri = uri;
            Title = feed.Title != null ? feed.Title.Text : String.Empty;
            LinkUri = feed.Links.Count > 0 ? feed.Links[0].Uri : null;
            ImageUri = feed.ImageUri != null ? feed.ImageUri : null;
        }

        public FeedInfo(string guidString, string jsonString)
        {
            if (String.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentNullException("jsonString");
            }

            Id = new Guid(guidString);
            FromJson(jsonString);
        }

        internal void Save()
        {
            lock (feedsLock)
            {
                ApplicationDataCompositeValue feeds = LocalValues.GetFeeds();
                feeds.Add(Id.ToString(), ToJson());
                LocalValues.SetFeeds(feeds);
            }
        }

        private void Remove()
        {
            lock (feedsLock)
            {
                ApplicationDataCompositeValue feeds = LocalValues.GetFeeds();
                feeds.Remove(Id.ToString());
                LocalValues.SetFeeds(feeds);
            }
        }

        internal async Task<bool> CreateTileAsync()
        {
            if (Id == Guid.Empty)
            {
                throw new Exception("Id cannot be empty.");
            }

            try
            {
                SecondaryTile tile = new SecondaryTile(
                    Id.ToString(),
                    Title,
                    ToJson(), // TODO: Find out if this can be personalized per notification.
                    new Uri("ms-appx:///Assets/Logo.scale-100.png"),
                    TileSize.Wide310x150);

                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.ShowNameOnWide310x150Logo = true;
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.scale-100.png");
                tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.scale-100.png");

                return await tile.RequestCreateAsync();
            }
            catch (Exception ex)
            {
                Logger.Append("RequestCreateAsync", ex);
            }

            return false;
        }

        internal void UpdateTile(SyndicationFeed feed)
        {
            if (Id == Guid.Empty)
            {
                throw new Exception("Id cannot be empty.");
            }

            if (!HasTile())
            {
                Debug.WriteLine("Feed without tile: {0}", this);
                Logger.Append("Feed without tile", this);
                Remove();
                return;
            }

            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(Id.ToString());
            updater.EnableNotificationQueue(true);
            updater.Clear();

            int itemCount = 0;

            foreach (SyndicationItem item in feed.Items)
            {
                // Don't create more than 5 notifications.
                if (itemCount++ >= MaxNotifications)
                {
                    break;
                }

                ItemInfo itemInfo = new ItemInfo(this, item);
                Logger.Builder.Append(itemInfo.ToLog());

                updater.Update(itemInfo.GetTileNotification());
            }
        }

        private bool HasTile()
        {
            return SecondaryTile.Exists(Id.ToString());
        }

        private string ToJson()
        {
            JsonObject feedObject = new JsonObject();

            feedObject.Add("FeedUri", JsonValue.CreateStringValue(FeedUri.ToString()));

            feedObject.Add("Title", JsonValue.CreateStringValue(Title));

            string linkUriString = LinkUri != null ? LinkUri.ToString() : String.Empty;
            feedObject.Add("LinkUri", JsonValue.CreateStringValue(linkUriString));

            string imageUriString = ImageUri != null ? ImageUri.ToString() : String.Empty;
            feedObject.Add("ImageUri", JsonValue.CreateStringValue(imageUriString));

            return feedObject.Stringify();
        }

        private void FromJson(string jsonString)
        {
            JsonObject feedObject = JsonObject.Parse(jsonString);

            if (feedObject.ContainsKey("FeedUri"))
            {
                Uri feedUri;
                if (Uri.TryCreate(feedObject.GetNamedString("FeedUri"), UriKind.Absolute, out feedUri))
                {
                    FeedUri = feedUri;
                }
            }

            if (feedObject.ContainsKey("Title"))
            {
                Title = feedObject.GetNamedString("Title");
            }

            if (feedObject.ContainsKey("LinkUri"))
            {
                Uri linkUri;
                if (Uri.TryCreate(feedObject.GetNamedString("LinkUri"), UriKind.Absolute, out linkUri))
                {
                    LinkUri = linkUri;
                }
            }

            if (feedObject.ContainsKey("ImageUri"))
            {
                Uri imageUri;
                if (Uri.TryCreate(feedObject.GetNamedString("ImageUri"), UriKind.Absolute, out imageUri))
                {
                    ImageUri = imageUri;
                }
            }
        }

        public override string ToString()
        {
            return FeedUri.ToString();
        }
    }
}
