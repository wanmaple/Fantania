using System.Collections.Concurrent;
using System.Threading;
using Avalonia.Threading;
using Fantania.Models;

namespace Fantania;

public class BackgroundJobManager
{
    public void Initialize(Workspace workspace)
    {
        _workspace = workspace;
        Thread th = new Thread(ThreadStart);
        th.IsBackground = true;
        th.Priority = ThreadPriority.BelowNormal;
        th.Start();
    }

    public void Finalize()
    {
        _threadShouldExit = true;
        _sem.Set();
    }

    public void AddJob(IRequireBackgroundJob job)
    {
        _jobs.Add(job);
        job.JobDirty = true;
        _sem.Set();
    }

    void ThreadStart()
    {
        while (true)
        {
            if (_sem.WaitOne())
            {
                _sem.Reset();
                if (_threadShouldExit) break;
                while (_jobs.TryTake(out IRequireBackgroundJob job))
                {
                    if (job.JobDirty)
                    {
                        job.JobDirty = false;
                        try
                        {
                            job.DoBackgroundJob(_workspace);
                        }
                        catch { }
                        finally
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                job.OnJobCompleted(_workspace);
                            });
                        }
                    }
                    if (_threadShouldExit) break;
                }
                if (_threadShouldExit) break;
            }
        }
        _sem.Dispose();
    }

    Workspace _workspace;
    ConcurrentBag<IRequireBackgroundJob> _jobs = new ConcurrentBag<IRequireBackgroundJob>();
    ManualResetEvent _sem = new ManualResetEvent(false);
    volatile bool _threadShouldExit = false;
}