using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinrtComponentUsingWRL;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CsApp
{
    enum Errors : uint
    {
        E_ACCESSDENIED = 0x80070005,
        E_CHANGED_STATE = 0x8000000C,
        REGDB_E_CLASSNOTREG = 0x80040154,
        RPC_E_DISCONNECTED = 0x80010108,
        E_FAIL = 0x80004005,
        E_INVALIDARG = 0x80070057,
        E_NOINTERFACE = 0x80004002,
        E_NOTIMPL = 0x80004001,
        E_POINTER = 0x80004003,
        RO_E_CLOSED = 0x80000013,
        E_ABORT = 0x80004004,
        E_BOUNDS = 0x8000000B,
        E_OUTOFMEMORY = 0x8007000E,
        RPC_E_WRONG_THREAD = 0x8001010E
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorsPage : Page
    {
        public ErrorsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void KnownErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            TestException(Errors.E_ACCESSDENIED);
            TestException(Errors.E_CHANGED_STATE);
            TestException(Errors.REGDB_E_CLASSNOTREG);
            TestException(Errors.RPC_E_DISCONNECTED);
            TestException(Errors.E_FAIL);
            TestException(Errors.E_INVALIDARG);
            TestException(Errors.E_NOINTERFACE);
            TestException(Errors.E_NOTIMPL);
            TestException(Errors.E_POINTER);
            TestException(Errors.RO_E_CLOSED);
            TestException(Errors.E_ABORT);
            TestException(Errors.E_BOUNDS);
            TestException(Errors.E_OUTOFMEMORY);
            TestException(Errors.RPC_E_WRONG_THREAD);
        }

        private void TestException(Errors value)
        {
            Foo foo = new Foo();

            try
            {
                foo.DoBar((uint)value);
            }
            catch (Exception ex)
            {
                string outputString = String.Format("0x{0:X}, {1}, {2}\r\n", (uint)value, value, ex.GetType());
                Output.Text += outputString;
            }
        }

        private async void RangeOfErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            await TestRange(0x80000000, 0xFFFF);
            await TestRange(0x80070000, 0xFFFF);
            for (uint i = 0x8; i <= 0xFFF; i++)
            {
                uint start = (0x8000 + i) * 0x10000;
                Debug.WriteLine("Testing 0x{0:X}", start);
                await TestRange(start, 0x50);
            }
        }

        private Task TestRange(uint start, uint count)
        {
            return Task.Run(() =>
            {
                Foo foo = new Foo();

                uint end = start + count;
                for (uint value = start; value < end; value++)
                {
                    try
                    {
                        foo.DoBar((uint)value);
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() != typeof(System.Exception))
                        {
                            var asyncInfo = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                Output.Text += String.Format("0x{0:X}, {1}\r\n", value, ex.GetType());
                            });
                        }
                    }

                    // Add a stupid delay.
                    if (value % 0x80 == 0)
                    {
                        var delayTask = Task.Delay(500);
                        delayTask.Wait();
                    }
                }
            });
        }


    }
}
