using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;
using Tasks;
using Windows.ApplicationModel.Activation;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tasks
{
    public sealed class LaunchManager
    {
        public static async void OnLaunched(Windows.ApplicationModel.Activation.LaunchActivatedEventArgs e)
        {
            //if (e.Kind != ActivationKind.Launch)
            //{
            //    return;
            //}

            Logger.Append("ActivationKind", e.Kind);

            if (!String.IsNullOrEmpty(e.Arguments))
            {

                // NOTICE: This Guid does not match the Guid in the ApplicationData.
                FeedInfo feedInfo = new FeedInfo(Guid.NewGuid().ToString(), e.Arguments);
                Logger.Append("feedInfo", feedInfo);

                if (feedInfo.LinkUri != null)
                {
                    int tries = 0;

                    while (true)
                    {
                        if (tries++ >= 5)
                        {
                            break;
                        }

                        bool launched = await Launcher.LaunchUriAsync(feedInfo.LinkUri);
                        Logger.Append("LaunchUriAsync", launched);

                        if (launched)
                        {
                            break;
                        }

                        // Retry in one second.
                        await Task.Delay(1000);
                    }
                }
            }

            var notAwait2 = Logger.CommitAsync();
        }
    }
}
