using System.Collections.Concurrent;

namespace Microshaoft;
public sealed class ThreadingTasksSchedulerSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<SendOrPostCallbackContext> _blockingCollection = new ();

    public override SynchronizationContext CreateCopy() => this;

    public override void Post(SendOrPostCallback callback, object? state)
    {
        Console.WriteLine($"begin: {nameof(Post)} @ {DateTime.Now:HH:mm:ss.ffffff}");
        _blockingCollection.Add(new SendOrPostCallbackContext(ExecutionType.Post, callback, state!, null!));
        Console.WriteLine($"end: {nameof(Post)} @ {DateTime.Now:HH:mm:ss.ffffff}");
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        Console.WriteLine($"begin: {nameof(Send)} @ {DateTime.Now:HH:mm:ss.ffffff}");
        using (var signal = new ManualResetEventSlim())
        {
            var sendOrPostCallbackContext = new SendOrPostCallbackContext(ExecutionType.Send, callback, state!, signal);
            _blockingCollection.Add(sendOrPostCallbackContext);
            signal.Wait();
            if (sendOrPostCallbackContext.Exception != null)
            {
                throw sendOrPostCallbackContext.Exception;
            }
        }
        Console.WriteLine($"end: {nameof(Send)} @ {DateTime.Now:HH:mm:ss.ffffff}");
    }

    public SendOrPostCallbackContext Receive(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(Receive)}");
        var context = _blockingCollection.Take(cancellationToken);
        if (context == null)
        {
            throw new ThreadInterruptedException("context was unblocked.");
        }
        return context;
    }

    //public void Unblock() => _blockingCollection.Add(null!);

    //public void Unblock(int count)
    //{
    //    for (; count > 0; count--)
    //    {
    //        _blockingCollection.Add(null!);
    //    }
    //}

    public void Dispose() => _blockingCollection.Dispose();
}