using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Phone.Notification.Management;
using Pebble_Time_Manager.Connector;

namespace BackgroundTasks
{
    public sealed class BackgroundNotifications : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();

            IAccessoryNotificationTriggerDetails nextTriggerDetails = AccessoryManager.GetNextTriggerDetails();
            if (nextTriggerDetails != null)
            {
                NotificationsHandler _handler = new NotificationsHandler();
                await _handler.ProcessNotifications(nextTriggerDetails);
            }
            else
            {
              //  Pebble_Time_Manager.Connector.TimeLineSynchronizer _TimeLineSynchronizer = new TimeLineSynchronizer();
                //await _TimeLineSynchronizer.Synchronize();
            }

            def.Complete();
        }
    }
}
