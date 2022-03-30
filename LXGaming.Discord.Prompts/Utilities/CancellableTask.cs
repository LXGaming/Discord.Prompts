namespace LXGaming.Discord.Prompts.Utilities;

public class CancellableTask : IAsyncDisposable {

    public PromptBase Prompt { get; }
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public bool Stopped { get; private set; }

    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _task;
    private bool _disposed;

    public CancellableTask(PromptBase prompt) {
        Prompt = prompt;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public Task StartAsync(Func<Task> function) {
        if (_disposed) {
            throw new ObjectDisposedException(GetType().FullName);
        }

        if (_task != null) {
            throw new InvalidOperationException("Task already started");
        }

        return _task = function();
    }

    public Task StopAsync() {
        if (_disposed) {
            throw new ObjectDisposedException(GetType().FullName);
        }

        Stopped = true;
        _cancellationTokenSource.Cancel();
        return _task ?? Task.CompletedTask;
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing) {
        if (_disposed) {
            return;
        }

        if (disposing) {
            try {
                _cancellationTokenSource.Cancel();
                if (_task != null) {
                    await _task;
                }
            } catch (Exception) {
                // no-op
            }

            _cancellationTokenSource.Dispose();
        }

        _disposed = true;
    }
}