namespace ZoneProductionLibrary.Jobs
{
    public interface IScheduledTask
    {
        public Func<Task> Task { get; }
        public DateTime LastCompleted { get; }
        public DateTime NextExecution { get; }

        public Task Start();
    }

    public class DailyTask : IScheduledTask
    {
        public Func<Task> Task { get; }
        public TimeSpan ExecutionTime { get; }
        public DateTime LastCompleted { get; private set; }
        public DateTime NextExecution { get; private set; }

        private bool _taskStarted = false;

        public DailyTask(Func<Task> Task, TimeSpan executionTime)
        {
            this.Task = Task;
            ExecutionTime = executionTime;
        }

        public async Task Start()
        {
            if (_taskStarted)
            {
                Log.Logger.Warning("{task} Already Started", this.Task.Method.Name);
                return;
            }

            NextExecution = DateTime.Today + ExecutionTime;
            
            if(NextExecution < DateTime.Now)
                NextExecution += TimeSpan.FromDays(1);
            
            Log.Information("New daily Task added {name} first execution {NextExecution}", Task.Method.Name, NextExecution);

            _taskStarted = true;
            
            await System.Threading.Tasks.Task.Delay(this.NextExecution - DateTime.Now);
            
            while (true)
            {
                Log.Logger.Information("Running daily task {task}", this.Task.Method.Name);
                
                this.NextExecution = DateTime.Today + TimeSpan.FromDays(1) + this.ExecutionTime;

                try
                {
                    await this.Task();
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Exception during scheduled task {task}", Task.Method.Name);
                }
                
                LastCompleted = DateTime.Now;
                
                await System.Threading.Tasks.Task.Delay(this.NextExecution - DateTime.Now);
            }
        }
    }
    
    public class TimedTask : IScheduledTask
    {
        public Func<Task> Task { get; }
        public TimeSpan ExecutionTime { get; }
        public DateTime LastCompleted { get; private set; }
        public DateTime NextExecution { get; private set; }

        private bool _taskStarted = false;

        public TimedTask(Func<Task> Task, TimeSpan executionTime)
        {
            this.Task = Task;
            ExecutionTime = executionTime;
        }

        public async Task Start()
        {
            if (_taskStarted)
            {
                Log.Logger.Warning("{task} Already Started", this.Task.Method.Name);
                return;
            }
            
            NextExecution = DateTime.Now + ExecutionTime;
            
            Log.Information("New Timed Task added {name}:{executionTime} first execution {NextExecution}", Task.Method.Name, ExecutionTime, NextExecution);

            _taskStarted = true;
            
            await System.Threading.Tasks.Task.Delay(this.NextExecution - DateTime.Now);
            
            while (true)
            {
                Log.Logger.Information("Running timed task {task} - {executionTime}", this.Task.Method.Name, ExecutionTime);
                
                this.NextExecution = DateTime.Now + this.ExecutionTime;

                try
                {
                    await this.Task();
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Exception during scheduled task {task}", Task.Method.Name);
                }
                
                LastCompleted = DateTime.Now;
                
                await System.Threading.Tasks.Task.Delay(this.NextExecution - DateTime.Now);
            }
        }
    }
}