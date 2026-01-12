namespace FantaniaLib;

public class RenderSync : IDisposable
{
    private class RenderContextReleaser : IDisposable
    {
        public RenderContextReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }

        readonly SemaphoreSlim _semaphore;
    }

    public void MarkFrameCollected()
    {
        Interlocked.Increment(ref _currentFrameId);
        _semFrameCollected.Set();
    }

    public bool WaitForNewFrame(int timeoutMs)
    {
        return _semFrameCollected.Wait(timeoutMs);
    }

    public void MarkFrameRendered()
    {
        _semFrameRendered.Set();
        _semFrameCollected.Reset();
    }
    
    public bool WaitForFrameRendered(int timeoutMs)
    {
        return _semFrameRendered.Wait(timeoutMs);
    }

    public void ResetFrameRendered()
    {
        _semFrameRendered.Reset();
    }
    
    public IDisposable AcquireGLContext()
    {
        _semRenderContext.Wait();
        return new RenderContextReleaser(_semRenderContext);
    }

    public void Dispose()
    {
        _semFrameCollected.Dispose();
        _semFrameRendered.Dispose();
        _semRenderContext.Dispose();
    }

    readonly ManualResetEventSlim _semFrameCollected = new ManualResetEventSlim(false); // UI线程收集完成
    readonly ManualResetEventSlim _semFrameRendered = new ManualResetEventSlim(false);  // 工作线程渲染完成
    readonly SemaphoreSlim _semRenderContext = new SemaphoreSlim(1, 1); // 渲染上下文信号量

    long _currentFrameId = 0L;
}