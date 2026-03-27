using System;
using System.Collections.Generic;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public class EditGameDataViewModel : ViewModelBase
{
    public IWorkspace Workspace => _workspace;
    public IReadOnlyList<string> GameDataGroups => _grouplist;

    public EditGameDataViewModel(IWorkspace workspace)
    {
        _workspace = workspace;
        _grouplist = new FilterableBindingSource<string>(_workspace.DatabaseModule.GameDataGroups);
        _grouplist.Filter(null, null);
    }

    public void ShowView()
    {
        if (_view == null)
        {
            _view = new EditGameDataView();
            _view.DataContext = this;
        }
        _view.Show();
    }

    public void FilterGroups(Predicate<string>? filter)
    {
        _grouplist.Filter(filter, null);
    }

    IWorkspace _workspace;
    EditGameDataView? _view;
    FilterableBindingSource<string> _grouplist;
}