using Microsoft.SharePoint.Client;

namespace ZoneProductionLibrary.Jobs
{
    public static class Scheduler
    {
        private static List<IScheduledTask> Tasks { get; } = [];

        public static void AddTask(IScheduledTask task)
        {
            Tasks.Add(task);

            Task.Run(async () =>
                     {
                         task.Start();
                     });
        }
    }
}