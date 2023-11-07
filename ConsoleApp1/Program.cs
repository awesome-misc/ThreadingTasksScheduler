// See https://aka.ms/new-console-template for more information
using Microshaoft;
using System.Diagnostics;

Console.WriteLine("Starting ...");
var currentThread = Thread.CurrentThread;
Console.WriteLine($"Complete Task: program @ Thread({currentThread.ManagedThreadId}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), @ {DateTime.Now: HH:mm:ss.fffff}");

Console.WriteLine("Hello, World!");

_ = Scenario_semaphore_async(32);

//_ = Scenario_using_Task_WhenAll_Throw_Exceptions_async(32);

//_ = Scenario_using_Task_WhenAll_async(32);

//_ = Scenario_using_Task_WaitAll_async();

//_ = Scenario_Switch_SetSynchronizationContext_async();

Console.WriteLine("Started ...");
currentThread = Thread.CurrentThread;
Console.WriteLine($"Complete Task: program @ Thread({currentThread.ManagedThreadId}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), @ {DateTime.Now: HH:mm:ss.fffff}");

Console.ReadLine();
Console.WriteLine("exited!");

async Task Scenario_semaphore_async(int iters = 1)
{
    var startTimestamp = Stopwatch.GetTimestamp();
    var semaphoreSlim = new SemaphoreSlim(4, 4);
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        await semaphoreSlim.WaitAsync();

        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        delay = 5000;
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        Console.WriteLine($"Complete Task: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
        
        semaphoreSlim.Release();
    }

    var tasks = new List<Task>();
    for (var i = 0; i < iters; i++)
    {
        var task = RunTaskAsync(i);
        tasks.Add(task);
    }

    await Task.WhenAll(tasks);

    tasks.Clear();
    tasks = null;

    var endTimestamp = Stopwatch.GetTimestamp();
    var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    var currentThread = Thread.CurrentThread;
    Console.WriteLine($"Complete Scenario: {nameof(Scenario_semaphore_async)} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), duration: {duration} @ {DateTime.Now: HH:mm:ss.fffff}");
}


async Task Scenario_using_Task_WaitAll_async(int iters = 1)
{
    var startTimestamp = Stopwatch.GetTimestamp();
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
    
    tasks.Clear();
    tasks = null;

    var endTimestamp = Stopwatch.GetTimestamp();
    var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    var currentThread = Thread.CurrentThread;
    Console.WriteLine($"Complete Scenario: {nameof(Scenario_using_Task_WaitAll_async)} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), duration: {duration} @ {DateTime.Now: HH:mm:ss.fffff}");
}


async Task Scenario_using_Task_WhenAll_Throw_Exceptions_async(int iters = 1)
{
    var startTimestamp = Stopwatch.GetTimestamp();
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        //Console.WriteLine($"throw new Exception: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
        throw new Exception($"Awesome Fake Exception @ {nameof(RunTaskAsync)}:{i}");
    }

    var tasks = new List<Task>();
    using (var threadingTasksScheduler = new ThreadingTasksScheduler(4))
    {
        SynchronizationContext.SetSynchronizationContext(threadingTasksScheduler.SynchronizationContext);
        for (var i = 0; i < iters; i++)
        {
            //Task task = 

            //try
            //{
            //    task = RunTaskAsync(i);
            //}
            //catch (Exception e)
            //{
            //    var currentThread1 = Thread.CurrentThread;
            //    Console.WriteLine($"Caught Exception: {e} @ Thread({currentThread1.Name}, {nameof(currentThread1.IsThreadPoolThread)}={currentThread1.IsThreadPoolThread}), @ {DateTime.Now: HH:mm:ss.fffff}");
            //}
            var taskId = i;
            var task =
                    RunTaskAsync(taskId)
                            .ContinueWith
                                (
                                    (t) =>
                                    {
                                        var e = t.Exception;
                                        if (e is not null)
                                        {
                                            if (e is AggregateException)
                                            {
                                                e = e.Flatten();
                                            }
                                            var currentThread1 = Thread.CurrentThread;
                                            Console.WriteLine($"Caught Exception: {string.Empty} @ Task {taskId}, @ Thread({currentThread1.Name}, {nameof(currentThread1.IsThreadPoolThread)}={currentThread1.IsThreadPoolThread}), @ {DateTime.Now: HH:mm:ss.fffff}");
                                        }
                                    }
                                    //, threadingTasksScheduler
                                );
            tasks.Add(task);
        }
        {
            using var task = Task.WhenAll(tasks);
            // can't use
            //await task;
            task.Wait();
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

    tasks.Clear();
    tasks = null;

    var endTimestamp = Stopwatch.GetTimestamp();
    var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    var currentThread = Thread.CurrentThread;
    Console.WriteLine($"Complete Scenario: {nameof(Scenario_using_Task_WhenAll_Throw_Exceptions_async)} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), duration: {duration} @ {DateTime.Now: HH:mm:ss.fffff}");
}

async Task Scenario_using_Task_WhenAll_async(int iters = 1)
{
    var startTimestamp = Stopwatch.GetTimestamp();
    await Task.CompletedTask;
    async Task RunTaskAsync(int i)
    {
        var data = $"{nameof(RunTaskAsync)}:{i}";
        var delay = Random.Shared.Next(100, 2000);
        delay = 5000;
        await Task.Delay(delay);
        var currentThread = Thread.CurrentThread;
        Console.WriteLine($"Complete Task: {data} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), delay: {delay} @ {DateTime.Now: HH:mm:ss.fffff}");
    }

    var tasks = new List<Task>();
    using (var threadingTasksScheduler = new ThreadingTasksScheduler(1))
    {
        SynchronizationContext.SetSynchronizationContext(threadingTasksScheduler.SynchronizationContext);
        for (var i = 0; i < iters; i++)
        {
            var task = RunTaskAsync(i);
            tasks.Add(task);
        }
        {
            //var task = Task.WhenAll(tasks);
            using var task = Task.WhenAll(tasks);
            //await task;
            task.Wait();
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

    tasks.Clear();
    tasks = null;

    var endTimestamp = Stopwatch.GetTimestamp();
    var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    var currentThread = Thread.CurrentThread;
    Console.WriteLine($"Complete Scenario: {nameof(Scenario_using_Task_WhenAll_async)} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), duration: {duration} @ {DateTime.Now: HH:mm:ss.fffff}");
}


async Task Scenario_Switch_SetSynchronizationContext_async(int iters = 1)
{
    var startTimestamp = Stopwatch.GetTimestamp();
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

    tasks.Clear();
    tasks = null;
    
    var endTimestamp = Stopwatch.GetTimestamp();
    var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    var currentThread = Thread.CurrentThread;
    Console.WriteLine($"Complete Scenario: {nameof(Scenario_Switch_SetSynchronizationContext_async)} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}), duration: {duration} @ {DateTime.Now: HH:mm:ss.fffff}");
}