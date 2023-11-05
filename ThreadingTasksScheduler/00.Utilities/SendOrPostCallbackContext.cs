namespace Microshaoft;


public enum ExecutionType
{
    Post
    , Send

}

public class SendOrPostCallbackContext
{
    public SendOrPostCallbackContext
                            (
                                ExecutionType executionType
                                , SendOrPostCallback callback
                                , object? state
                                , ManualResetEventSlim signal
                            )
    {
        _executionType = executionType;
        _callback = callback;
        _state = state;
        _signal = signal;
    }

    //public void Dispose()
    //{
    //    _signal.Dispose();
    //}

    private ExecutionType _executionType { get; }
    private SendOrPostCallback _callback { get; }
    private object? _state { get; }
    private ManualResetEventSlim _signal { get; }
    public Exception? Exception { get; private set; }

    public void Execute()
    {
        switch (_executionType)
        {
            case ExecutionType.Post:
                _callback(_state);
                break;
            case ExecutionType.Send:
                try
                {
                    _callback(_state);
                }
                catch (Exception e)
                {
                    Exception = e;
                }
                _signal.Set();
                break;
            default:
                throw new ArgumentException($"{nameof(_executionType)} is not a valid value.");
        }
    }
}