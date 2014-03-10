using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Syndication;

namespace FeedsNotifier
{
    class Program
    {
        private static SyndicationClient client;
        private static string[] websites;
        private static string[] keywords;

        static void Main(string[] args)
        {
            client = new SyndicationClient();
            client.BypassCacheOnRetrieve = true;

            PrintVersion();
            LoadSettings();
            KeywordsToLower();
            GetFeeds();
        }

        private static void LoadSettings()
        {
            const string fileName = "Settings.json";
            string jsonString = File.ReadAllText(fileName);
            JsonObject jsonObject;
            if (!JsonObject.TryParse(jsonString, out jsonObject))
            {
                throw new Exception(String.Format("Invalid JSON object in {0}.", fileName));
            }

            JsonArray websitesArray = jsonObject.GetNamedArrayOrEmptyArray("websites");
            websites = websitesArray.ToStringArray();

            JsonArray keywordsArray = jsonObject.GetNamedArrayOrEmptyArray("keywords");
            keywords = keywordsArray.ToStringArray();
        }

        private static void KeywordsToLower()
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].ToLower();
            }
        }

        private static void GetFeeds()
        {
            if (websites == null)
            {
                throw new Exception("websites array is null.");
            }

            List<Task> tasks = new List<Task>();
            foreach (string website in websites)
            {
                tasks.Add(GetFeed(website));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task GetFeed(string uriString)
        {
            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(new Uri(uriString));
                Console.WriteLine("Feed: {0}", feed.Title.Text);

                foreach (SyndicationItem item in feed.Items)
                {
                    LookForKeywords(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void LookForKeywords(SyndicationItem item)
        {
            string title = item.Title != null ? item.Title.Text.ToLower() : String.Empty;
            string content = item.Content != null ? item.Content.Text.ToLower() : String.Empty;
            string summary = item.Summary.Text != null ? item.Summary.Text.ToLower() : String.Empty;

            foreach (string keyword in keywords)
            {
                if (title.Contains(keyword) || content.Contains(keyword) || summary.Contains(keyword))
                {
                    PrepareToSend(item);
                    break;
                }
            }
        }

        private static void PrepareToSend(SyndicationItem item)
        {
            Console.WriteLine("Match: {0}", item.Id);

            string title = item.Title != null ? item.Title.Text : String.Empty;
            string summary = item.Summary.Text != null ? item.Summary.Text : String.Empty;
            string id = item.Id;
            byte[] idAsBytes = Encoding.UTF8.GetBytes(id);
            string idAsBase64 = Convert.ToBase64String(idAsBytes);
            string fileName = idAsBase64 + ".txt";

            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                // Questions already sent.
                return;
            }

            Uri link = null;
            if (item.Links.Count > 0)
            {
                link = item.Links[0].Uri;
            }

            string body = String.Format("<p>{0}</p>\r\n{1}", link.AbsoluteUri, summary);

            File.WriteAllText(fileName, body);

            try
            {
                SendEmail(title, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                // Undo file.
                File.Delete(fileName);
            }
        }

        private static void PrintVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Version: {0}", version);
        }

        private static void SendEmail(string subject, string body)
        {
            var mail = new MailMessage();
            mail.To.Add("kiewic+azure@gmail.com");
            mail.From = new MailAddress("FeedsNotifier <robot@http2.cloudapp.net>");
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            var smtpServer = new SmtpClient("localhost"); // smtp.gmail.com
            smtpServer.Port = 25; // 587
            //smtpServer.Credentials = new NetworkCredential("borrameborrame", GetCredential());
            //smtpServer.EnableSsl = true;
            smtpServer.Send(mail);
        }
    }
}
