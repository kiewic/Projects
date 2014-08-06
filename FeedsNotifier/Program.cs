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
using System.Windows.Forms;

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
            string fullName = ToAbsolutePath(fileName);
            string jsonString = File.ReadAllText(fullName);
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

        private static string ToAbsolutePath(string path)
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + "\\" + path;
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
            WriteGreenLine(String.Format("Match: {0}", item.Id));

            string title = item.Title != null ? item.Title.Text : String.Empty;
            string summary = item.Summary.Text != null ? item.Summary.Text : String.Empty;
            string id = item.Id;
            byte[] idAsBytes = Encoding.UTF8.GetBytes(id);
            string idAsBase64 = Convert.ToBase64String(idAsBytes);
            string fileName = String.Format("temp\\{0}.txt", idAsBase64);
            string fullName = ToAbsolutePath(fileName);

            FileInfo fileInfo = new FileInfo(fullName);
            if (fileInfo.Exists)
            {
                // Questions already sent.
                WriteYellowLine(String.Format("Already sent: {0}", item.Id));
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
                WriteRedLine(ex.Message);

                // Undo file.
                File.Delete(fileName);
            }
        }

        private static void PrintVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Version: {0}", version);
        }

        private static void WriteGreenLine(string str)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = originalColor;
        }

        private static void WriteRedLine(string str)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = originalColor;
        }

        private static void WriteYellowLine(string str)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = originalColor;
        }

        private static void SendEmail(string subject, string body)
        {
            var mail = new MailMessage();
            mail.To.Add("kiewic@gmail.com");
            mail.From = new MailAddress("FeedsNotifier <me@kiewic.com>");
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
