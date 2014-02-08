using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheConsoleApp
{
    class Program
    {
        private static AutoResetEvent autoResetEvent;

        static void Main(string[] args)
        {
            autoResetEvent = new AutoResetEvent(false);
            Foo();
            autoResetEvent.WaitOne();
        }

        private static async void Foo()
        {
            Uri uri = new Uri("http://localhost/?cache=1");

            System.Net.Http.WebRequestHandler handler =
                new System.Net.Http.WebRequestHandler();

            // Cache options:
            //     System.Net.Cache.RequestCacheLevel.BypassCache
            //     System.Net.Cache.RequestCacheLevel.CacheIfAvailable
            handler.CachePolicy = new System.Net.Cache.RequestCachePolicy(
                System.Net.Cache.RequestCacheLevel.CacheIfAvailable);

            System.Net.Http.HttpClient client2 =
                new System.Net.Http.HttpClient(handler);

            System.Net.Http.HttpResponseMessage response2 = await client2.GetAsync(uri);
            response2.EnsureSuccessStatusCode();
            string str = await response2.Content.ReadAsStringAsync();
            Console.WriteLine(str);

            System.Threading.Thread.Sleep(1111);

            response2 = await client2.GetAsync(uri);
            response2.EnsureSuccessStatusCode();
            str = await response2.Content.ReadAsStringAsync();
            Console.WriteLine(str);

            autoResetEvent.Set();
        }
    }
}
