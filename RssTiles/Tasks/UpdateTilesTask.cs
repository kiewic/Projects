using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Tasks
{
    public sealed class UpdateTilesTask : IBackgroundTask
    {
        public static void RegisterTask()
        {
            IBackgroundTaskRegistration updateTilesTask = BackgroundTaskRegistration.AllTasks.FirstOrDefault(
                task => task.Value.Name == "UpdateTilesTask").Value;

            if (updateTilesTask == null)
            {
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.TaskEntryPoint = "Tasks.UpdateTilesTask";
                builder.Name = "UpdateTilesTask";
                builder.SetTrigger(new MaintenanceTrigger(15, false));
                //builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
                updateTilesTask = builder.Register();
            }
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            try
            {
                string taskName = taskInstance.Task.Name;
                Logger.Append("Task started", taskName);

                FeedManager feedManager = new FeedManager();
                await feedManager.UpdateTilesAsync();

                Logger.Append("Task ended", taskName);
            }
            catch (Exception ex)
            {
                Logger.Append("Task exception", ex);
            }

            await Logger.CommitAsync();
            deferral.Complete();
        }
    }
}
