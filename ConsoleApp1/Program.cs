// See https://aka.ms/new-console-template for more information
using Microshaoft;

Console.WriteLine("Hello, World!");

_ = Scenario_using_Task_WhenAll_async();

_ = Scenario_using_Task_WaitAll_async();

_ = Scenario_Switch_SetSynchronizationContext_async();

Console.WriteLine("Started ...");
Console.ReadLine();

async Task Scenario_using_Task_WaitAll_async(int iters = 1)
{
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        Console.WriteLine($"Complete Task: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
    }

    var tasks = new List<Task>();
    using (var threadingTasksScheduler = new ThreadingTasksScheduler(4))
    { 
        SynchronizationContext.SetSynchronizationContext(threadingTasksScheduler.SynchronizationContext);
        
        for (var i = 0; i < iters; i++)
        {
            var task = RunTaskAsync(i);
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
    }
    tasks
        .ForEach
            (
                (x) =>
                {
                    x.Dispose();
                }
            );
}


async Task Scenario_using_Task_WhenAll_async(int iters = 1)
{
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        Console.WriteLine($"Complete Task: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
    }

    var tasks = new List<Task>();
    using (var threadingTasksScheduler = new ThreadingTasksScheduler(4))
    {
        SynchronizationContext.SetSynchronizationContext(threadingTasksScheduler.SynchronizationContext);
        for (var i = 0; i < iters; i++)
        {
            var task = RunTaskAsync(i);
            tasks.Add(task);
        }
        {
            var task = Task.WhenAll(tasks);
            tasks.Add(task);
            await task;
        }
    }
    tasks
        .ForEach
            (
                (x) =>
                {
                    x.Dispose();
                }
            );
}


async Task Scenario_Switch_SetSynchronizationContext_async(int iters = 1)
{
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        Console.WriteLine($"Complete Task: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
    }

    var tasks = new List<Task>();
    using (var threadingTasksScheduler = new ThreadingTasksScheduler(4))
    {
        var action = () =>
        {
            for (var i = 0; i < iters; i++)
            {
                var task = RunTaskAsync(i);
                tasks.Add(task);
            }
            {
                var task = Task.WhenAll(tasks);
                tasks.Add(task);
                task.Wait();
            }
        };

        // lastSynchronizationContext is null
        var lastSynchronizationContext = SynchronizationContext.Current;
                
        SynchronizationContext.SetSynchronizationContext(threadingTasksScheduler.SynchronizationContext);
        action();

        SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
        action();
        
    }
    tasks
        .ForEach
            (
                (x) =>
                {
                    x.Dispose();
                }
            );
}