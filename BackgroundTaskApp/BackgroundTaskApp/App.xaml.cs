using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace BackgroundTaskApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        IBackgroundTaskRegistration SongProcessingTask2;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            SetupBackgroundTask();
        }

        private void HandlerWithLambaExample()
        {
            Windows.Networking.Sockets.DatagramSocket socket = new Windows.Networking.Sockets.DatagramSocket();
            socket.MessageReceived += new TypedEventHandler<
                Windows.Networking.Sockets.DatagramSocket,
                Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs>((datagramSocket, eventArgs) =>
            {
                // Empty handler.
            });
        }

        private void SetupBackgroundTask()
        {
            SongProcessingTask2 = BackgroundTaskRegistration.AllTasks.FirstOrDefault(
                task => task.Value.Name == "TheSongProcessingTask2").Value;

            if (this.SongProcessingTask2 == null)
            {
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.TaskEntryPoint = "Tasks.SongProcessingTask2";
                builder.Name = "TheSongProcessingTask2";
                builder.SetTrigger(new MaintenanceTrigger(15, false));
                //builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
                this.SongProcessingTask2 = builder.Register();
            }

            this.SongProcessingTask2.Completed += new BackgroundTaskCompletedEventHandler(OnBackgroundTaskCompleted);
            this.SongProcessingTask2.Progress += new BackgroundTaskProgressEventHandler(OnBackgroundTaskProgress);
        }

        private void OnBackgroundTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            args.CheckResult(); // TODO: What kind of errors does this report?
            Debug.WriteLine(sender); // BackgroundTaskRegistration
            string instanceIdString = args.InstanceId.ToString();

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(instanceIdString))
            {
                // This task didn't schedule a download.
                return;
            }

            Guid transferGuid = (Guid)ApplicationData.Current.LocalSettings.Values[instanceIdString];
            Debug.WriteLine("Background task completed! Last download was {0}", transferGuid);
        }

        private void OnBackgroundTaskProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Debug.WriteLine("Sender si {0}", sender);
            Debug.WriteLine("Progress is {0}", args.Progress);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            else
            {
                MainPage page = rootFrame.Content as MainPage;
                page.SetRedditValuesFromToast(e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
