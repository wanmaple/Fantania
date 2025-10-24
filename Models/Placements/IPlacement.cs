using System;
using System.Collections.ObjectModel;

namespace Fantania.Models;

public interface IPlacement
{
    string Name { get; }
    string IconPath { get; }
    string Tooltip { get; }
    ObservableCollection<IPlacement> Children { get; }
    Type GroupType { get; }
}