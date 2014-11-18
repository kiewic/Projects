using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Tasks
{
    public sealed class RedditItem
    {
        const string LastRedditItemKey = "LastRedditItem";

        public string LastKey
        {
            get
            {
                IPropertySet localValues = ApplicationData.Current.LocalSettings.Values;

                if (localValues.ContainsKey(LastRedditItemKey))
                {
                    return localValues[LastRedditItemKey] as string;
                }

                return String.Empty;
            }
            set
            {
                IPropertySet localValues = ApplicationData.Current.LocalSettings.Values;
                localValues[LastRedditItemKey] = value;
            }
        }

        public string Key { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string Url { get; set; }
        public string Permalink { get; set; }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue("Title", JsonValue.CreateStringValue(Title));
            jsonObject.SetNamedValue("Url", JsonValue.CreateStringValue(Url));
            jsonObject.SetNamedValue("Permalink", JsonValue.CreateStringValue(Permalink));
            return jsonObject.Stringify();
        }

        public bool TryParse(string jsonString)
        {
            JsonObject jsonObject;
            if (!JsonObject.TryParse(jsonString, out jsonObject))
            {
                return false;
            }

            if (jsonObject.Keys.Contains("Url"))
            {
                Url = jsonObject.GetNamedString("Url");
            }

            if (jsonObject.Keys.Contains("Title"))
            {
                Title = jsonObject.GetNamedString("Title");
            }

            if (jsonObject.Keys.Contains("Permalink"))
            {
                Permalink = jsonObject.GetNamedString("Permalink");
            }

            return true;
        }

        public bool TryUrlToUri(out Uri uri)
        {
            return Uri.TryCreate(new Uri("https://www.reddit.com"), Url, out uri);
        }

        public bool TryPermalinkToUri(out Uri uri)
        {
            return Uri.TryCreate(new Uri("https://www.reddit.com"), Permalink, out uri);
        }
    }
}
