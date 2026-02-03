using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public partial class ToolbarView : UserControl
{
    LevelViewModel ViewModel => (LevelViewModel)DataContext!;

    public ToolbarView()
    {
        InitializeComponent();
        Focusable = true;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitializeShortcutKeys();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        FinalizeShortcutKeys();
    }

    void InitializeShortcutKeys()
    {
        var window = AvaloniaHelper.GetTopWindow();
        var placeDefs = ViewModel.PlacementModesDefinitions;
        for (int i = 0; i < placeDefs.Count; i++)
        {
            var binding = new KeyBinding
            {
                Command = placeDefs[i].Command!,
                CommandParameter = placeDefs[i].Value,
                Gesture = new KeyGesture(Key.D1 + i),
            };
            window.KeyBindings.Add(binding);
            _keyBindings.Add(binding);
        }
        var transDefs = ViewModel.TransformModesDefinitions;
        Key[] transKeys = [Key.Q, Key.W, Key.E, Key.R,];
        for (int i = 0; i < transDefs.Count; i++)
        {
            var binding = new KeyBinding
            {
                Command = transDefs[i].Command!,
                CommandParameter = transDefs[i].Value,
                Gesture = new KeyGesture(transKeys[i]),
            };
            window.KeyBindings.Add(binding);
            _keyBindings.Add(binding);
        }
    }

    void FinalizeShortcutKeys()
    {
        var window = AvaloniaHelper.GetTopWindow();
        foreach (var binding in _keyBindings)
        {
            window.KeyBindings.Remove(binding);
        }
    }

    List<KeyBinding> _keyBindings = new List<KeyBinding>(0);
}