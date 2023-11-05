// See https://aka.ms/new-console-template for more information
using Microshaoft;
using System.Collections.Concurrent;
using System.Text;

Console.WriteLine("Hello, World!");

var scheduler = new ThreadingTasksScheduler(4);
SynchronizationContext.SetSynchronizationContext(scheduler.SynchronizationContext);

StringBuilder result1 = null!;

ConcurrentBag<string> result2 = null!;


var input = string.Empty;

var information = "Press any key to test, Press 'q' to quit the sample.";

Console.WriteLine(information);

while ((input= Console.ReadLine()) != "q")
{
    result1 = new StringBuilder(5000);

    result2 = new ConcurrentBag<string>();

    RunOnce();

    Thread.Sleep(2000);

    Console.WriteLine($"Result SB:\n{result1}");
    Console.WriteLine($"Result {nameof(ConcurrentBag<string>)}:");
    foreach (var item in result2)
    {
        Console.WriteLine(item);
    }
    Console.WriteLine(information);
}

void RunOnce()
{
    for (var i = 0; i < 100; i++)
    {
        var ii = i;
        _ = Task
                .Run
                    (
                        () =>
                        {
                            var data = $"{nameof(Task.Run)} data - {ii}";
                            result1.AppendLine(data);
                            result2.Add(data);
                            var delay = Random.Shared.Next(100, 2000);
                            Task.Delay(delay);
                            //Console.WriteLine($"Complete: {data} delay {delay} @ {Thread.CurrentThread.ManagedThreadId}");
                        }
                    );
    }
    _ = WriteTestAsync();

}




scheduler.Dispose();

async Task WriteTestAsync()
{
    var t1 = Write1Async();
    //t1 = Write4();
    var t2 = Write2Async();
    var data = nameof(WriteTestAsync);
    result1.AppendLine($"{nameof(data)}: {data}");
    result2.Add($"{nameof(data)}: {data}");
    await Task.WhenAll(t1, t2);
    await Write3Async();
}

async Task<string> Write4Async()
{
    // Yield the thread (runs the remainder of the async code as a callback on the SchedulingSyncContext thread)
    //await Task.Yield();
    //await Task.Delay(10);
    var data = nameof(Write4Async);
    result1!.AppendLine($"{nameof(data)}: {data}");
    result2!.Add($"{nameof(data)}: {data}");
    //Console.WriteLine($"Complete: {data}");
    return await Task.FromResult(data);
}

async Task Write1Async()
{
    // Yield the thread (runs the remainder of the async code as a callback on the SchedulingSyncContext thread)
    // task = null
    await Task.Yield();
    var data = nameof(Write1Async);
    result1.AppendLine($"{nameof(data)}: {data}");
    result2!.Add($"{nameof(data)}: {data}");
    //Console.WriteLine($"Complete: {data}");
}
async Task Write2Async()
{
    // Wait for 10 milliseconds before continuing (uses callbacks)
    await Task.Delay(1000);
    var data = nameof(Write2Async);
    result1.AppendLine($"{nameof(data)}: {data}");
    result2!.Add($"{nameof(data)}: {data}");
    //Console.WriteLine($"Complete: {data}");
}
async Task Write3Async()
{
    var data = nameof(Write3Async);
    // Run these two tasks on the thread pool (could come out in any order)
    var tA = Task
                .Run
                    (
                        () =>
                        {
                            result1.AppendLine($"{nameof(data)}: {data} - 1");
                            result2.Add($"{nameof(data)}: {data} - 1");
                        }
                    );
    var tB = Task
                .Run
                    (
                        () =>
                        {
                            result1.AppendLine($"{nameof(data)}: {data} - 2");
                            result2.Add($"{nameof(data)}: {data} - 2");
                        }
                    );
    // Wait for them to finish before continuing (uses callbacks)
    await Task.WhenAll(tA, tB);
    //Console.WriteLine($"Complete: {data}");
}
Console.ReadLine();