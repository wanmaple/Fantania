using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class WorkspaceView : UserControl
{
    WorkspaceViewModel? ViewModel => DataContext as WorkspaceViewModel;

    public WorkspaceView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        topLevel.RequestAnimationFrame(OnTick);
    }

    void OnTick(TimeSpan elapsed)
    {
        if (ViewModel != null)
            ViewModel.Workspace.Tick(elapsed);
            
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}