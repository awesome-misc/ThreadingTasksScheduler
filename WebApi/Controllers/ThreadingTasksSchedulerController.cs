using Microshaoft;
using Microsoft.AspNetCore.Mvc;
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
        // Awesome Yuer
        // Must use below code at first in the method for using the ThreadingTasksScheduler
        SynchronizationContext.SetSynchronizationContext(_threadingTasksScheduler.SynchronizationContext);

        var taskId = 0;
        async Task<JsonNode> runConsumerTaskAsync(JsonNode x)
        {
            var delay = Random.Shared.Next(2 * 1000, 10 * 1000);
            await Task.Delay(delay);
            var currentThread = Thread.CurrentThread;
            _logger.LogInformation($"Complete Task {nameof(runConsumerTaskAsync)}({taskId}), \n{nameof(parameters)}=\n{parameters}:\n delay {delay} @ Thread({currentThread.Name}, {nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");
            return parameters;
        };

        // Awesome Yuer
        // Invoke any Async method
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
                            _ = runOneTaskAsync(parameters, i);
                        }
                    }
                );
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