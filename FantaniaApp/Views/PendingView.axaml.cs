using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace Fantania.Views;

public partial class PendingView : Window
{
    public PendingView()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        StartSpinner();
    }

    protected override void OnClosed(EventArgs e)
    {
        StopSpinner();
        base.OnClosed(e);
    }

    void StartSpinner()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (s, ev) =>
        {
            _angle += 6; if (_angle >= 360) _angle -= 360;
            if (spinner.RenderTransform is RotateTransform rt)
                rt.Angle = _angle;
        });
        _timer.Start();
    }

    void StopSpinner()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
    }

    public static Task ShowForAsync(Window owner, Task task)
    {
        var dlg = new PendingView();
        task.ContinueWith(t => Dispatcher.UIThread.Post(() => { try { dlg.Close(); } catch { } }), TaskScheduler.Default);
        return ShowAndPropagateAsync(dlg, owner, task);
    }

    static async Task ShowAndPropagateAsync(PendingView dlg, Window owner, Task task)
    {
        await dlg.ShowDialog(owner);
        if (task.IsFaulted)
            throw task.Exception!;
    }
    
    DispatcherTimer? _timer;
    double _angle;
}