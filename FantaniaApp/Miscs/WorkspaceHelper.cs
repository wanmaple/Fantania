using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania;

public static class WorkspaceHelper
{
    public static MainWindowViewModel GetApplicationViewModel()
    {
        return (MainWindowViewModel)AvaloniaHelper.GetTopWindow().DataContext!;
    }

    public static Workspace GetCurrentWorkspace()
    {
        return GetApplicationViewModel().Workspace!;
    }
}