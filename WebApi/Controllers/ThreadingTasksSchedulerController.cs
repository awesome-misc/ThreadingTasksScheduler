using Microshaoft;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<IActionResult> GetAsync(int iters = 200)
    {
        // Awesome Yuer
        // should use below code in the method for using the ThreadingTasksScheduler
        SynchronizationContext.SetSynchronizationContext(_threadingTasksScheduler.SynchronizationContext);

        Func<Task> runFirstOneTaskAsync = async () => 
        {
            var delay = 10000;
            await Task.Delay(delay);
            var currentThread = Thread.CurrentThread;
            _logger.LogInformation($"Complete Task {nameof(runFirstOneTaskAsync)} delay {delay} @ {currentThread.ManagedThreadId}({nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");
        };

        // Awesome Yuer
        // Single Async Task Test
        _ = runFirstOneTaskAsync();

        // Awesome Yuer
        // Multiple Async Batch Tasks Test
        await StartRunBatchTasks(iters);
        _logger.LogInformation($"=============Tasks Started @ {DateTime.Now}=========================");

        return Ok();
    }

    private async Task StartRunBatchTasks(int iters = 200)
    {
        Console.Clear();
        Console.Clear();
        Console.Clear();
        Console.Clear();

        var funcAsync = async (int x) => { return await RunOneTaskAsync(x); };

        for (var i = 0; i < iters; i++)
        {
            _ = funcAsync(i);
        }

        async Task<string> RunOneTaskAsync(int i)
        {
            var delay = Random.Shared.Next(2000, 10000);
            await Task.Delay(delay);
            var data = $"{nameof(RunOneTaskAsync)}";
            var currentThread = Thread.CurrentThread;
            _logger.LogInformation($"Complete Task {i}: {data} delay {delay} @ {currentThread.ManagedThreadId}({nameof(currentThread.IsThreadPoolThread)}={currentThread.IsThreadPoolThread}) @ {DateTime.Now: HH:mm:ss.fffff}");
            return data;
        }

        await Task.CompletedTask;
    }
}