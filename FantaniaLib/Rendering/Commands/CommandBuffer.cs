using System.Numerics;

namespace FantaniaLib;

public class CommandBuffer : IDisposable
{
    /// <summary>
    /// UI线程，添加命令到Back Buffer。
    /// </summary>
    public void AddCommand(IRenderCommand cmd)
    {
        if (_disposed) return;
        lock (_mutexSwap)
        {
            _backBuffer.Add(cmd);
            _semNewCmd.Set();
        }
    }

    /// <summary>
    /// 渲染线程，获取命令列表。
    /// </summary>
    public IReadOnlyList<IRenderCommand>? WaitForCommands(int timeoutMs = -1)
    {
        if (_disposed) return null;
        if (!_semNewCmd.Wait(timeoutMs)) return null;
        lock (_mutexSwap)
        {
            var temp = _frontBuffer;
            _frontBuffer = _backBuffer;
            _backBuffer = temp;
            _backBuffer.Clear();
            _semNewCmd.Reset();
            return [.. _frontBuffer];
        }
    }

    public void Clear()
    {
        lock (_mutexSwap)
        {
            _frontBuffer.Clear();
            _backBuffer.Clear();
            _semNewCmd.Reset();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _semNewCmd.Set();
        _semNewCmd.Dispose();
        lock (_mutexSwap)
        {
            _frontBuffer.Clear();
            _backBuffer.Clear();
        }
    }

    List<IRenderCommand> _frontBuffer = new List<IRenderCommand>();
    List<IRenderCommand> _backBuffer = new List<IRenderCommand>();

    readonly object _mutexSwap = new object();
    readonly ManualResetEventSlim _semNewCmd = new ManualResetEventSlim(false);
    volatile bool _disposed = false;
}