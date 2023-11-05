namespace Microshaoft;

public class ThreadingTasksScheduler : IDisposable
{
    private readonly Thread[] _consumersThreads;

    public SynchronizationContext SynchronizationContext => _disposed ? throw new ObjectDisposedException(typeof(ThreadingTasksScheduler).FullName) : _synchronizationContext;
    
    private ThreadingTasksSchedulerSynchronizationContext _synchronizationContext;
    
    private bool _disposed;
    
    private readonly object _locker = new object();

    private readonly bool[] _consumersThreadsIsStopping;

    public ThreadingTasksScheduler(int consumersThreadsCount = 1, bool isBackground = false)
    {
        if (consumersThreadsCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(consumersThreadsCount));
        }

        _synchronizationContext = new ThreadingTasksSchedulerSynchronizationContext();
        
        _consumersThreads = new Thread[consumersThreadsCount];

        _consumersThreadsIsStopping = new bool[consumersThreadsCount];

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
        int i = (int) state;
        while (!_consumersThreadsIsStopping[i])
        {
            SendOrPostCallbackContext callback;
            try
            {
                callback = _synchronizationContext.Receive();
            }
            catch (Exception)
            {
                return;
            }
            callback?.Execute();
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

        for(var i = 0; i < _consumersThreadsIsStopping.Length; i ++)
        {
            //thread.Join();
            _consumersThreadsIsStopping[i] = true;
        }

        _synchronizationContext.Unblock(_consumersThreads.Length);

        //Thread.Sleep(1000);

        _synchronizationContext.Dispose();
    }
}