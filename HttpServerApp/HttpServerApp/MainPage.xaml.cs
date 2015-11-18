using HttpServerNoodles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HttpServerApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateAddressBlock();

            HttpServer server = new HttpServer();
            server.Start();
        }

        private void UpdateAddressBlock()
        {
            StringBuilder builder = new StringBuilder();

            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    Debug.WriteLine(localHostName);

                    string hostNameString = localHostName.CanonicalName;

                    if (localHostName.Type == HostNameType.Ipv6)
                    {
                        hostNameString = String.Format("[{0}]", localHostName);

                        // I think it is not easy to share an IPv6 yet. Let's skip these addresses.
                        continue;
                    }

                    builder.AppendFormat("http://{0}\r\n", hostNameString);
                }
            }

            AddressBlock.Text = builder.ToString().Trim();
        }
    }
}
