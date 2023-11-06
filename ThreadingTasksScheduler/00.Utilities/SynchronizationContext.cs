using System.Collections.Concurrent;

namespace Microshaoft;
public sealed class ThreadingTasksSchedulerSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<SendOrPostCallbackContext> _blockingCollection = new ();

    public override SynchronizationContext CreateCopy() => this;

    public override void Post(SendOrPostCallback callback, object? state)
    {
        _blockingCollection.Add(new SendOrPostCallbackContext(ExecutionType.Post, callback, state!, null!));
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        using (var signal = new ManualResetEventSlim())
        {
            var callbackItem = new SendOrPostCallbackContext(ExecutionType.Send, callback, state!, signal);
            _blockingCollection.Add(callbackItem);
            signal.Wait();
            if (callbackItem.Exception != null)
            {
                throw callbackItem.Exception;
            }
        }
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