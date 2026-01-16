using System.Threading.Tasks;
using Avalonia.Controls;
using Fantania.Localization;
using FantaniaLib;

namespace Fantania.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClosing)
        {
            e.Cancel = true;
            bool close = true;
            ViewModels.MainWindowViewModel vm = (ViewModels.MainWindowViewModel)DataContext!;
            Workspace? workspace = vm.Workspace;
            if (workspace != null)
            {
                if (workspace.IsModified)
                {
                    await MessageBoxHelper.PopupWarningYesNoCancel(this, LocalizationHelper.GetLocalizedString("WARN_ConfirmSaveWorkspaceExit")).ContinueWith(async task =>
                    {
                        if (task.Result == ButtonResults.Yes)
                        {
                            await workspace.Save();
                        }
                        else if (task.Result == ButtonResults.Cancel)
                        {
                            close = false;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            if (close)
            {
                _forceClosing = true;
                Close();
            }
        }
        base.OnClosing(e);
    }

    bool _forceClosing = false;
}