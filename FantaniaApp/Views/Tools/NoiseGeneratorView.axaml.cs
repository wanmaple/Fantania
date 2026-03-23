using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public partial class NoiseGeneratorView : Window
{
    NoiseGeneratorViewModel ViewModel => (NoiseGeneratorViewModel)DataContext!;
    CancellationTokenSource? _refreshCts;

    public NoiseGeneratorView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ViewModel.Noise.PropertyChanged += Noise_PropertyChanged;
        foreach (var noiseType in Enum.GetValues<NoiseTypes>())
        {
            ViewModel.Noise.NoiseParameterAt(noiseType).PropertyChanged += Noise_PropertyChanged;
        }
        _ = RefreshTextureAsync(CancellationToken.None);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;
        foreach (var noiseType in Enum.GetValues<NoiseTypes>())
        {
            ViewModel.Noise.NoiseParameterAt(noiseType).PropertyChanged -= Noise_PropertyChanged;
        }
        ViewModel.Noise.PropertyChanged -= Noise_PropertyChanged;
        base.OnUnloaded(e);
    }

    void Noise_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 任何属性变化都重新刷新纹理
        _ = RefreshTextureAsync(CancellationToken.None);
    }

    async Task RefreshTextureAsync(CancellationToken externalCt)
    {
        // 取消之前的任务
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = new CancellationTokenSource();
        // 合并外部和内部的取消令牌
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt, _refreshCts.Token);
        var ct = linkedCts.Token;
        try
        {
            ViewModel.IsGenerating = true;
            int width = ViewModel.Noise.Size.X;
            int height = ViewModel.Noise.Size.Y;
            Noise2DLite noiseClone = ViewModel.Noise.Clone();
            // 在后台线程执行生成逻辑
            var bitmap = await Task.Run(() => 
            {
                if (ct.IsCancellationRequested) return null;
                return RefreshTexture(ct, noiseClone, width, height);
            }, ct);
            if (bitmap != null && !ct.IsCancellationRequested)
            {
                ViewModel.NoiseBitmap = bitmap;
            }
        }
        catch (OperationCanceledException)
        {
            // 任务被取消，属于正常情况
        }
        finally
        {
            if (!ct.IsCancellationRequested)
            {
                ViewModel.IsGenerating = false;
            }
        }
    }

    unsafe Bitmap? RefreshTexture(CancellationToken ct, Noise2DLite noise, int width, int height)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Opaque);
        using (var frameBuffer = bitmap.Lock())
        {
            byte* data = (byte*)frameBuffer.Address;
            int stride = frameBuffer.RowBytes;
            for (int y = 0; y < height; y++)
            {
                byte* row = data + y * stride;
                for (int x = 0; x < width; x++)
                {
                    if (ct.IsCancellationRequested)
                        return null;
                    float sampleX = x + 0.5f;
                    float sampleY = y + 0.5f;
                    float val = (noise.Get(sampleX, sampleY, Math.Max(width, height)) + 1.0f) * 0.5f;
                    val = Math.Clamp(val, 0f, 1f);
                    byte pixelValue = (byte)(val * 255);
                    row[x * 4 + 0] = pixelValue;     // R
                    row[x * 4 + 1] = pixelValue;     // G
                    row[x * 4 + 2] = pixelValue;     // B
                    row[x * 4 + 3] = 255;            // A
                }
            }
        }
        return bitmap;
    }
}