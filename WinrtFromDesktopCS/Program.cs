using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Syndication;

namespace WinrtFromDesktopCS
{
    class Program
    {
        static string[] keys = new string[] {
            "announce",
            "forumsredesign",
            "whatforum",
            "reportabug",
            "suggest",
            "Offtopic",
            "Profile",
            "csharpgeneral",
            "vbgeneral",
            "vcgeneral",
            "vclanguage",
            "parallelcppnative",
            "fsharpgeneral",
            "roslyn",
            "visualstudiogeneral",
            "vssetup",
            "vsx",
            "vsto",
            "vseditor",
            "msbuild",
            "lsextensibility",
            "lightswitch",
            "vsarch",
            "vsmantest",
            "vstest",
            "vsunittest",
            "vsdebug",
            "vsta",
            "ApplicationInsights",
            "TFService",
            "winappsuidesign",
            "tailoringappsfordevices",
            "winappswithhtml5",
            "winappswithnativecode",
            "winappswithcsharp",
            "wingameswithdirectx",
            "toolsforwinapps",
            "windowsstore",
            "AppProducer",
            "windowscompatibility",
            "powermanagement",
            "windowsaccessibilityandautomation",
            "windowsdirectshowdevelopment",
            "windowsgeneraldevelopmentissues",
            "tabletandtouch",
            "mediafoundationdevelopment",
            "msmq",
            "netmon",
            "windowsopticalplatform",
            "peertopeer",
            "windowssecurity",
            "sidebargadfetdevelopment",
            "windowsuidevelopment",
            "vcgeneral",
            "windbg",
            "windowsdesktopsearchdevelopment",
            "windowsdesktopsearchhelp",
            "wfp",
            "windowswic",
            "perfmon",
            "windowspro-audiodevelopment",
            "windowssdk",
            "wwsapi",
            "wsk",
            "wdk",
            "windowssensorandlocationplatform",
            "windowsribbondevelopment",
            "whck",
            "vclanguage",
            "messageanalyzer",
            "wpdevelop",
            "wptools",
            "wphowto",
            "wpnotifications",
            "wpinappads",
            "wpmango",
            "wpexpression",
            "wpappstudio",
            "appsforoffice",
            "officegeneral",
            "accessdev",
            "exceldev",
            "outlookdev",
            "worddev",
            "oxmlsdk",
            "vsto",
            "sharepointgeneral",
            "appsforsharepoint",
            "sharepointdevelopment",
            "sharepointcustomization",
            "sharepointsearch",
            "sharepointadmin",
            "sharepointgeneralprevious",
            "sharepointdevelopmentprevious",
            "sharepointcustomizationprevious",
            "sharepointsearchprevious",
            "sharepointadminprevious",
            "sharepointgenerallegacy",
            "sharepointdevelopmentlegacy",
            "sharepointcustomizationlegacy",
            "sharepointsearchlegacy",
            "sharepointadminlegacy",
            "windowsazuredevelopment",
            "windowsazuretroubleshooting",
            "windowsazuredata",
            "ssdsgetstarted",
            "windowsazureconnectivity",
            "windowsazuremanagement",
            "windowsazuresecurity",
            "windowsazurepurchasing",
            "wflmgr",
            "servbus",
            "WindowsAzureAD",
            "windowsazureactiveauthentication",
            "MediaServices",
            "azurebiztalksvcs",
            "WAVirtualMachinesVirtualNetwork",
            "WAVirtualMachinesforWindows",
            "windowsazurepack",
            "windowsazurewebsitespreview",
            "azuremobile",
            "azuregit",
            "hypervrecovmgr",
            "windowsazureonlinebackup",
            "hdinsight",
            "TFService",
            "OracleOnWindowsAzure",
            "DataMarket"};

        static void Main(string[] args)
        {
            SyndicationClient client = new SyndicationClient();
            JsonArray jsonArray = new JsonArray();

            foreach (string key in keys)
            {
                string uriString = "http://social.msdn.microsoft.com/Forums/en-US/" + key + "/threads?outputAs=rss";
                Uri uri = new Uri(uriString);
                Task<SyndicationFeed> task = client.RetrieveFeedAsync(uri).AsTask();
                task.Wait();
                SyndicationFeed feed = task.Result;
                Console.WriteLine(key);
                Console.WriteLine(feed.Title.Text);
                Console.WriteLine(feed.Subtitle.Text);
                Console.WriteLine();

                JsonObject jsonObject = new JsonObject();
                jsonObject.AddStringValue("favicon_url", "http://social.microsoft.com/Forums/GlobalResources/images/Msdn/favicon.ico");
                jsonObject.AddStringValue("icon_url", "http://kiewic.com/questions/icon/" + key);
                jsonObject.AddStringValue("audience", feed.Subtitle.Text);
                jsonObject.AddStringValue("site_url", uriString);
                jsonObject.AddStringValue("api_site_parameter", key);
                jsonObject.AddStringValue("name", feed.Title.Text);

                jsonArray.Add(jsonObject);
            }

            File.WriteAllText("msdn.json", jsonArray.Stringify());
            Console.WriteLine(jsonArray.Stringify());
        }
    }
}
