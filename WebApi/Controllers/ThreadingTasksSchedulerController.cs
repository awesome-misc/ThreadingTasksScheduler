using Microshaoft;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]

public class ThreadingTasksSchedulerController : ControllerBase
{
    private readonly ILogger<ThreadingTasksScheduler> _logger;
    private readonly ThreadingTasksScheduler _threadingTasksScheduler;

    public ThreadingTasksSchedulerController
                        (
                            ILogger<ThreadingTasksScheduler> logger
                            , ThreadingTasksScheduler threadingTasksScheduler
                        )
    {
        _logger = logger;
        _threadingTasksScheduler = threadingTasksScheduler;
    }
    
    [HttpPost]
    [Route("PostSingleTask")]
    public async Task<IActionResult> PostSingleTaskAsync
                                        (
                                            [FromBody] JsonNode parameters = null!
                                        )
    {
        Console.Clear();
        Console.Clear();
        Console.Clear();
        Console.Clear();

        // Awesome Yuer
        // Must use below code at first in the method for using the ThreadingTasksScheduler
        SynchronizationContext.SetSynchronizationContext(_threadingTasksScheduler.SynchronizationContext);

        var taskId = 0;
        async Task<JsonNode> runConsumerTaskAsync(JsonNode x)
        {
            // Consumer Task Process
            var delay = Random.Shared.Next(2 * 1000, 10 * 1000);
            await Task.Delay(delay);
            var currentThread = Thread.CurrentThread;
            _logger.LogInformation($"Complete Task {nameof(runConsumerTaskAsync)}({taskId}), \n{nameof(parameters)}=\n{parameters}:\n delay {delay} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");
            return parameters;
        };

        // Awesome Yuer
        // Invoke any Async method as Producer
        _ = runConsumerTaskAsync(parameters);

        _logger.LogInformation($"==============All Tasks Started @ {DateTime.Now}=========================");
        return
            await Task.FromResult(Ok());
    }

    [HttpPost]
    [Route("PostBatchTasks")]
    public async Task<IActionResult> PostBatchTasksAsync
                                        (
                                            [FromQuery]int iters = 200
                                            , [FromBody] JsonNode parameters = null!
                                        )
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        Console.Clear();
        Console.Clear();
        Console.Clear();
        Console.Clear();


        var tasks = new List<Task>();

        await StartRunConsumersInThreadingTasksSchedulerAsync
                (
                    () =>
                    {
                        var i = 0;
                        Func<JsonNode, int, Task> runOneTaskAsync = async (JsonNode x, int taskId) =>
                        {
                            var delay = Random.Shared.Next(2 * 1000, 10 * 1000);
                            await Task.Delay(delay);
                            var currentThread = Thread.CurrentThread;
                            _logger.LogInformation($"Complete Task {nameof(runOneTaskAsync)}({taskId}), \n{nameof(parameters)}=\n{x}:\n delay {delay} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");
                        };

                        for (; i < iters; i++)
                        {
                            var task = runOneTaskAsync(parameters, i);
                            tasks.Add(task);
                        }
                    }
                );

        Func<Task> tasksWhenAllAsync = async () =>
        {
            var taskId = -1;
            var delay = 0;

            //Task.WaitAll 阻塞当前线程，直到所有其他任务完成执行。
            //Task.WaitAll 方法调用实际上阻塞并等待所有其他任务完成。

            //Task.WhenAll 方法用于创建当且仅当所有其他任务都已完成时才会完成的任务。
            //Task.WhenAll 将得到一个不完整的任务对象。但是，它不会阻塞，而是允许程序执行。

            //Task.WaitAll(tasks.ToArray());

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();
            tasks = null;
            var currentThread = Thread.CurrentThread;
            _logger.LogInformation($"Complete Task {nameof(tasksWhenAllAsync)}({taskId}), \ndelay {delay} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");

            var endTimestamp = Stopwatch.GetTimestamp();
            var duration = (endTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"==============When All Tasks Completed @ duration {duration}ms @ {DateTime.Now}=========================");
            await Task.CompletedTask;
        };

        _ = tasksWhenAllAsync();

        _logger.LogInformation($"==============All Tasks Started @ {DateTime.Now}=========================");
        return Ok();
    }

    private async Task StartRunConsumersInThreadingTasksSchedulerAsync(Action action)
    {
        // Awesome Yuer
        // Must use below code at first in the method for using the ThreadingTasksScheduler
        SynchronizationContext.SetSynchronizationContext(_threadingTasksScheduler.SynchronizationContext);

        action();

        await Task.CompletedTask;
    }
}