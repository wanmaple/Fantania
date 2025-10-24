using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Avalonia.Skia.Helpers;
using Fantania.ViewModels;
using SkiaSharp;

namespace Fantania.Models;

[DataGroup(Group = "StylegroundTemplates"), DisableEditing("MainTexture")]
public class GradientStylegroundTemplate : StylegroundTemplate, IRequireBackgroundJob
{
    public override Type StylegroundType => typeof(Styleground);

    private Gradient2D _gradient = Gradient2D.Default;
    [EditGradient2D, DatabaseGradient2D, Tooltip("TooltipStylegroundGradient")]
    public Gradient2D Gradient
    {
        get => _gradient;
        set
        {
            if (_gradient != value)
            {
                OnPropertyChanging(nameof(Gradient));
                lock (_lock)
                    _gradient = value;
                OnPropertyChanged(nameof(Gradient));
            }
        }
    }

    volatile bool _dirty = false;
    public bool JobDirty
    {
        get => _dirty;
        set => Interlocked.Exchange(ref _dirty, value);
    }

    public override IRenderer CreateRenderer()
    {
        var renderer = new GradientRenderer(512, 512);
        renderer.Gradient = Gradient;
        // renderer.Blur = true;
        return renderer;
    }

    public override void UpdateRenderer(IRenderer renderer)
    {
        if (renderer is GradientRenderer gradientRenderer)
        {
            gradientRenderer.Gradient = Gradient;
        }
    }

    public override void OnInitialized(Workspace workspace)
    {
        base.OnInitialized(workspace);
        PropertyChanged += OnGradientChanged;
    }

    public override void OnUnintialized(Workspace workspace)
    {
        PropertyChanged -= OnGradientChanged;
        base.OnUnintialized(workspace);
    }

    void OnGradientChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Gradient))
        {
            WorkspaceViewModel.Current.Workspace.JobManager.AddJob(this);
        }
    }

    public unsafe void DoBackgroundJob(Workspace workspace)
    {
        const int SIZE = 64;
        byte* data = stackalloc byte[SIZE * SIZE * 4];
        Gradient2D gradient;
        lock (_lock)
            gradient = Gradient.Clone();
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                float sampleX = (x + 0.5f) / SIZE;
                float sampleY = (y + 0.5f) / SIZE;
                System.Numerics.Vector4 color = gradient.Evaluate(new System.Numerics.Vector2(sampleX, sampleY)).Linear2Srgb();
                byte r = (byte)(color.X * 255);
                byte g = (byte)(color.Y * 255);
                byte b = (byte)(color.Z * 255);
                byte a = (byte)(color.W * 255);
                int offset = (y * SIZE + x) * 4;
                data[offset] = r;
                data[offset + 1] = g;
                data[offset + 2] = b;
                data[offset + 3] = a;
            }
        }
        var imgInfo = new SKImageInfo(SIZE, SIZE, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
        SKPixmap pixmap = new SKPixmap(imgInfo, (nint)data, SIZE * 4);
        using (var image = SKImage.FromPixels(pixmap))
        {
            string imagePath = Path.Join(workspace.MessFolder, GenerateUniqueName() + ".png");
            using (var fs = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
            {
                ImageSavingHelper.SaveImage(image, fs, 20);
                fs.Flush();
            }
        }
    }

    public void OnJobCompleted(Workspace workspace)
    {
        string imagePath = Path.Join(Workspace.MESS_FOLDER, GenerateUniqueName() + ".png");
        MainTexture = imagePath.Replace('\\', '/');
        OnPropertyChanged(nameof(IconPath));
    }

    public bool ShouldQueueJob(Workspace workspace)
    {
        return !MainTexture.StartsWith(Workspace.MESS_FOLDER) || !File.Exists(workspace.GetAbsolutePath(MainTexture));
    }

    public void OnDestroy(Workspace workspace)
    {
        if (MainTexture.StartsWith(Workspace.MESS_FOLDER))
        {
            string imagePath = workspace.GetAbsolutePath(MainTexture);
            if (File.Exists(imagePath))
                File.Delete(imagePath);
        }
    }

    object _lock = new object();
}