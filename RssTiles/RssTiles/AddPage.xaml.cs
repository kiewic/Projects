using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RssTiles
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        public AddPage()
        {
            this.InitializeComponent();
            UpdateTilesTask.RegisterTask();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // TODO: Why this is not called when a tile is clicked?
            LaunchManager.OnLaunched(e.Parameter as LaunchActivatedEventArgs);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SuccessBlock.Visibility = Visibility.Collapsed;
            FailBlock.Visibility = Visibility.Collapsed;

            WorkingBlock.Visibility = Visibility.Visible;

            try
            {
                FeedManager feedManager = new FeedManager();
                await feedManager.AddFeedAsync(SourceBox.Text);

                WorkingBlock.Visibility = Visibility.Collapsed;

                SuccessBlock.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                WorkingBlock.Visibility = Visibility.Collapsed;

                FailBlock.Text = ex.Message;
                FailBlock.Visibility = Visibility.Visible;
            }

        }

        private void SourceBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                AddButton_Click(AddButton, null);
            }
        }
    }
}
