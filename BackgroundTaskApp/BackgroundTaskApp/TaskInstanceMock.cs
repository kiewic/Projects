using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;

namespace BackgroundTaskApp
{
    class TaskInstanceMock : IBackgroundTaskInstance
    {
        public event BackgroundTaskCanceledEventHandler Canceled;

        public BackgroundTaskDeferral GetDeferral()
        {
            throw new NotImplementedException();
        }

        public Guid InstanceId
        {
            get
            {
                return Guid.Parse("3F2504E0-4F89-41D3-9A0C-0305E82C3301");
            }
        }

        public uint Progress
        {
            get;
            set;
        }

        public uint SuspendedCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BackgroundTaskRegistration Task
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object TriggerDetails
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
