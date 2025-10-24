using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class DatabaseObjectWindow : Window
{
    public DatabaseObjectWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
    }

    public async Task AddObject()
    {
        var obj = (DataContext as DatabaseObjectViewModel).CurrentObject;
        if (properties.ViewModel.EditableProperties.Any(prop => !string.IsNullOrEmpty(prop.Error)))
        {
            return;
        }
        txtErr.Text = string.Empty;
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.AddObject(obj);
        this.Close();
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var prop in properties.ViewModel.EditableProperties)
            {
                if (!string.IsNullOrEmpty(prop.Error))
                {
                    txtErr.Text = prop.Error;
                    return;
                }
            }
            txtErr.Text = string.Empty;
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}