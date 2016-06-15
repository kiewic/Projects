using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Endpoint=sb://kiewic20151108.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxx=";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists("TestQueue"))
            {
                namespaceManager.CreateQueue("TestQueue");
            }

            QueueClient client =
                QueueClient.CreateFromConnectionString(connectionString, "TestQueue");

            var scheduleTime = DateTimeOffset.UtcNow.Add(new TimeSpan(0, 1, 0));
            Console.WriteLine(scheduleTime);

            if (args.Length == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(1000 * i);
                    client.Send(new BrokeredMessage("Test message " + i)
                    {
                        //ScheduledEnqueueTimeUtc = scheduleTime.UtcDateTime
                    });
                }
            }
            else
            {
                while (true)
                {
                    // Wait 1 hour.
                    var message = client.ReceiveAsync(new TimeSpan(1, 0, 0)).Result;

                    try
                    {
                        message.ScheduledEnqueueTimeUtc = scheduleTime.UtcDateTime;

                        // Process message from queue.
                        Console.WriteLine("Body: " + message.GetBody<string>());
                        Console.WriteLine("MessageID: " + message.MessageId);
                        Console.WriteLine("Now:             " + DateTimeOffset.UtcNow);
                        Console.WriteLine("EnqueuedTimeUtc: " + message.EnqueuedTimeUtc);

                        // Remove message from queue.
                        message.Abandon();
                    }
                    catch (Exception)
                    {
                        // Indicates a problem, unlock message in queue.
                        message.Abandon();
                    }
                }
            }
        }
    }
}
