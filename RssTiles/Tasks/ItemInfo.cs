using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.Data.Html;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Web.Syndication;

namespace Tasks
{
    public sealed class ItemInfo
    {
        private const int HoursToExpiration = 8;

        public FeedInfo Feed { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public Uri ItemUri { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public DateTimeOffset PublishedDate { get; set; }

        public DateTimeOffset ExpirationTime {
            get
            {
                return PublishedDate.AddHours(HoursToExpiration);
            }
        }

        public ItemInfo(string jsonString)
        {
            FromJson(jsonString);
        }

        public ItemInfo(FeedInfo feedInfo, SyndicationItem item)
        {
            Feed = feedInfo;
            Id = item.Id;
            Title = item.Title != null ? item.Title.Text : String.Empty;
            ItemUri = item.ItemUri;
            Summary = item.Summary != null ? item.Summary.Text : String.Empty;
            Content = item.Content != null ? item.Content.Text : String.Empty;
            PublishedDate = item.PublishedDate;
        }

        internal TileNotification GetTileNotification()
        {
            XmlDocument square150x150Xml = GetSquare150x150Tile();
            XmlDocument wide310x150Xml = GetWide310x150Tile();
            XmlDocument square310x310Xml = GetSquare310x310Tile();

            IXmlNode square150x150Visual = square150x150Xml.GetElementsByTagName("visual")[0];
            IXmlNode wide310x150Binding = wide310x150Xml.GetElementsByTagName("binding")[0];
            IXmlNode square310x310Binding = square310x310Xml.GetElementsByTagName("binding")[0];

            IXmlNode subnode = square150x150Xml.ImportNode(wide310x150Binding, true);
            square150x150Visual.AppendChild(subnode);

            subnode = square150x150Xml.ImportNode(square310x310Binding, true);
            square150x150Visual.AppendChild(subnode);

            Debug.WriteLine(square150x150Xml.GetXml());

            TileNotification tileNotification = new TileNotification(square150x150Xml);
            tileNotification.ExpirationTime = ExpirationTime;

            return tileNotification;
        }

        internal XmlDocument GetSquare150x150Tile()
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text04);

            Debug.WriteLine(tileXml.GetXml());

            tileXml.GetElementsByTagName("text")[0].InnerText = Title;

            SetNoneBranding(tileXml);

            return tileXml;
        }

        private XmlDocument GetWide310x150Tile()
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text09);

            Debug.WriteLine(tileXml.GetXml());

            tileXml.GetElementsByTagName("text")[0].InnerText = Feed.Title;
            tileXml.GetElementsByTagName("text")[1].InnerText = Title;

            SetNoneBranding(tileXml);

            return tileXml;
        }

        private XmlDocument GetSquare310x310Tile()
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndTextOverlay02);

            Debug.WriteLine(tileXml.GetXml());

            tileXml.GetElementsByTagName("text")[0].InnerText = Title;
            tileXml.GetElementsByTagName("text")[1].InnerText = GetContent();

            SetNoneBranding(tileXml);

            return tileXml;
        }

        private void SetNoneBranding(XmlDocument tileXml)
        {
            XmlAttribute brandingXml = tileXml.CreateAttribute("branding");
            brandingXml.Value = "none";
            tileXml.GetElementsByTagName("binding")[0].Attributes.SetNamedItem(brandingXml);
        }

        private string GetContent()
        {
            string content = RemoveNull(HtmlUtilities.ConvertToText(Summary));
            if (String.IsNullOrEmpty(content))
            {
                content = RemoveNull(HtmlUtilities.ConvertToText(Content));
            }
            return content.Substring(0, content.Length < 500 ? content.Length : 500);
        }

        private string ToJsonTag()
        {
            JsonObject tagObject = new JsonObject();

            if (ItemUri != null)
            {
                tagObject.Add("ItemUri", JsonValue.CreateStringValue(ItemUri.ToString()));
            }

            return tagObject.Stringify();
        }

        public void FromJson(string jsonString)
        {
            JsonObject tagObject = new JsonObject();
            if (!JsonObject.TryParse(jsonString, out tagObject))
            {
                // Invalid JSON.
                return;
            }

            if (tagObject.ContainsKey("ItemUri"))
            {
                IJsonValue itemUri = tagObject["ItemUri"];
                if (itemUri.ValueType == JsonValueType.String)
                {
                    ItemUri = new Uri(itemUri.GetString());
                }
                tagObject.Add("ItemUri", JsonValue.CreateStringValue(ItemUri.ToString()));
            }
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public string ToLog()
        {
            return String.Format("Item: {0}, ExpirationTime: {1}\r\n", this, ExpirationTime);
        }

        private string RemoveNull(string text)
        {
            text = text.Trim();
            text = text.Replace("\0", "");
            return text;
        }
    }
}
