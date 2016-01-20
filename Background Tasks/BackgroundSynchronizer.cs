using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Pebble_Time_Manager.Connector;

namespace BackgroundTasks
{
    public sealed class BackgroundSynchronizer : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();

            System.Diagnostics.Debug.WriteLine("Start BackgroundSynchronizer");

            Pebble_Time_Manager.Connector.TimeLineSynchronizer _TimeLineSynchronizer = new TimeLineSynchronizer();
            await _TimeLineSynchronizer.Synchronize();

            System.Diagnostics.Debug.WriteLine("Start BackgroundSynchronizer");

            def.Complete();
        }
    }
}
