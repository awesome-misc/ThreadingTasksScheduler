namespace Microshaoft;

public class ThreadingTasksScheduler : IDisposable
{
    private readonly Thread[] _consumersThreads;

    public SynchronizationContext SynchronizationContext => _disposed ? throw new ObjectDisposedException(typeof(ThreadingTasksScheduler).FullName) : _synchronizationContext;
    
    private ThreadingTasksSchedulerSynchronizationContext _synchronizationContext;
    
    private bool _disposed;
    
    private readonly object _locker = new object();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ThreadingTasksScheduler
                (
                    int consumersThreadsCount = 1
                    , bool isBackground = false
                )
    {
        if (consumersThreadsCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(consumersThreadsCount));
        }

        _synchronizationContext = new ThreadingTasksSchedulerSynchronizationContext();
        
        _consumersThreads = new Thread[consumersThreadsCount];

        for (int i = 0; i < _consumersThreads.Length; i++)
        {
            _consumersThreads[i] = new Thread(ThreadProcess!)
            {
                IsBackground = isBackground,
                Name = $"Thread-{i + 1:000}"
            };
            _consumersThreads[i].Start(i);
        }
    }

    private void ThreadProcess(object state)
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            SendOrPostCallbackContext callbackContext;
            try
            {
                callbackContext = _synchronizationContext.Receive(_cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                return;
            }
            callbackContext?.Execute();
        }
        //Console.WriteLine($"Thread {i} is stopped.");
    }

    public void Dispose()
    {
        lock (_locker)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }
        
        _cancellationTokenSource.Cancel();

        foreach (var thread in _consumersThreads)
        {
            thread.Join();
        }

        _synchronizationContext.Dispose();
    }
}