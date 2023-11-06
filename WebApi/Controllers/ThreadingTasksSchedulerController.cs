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

            //Task.WaitAll ������ǰ�̣߳�ֱ�����������������ִ�С�
            //Task.WaitAll ��������ʵ�����������ȴ���������������ɡ�

            //Task.WhenAll �������ڴ������ҽ��������������������ʱ�Ż���ɵ�����
            //Task.WhenAll ���õ�һ����������������󡣵��ǣ������������������������ִ�С�

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