using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;

namespace Fantania;

public class ScrollToEndBehavior : Behavior<ItemsControl>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is { })
        {
            AssociatedObject.SizeChanged += OnAssociatedObjectSizeChanged;
            AssociatedObject.TemplateApplied += OnAssociatedObjectTemplateApplied;
        }
    }
    
    private void OnAssociatedObjectSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_shouldScrollToEnd && AssociatedObject?.Items.Count > 0)
        {
            _scrolling = true;
            AssociatedObject.ScrollIntoView(AssociatedObject.Items.Count - 1);
            _scrolling = false;
        }
    }

    void OnAssociatedObjectTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
            // scroll into view when loaded
        AssociatedObject.ScrollIntoView(AssociatedObject.Items.Count - 1);
    }

    bool _shouldScrollToEnd = true;
    bool _scrolling;
}