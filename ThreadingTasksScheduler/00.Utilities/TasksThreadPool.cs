using System.Collections.Concurrent;

namespace Microshaoft;
public class TasksThreadPool : TaskScheduler, IDisposable
{
    private bool _disposedValue;

    private readonly BlockingCollection<Task> _blockingCollection = new BlockingCollection<Task>();

    public int ConsumersCount { get; }

    private Thread[] _consumersThreads;

    public TasksThreadPool(int consumersCount, bool isBackground = false)
    {
        if (consumersCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(consumersCount), "Must be at least 1");
        }
        ConsumersCount = consumersCount;
        _consumersThreads = new Thread[consumersCount];
        for (int i = 0; i < consumersCount; i++)
        {
            _consumersThreads[i] = new Thread(ThreadProcess)
            {
                IsBackground = isBackground
            };
            _consumersThreads[i].Start();
        }
    }
    private void ThreadProcess()
    {
        while (true)
        {
            var task = _blockingCollection.Take();
            if (task != null)
            {
                TryExecuteTask(task);
            }
        }
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var thread in _consumersThreads)
                {
                    thread.Join();
                }
                
                // TODO: dispose managed state (managed objects)
                foreach (var task in _blockingCollection.GetConsumingEnumerable())
                {
                    task.Dispose();
                }
            }
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TaskThreadPool()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected override IEnumerable<Task>? GetScheduledTasks()
    {
        return _blockingCollection.AsEnumerable();
    }

    protected override void QueueTask(Task task)
    {
        _blockingCollection.Add(task);
    }

    //public void QueueTask(Task task)
    //{
    //    QueueTask(task);
    //}

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return !taskWasPreviouslyQueued && TryExecuteTask(task);
    }
}
