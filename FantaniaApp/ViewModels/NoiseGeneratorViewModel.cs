using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class NoiseGeneratorViewModel : ViewModelBase
{
    public Noise2DLite Noise => _noise;
    public IWorkspace Workspace => _workspace;

    private Bitmap? _noiseBitmap = null;
    public Bitmap? NoiseBitmap
    {
        get { return _noiseBitmap; }
        set
        {
            if (_noiseBitmap != value)
            {
                _noiseBitmap = value;
                OnPropertyChanged(nameof(NoiseBitmap));
            }
        }
    }
    
    private bool _isGenerating = false;
    public bool IsGenerating
    {
        get { return _isGenerating; }
        set
        {
            if (_isGenerating != value)
            {
                _isGenerating = value;
                OnPropertyChanged(nameof(IsGenerating));
            }
        }
    }

    public NoiseGeneratorViewModel(NoiseGeneratorView view, IWorkspace workspace)
    {
        _view = view;
        _workspace = workspace;
    }

    [RelayCommand]
    public async Task SaveNoise()
    {
        while (IsGenerating)
        {
            await Task.Delay(100);
        }
        TopLevel topLevel = TopLevel.GetTopLevel(_view)!;
        var options = new FilePickerSaveOptions
        {
            Title = "Save Noise Texture",
            SuggestedFileName = "noise.png",
            DefaultExtension = "png",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("PNG Image")
                {
                    Patterns = ["*.png"],
                    AppleUniformTypeIdentifiers = ["public.png"]
                }
            },
        };
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file != null)
        {
            using var stream = await file.OpenWriteAsync();
            NoiseBitmap?.Save(stream);
        }
    }

    Noise2DLite _noise = new Noise2DLite();
    NoiseGeneratorView _view;
    IWorkspace _workspace;
}