using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tasks
{
    class Logger
    {
        private static StringBuilder builder = new StringBuilder();
        private static object builderLock = new Object();

        public static StringBuilder Builder
        {
            get
            {
                return builder;
            }
        }

        internal static void Append(string action, object id)
        {
            lock (builderLock)
            {
                builder.AppendFormat("{0}: {1}\r\n", action, id);
            }
        }

        internal static async Task CommitAsync()
        {
            try
            {
                DateTime now = DateTime.Now;
                string fileName = String.Format(
                    "{0:D4}_{1:D2}_{2:D2}-{3:D2}_{4:D2}_{5:D2}.log",
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour,
                    now.Minute,
                    now.Second);

                IStorageFile logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    fileName,
                    CreationCollisionOption.GenerateUniqueName);

                lock (builderLock)
                {
                    // NOTE: This async call is not async because it is inside a lock.
                    Task writeTask = FileIO.WriteTextAsync(logFile, builder.ToString()).AsTask();
                    writeTask.Wait();

                    builder.Clear();
                }
            }
            catch (Exception ex)
            {
                // We don't want the logger to cause any exception.
                Debug.WriteLine(ex);
            }
        }
    }
}
