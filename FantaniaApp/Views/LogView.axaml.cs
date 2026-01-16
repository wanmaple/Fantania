using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;

namespace Fantania.Views;

public partial class LogView : UserControl
{
    private class AutoScrollBehavior : IDisposable
    {
        public AutoScrollBehavior(ScrollViewer sv, ItemsControl ic)
        {
            _sv = sv;
            _ic = ic;
            _boundsSubscription = _ic.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(OnBoundsChanged));
        }
    
        void OnBoundsChanged(Rect bounds)
        {
            double newHeight = bounds.Height;
            double delta = newHeight - _lastHeight;
            if (Math.Abs(delta) > 0.1 && delta > 0) // 只处理高度增加
            {
                AdjustScrollPosition(delta, newHeight);
            }
            _lastHeight = newHeight;
        }
    
        private void AdjustScrollPosition(double heightChange, double newTotalHeight)
        {
            double viewportHeight = _sv.Viewport.Height;
            double scrollOffset = _sv.Offset.Y;
            // 计算到底部的偏移
            double distanceToBottom = (newTotalHeight - heightChange) - (viewportHeight + scrollOffset);
            // 如果用户在查看最新内容，自动滚动
            if (distanceToBottom <= 10)
            {
                _sv.ScrollToEnd();
            }
            else
            {
                // 用户在查看历史内容，保持到顶部的偏移不变
            }
        }
    
        public void Dispose()
        {
            _boundsSubscription?.Dispose();
        }

        IDisposable _boundsSubscription;
        ScrollViewer _sv;
        ItemsControl _ic;
        double _lastHeight = 0.0;
    }

    public LogView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (svLogs != null && icContent != null)
        {
            _behavior?.Dispose();
            _behavior = new AutoScrollBehavior(svLogs, icContent);
        }
    }

    protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _behavior?.Dispose();
    }

    AutoScrollBehavior? _behavior;
}