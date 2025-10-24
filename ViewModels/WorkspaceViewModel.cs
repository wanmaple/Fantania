using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fantania.Models;

namespace Fantania.ViewModels
{
    public partial class WorkspaceViewModel : ViewModelBase
    {
        public static WorkspaceViewModel Current { get; set; }

        public Workspace? Workspace => _workspace;

        public ObservableCollection<IPlacement> Placements => _placements;
        public ObservableCollection<LogContent> Logs => _logs;
        public ulong FrameCount { get; set; } = 0u;

        private ObservableCollection<ObservableObject> _selectedObjs = new ObservableCollection<ObservableObject>();
        public IReadOnlyList<ObservableObject> SelectedObjects => _selectedObjs;

        public WorkspaceViewModel(Workspace workspace)
        {
            _workspace = workspace;
            _logs = new ObservableCollection<LogContent>();
            InitializePlacements();
        }

        public int GenerateID(string group)
        {
            var groupedObjs = Workspace.MainDatabase.ObjectsOfGroup(group);
            if (groupedObjs.Count <= 0)
                return 1;
            int maxId = groupedObjs.Max(obj => obj.ID);
            return maxId + 1;
        }

        public string GenerateClonedName(string group, string origName)
        {
            string name = origName + "_Copy";
            var groupedObjs = Workspace.MainDatabase.ObjectsOfGroup(group);
            var names = new HashSet<string>(groupedObjs.Count);
            foreach (var obj in groupedObjs)
            {
                names.Add(obj.Name);
            }
            int idx = 0;
            while (names.Contains(name))
            {
                name = $"{origName}_Copy{idx.ToString()}";
                ++idx;
            }
            return name;
        }

        public void AddSelectedObject(ObservableObject obj)
        {
            _selectedObjs.Add(obj);
            RaiseSelectedObjectsChanged();
        }

        public void RemoveSelectedObject(ObservableObject obj)
        {
            _selectedObjs.Remove(obj);
            RaiseSelectedObjectsChanged();
        }

        public void ClearSelections()
        {
            for (int i = SelectedObjects.Count - 1; i >= 0; --i)
            {
                var obj = SelectedObjects[i];
                if (obj is DatabaseObject) continue;
                (obj as LevelObject).IsSelected = false;
            }
            _selectedObjs.Clear();
            RaiseSelectedObjectsChanged();
        }

        public async Task Log(string content)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var log = new LogContent
                {
                    FontSize = 14,
                    FontStyle = FontStyle.Normal,
                    FontWeight = FontWeight.Light,
                    Color = Brushes.White,
                    Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
                };
                _logs.Add(log);
            });
        }

        public async Task LogOptional(string content)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var log = new LogContent
                {
                    FontSize = 14,
                    FontStyle = FontStyle.Italic,
                    FontWeight = FontWeight.Light,
                    Color = Brushes.LightGray,
                    Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
                };
                _logs.Add(log);
            });
        }

        public async Task LogWarning(string content)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var log = new LogContent
                {
                    FontSize = 14,
                    FontStyle = FontStyle.Italic,
                    FontWeight = FontWeight.Light,
                    Color = Brushes.Yellow,
                    Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
                };
                _logs.Add(log);
            });
        }

        public async Task LogError(string content)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var log = new LogContent
                {
                    FontSize = 14,
                    FontStyle = FontStyle.Normal,
                    FontWeight = FontWeight.Medium,
                    Color = Brushes.Red,
                    Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
                };
                _logs.Add(log);
            });
        }

        [RelayCommand]
        public async Task DuplicateTemplate()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (SelectedObjects.Count == 1 && SelectedObjects[0] is DatabaseObject dbObj)
                {
                    var dup = dbObj.Clone();
                    Workspace.AddObject(dup);
                }
            });
        }

        [RelayCommand]
        public async Task ClearLog()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _logs.Clear();
            });
        }

        void InitializePlacements()
        {
            _placements = new ObservableCollection<IPlacement>();
            var decos = new PlacementContainer(Localization.Resources.PlacementDecoration, "avares://Fantania/Assets/icons/placements/decoration.png");
            _placements.Add(decos);
            // decos.Children.Add(new PlacementGroup(Localization.Resources.PlacementUnlitSprite, "avares://Fantania/Assets/icons/placements/image.png", Localization.Resources.TooltipPlacementUnlitSprite, typeof(UnlitSpriteTemplate)));
            // decos.Children.Add(new PlacementGroup(Localization.Resources.PlacementUnlitCurvedSprite, "avares://Fantania/Assets/icons/placements/curved.png", Localization.Resources.TooltipUnlitCurvedSprite, typeof(UnlitCurvedSpriteTemplate)));
            // decos.Children.Add(new PlacementGroup(Localization.Resources.PlacementUnlitNoiseSprite, "avares://Fantania/Assets/icons/placements/noise.png", Localization.Resources.TooltipUnlitNoiseSprite, typeof(UnlitNoiseSpriteTemplate)));
        }

        void RaiseSelectedObjectsChanged()
        {
            OnPropertyChanged(nameof(SelectedObjects));
        }

        Workspace? _workspace;
        ObservableCollection<IPlacement> _placements;
        ObservableCollection<LogContent> _logs;
    }
}
