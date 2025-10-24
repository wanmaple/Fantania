using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Fantania.ViewModels;

namespace Fantania;

public partial class EditNoise2DControl : UserControl
{
    public static readonly StyledProperty<ObservableCollection<IEditableProperty>> EditablePropertiesProperty = AvaloniaProperty.Register<EditNoise2DControl, ObservableCollection<IEditableProperty>>(nameof(EditableProperties), defaultValue: null);
    public ObservableCollection<IEditableProperty> EditableProperties
    {
        get => GetValue(EditablePropertiesProperty);
        set => SetValue(EditablePropertiesProperty, value);
    }

    public static readonly StyledProperty<IEditableProperty> NoiseTypeProperty = AvaloniaProperty.Register<EditNoise2DControl, IEditableProperty>(nameof(NoiseType), defaultValue: null);
    public IEditableProperty NoiseType
    {
        get => GetValue(NoiseTypeProperty);
        set => SetValue(NoiseTypeProperty, value);
    }

    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public EditNoise2DControl()
    {
        InitializeComponent();
        _noiseArgs[(int)NoiseTypes.Cellular] = new CellularNoiseParameters();
        _noiseArgs[(int)NoiseTypes.Perlin] = new PerlinNoiseParameters();
        _noiseArgs[(int)NoiseTypes.Simplex] = new SimplexNoiseParameters();
        _noiseArgs[(int)NoiseTypes.Value] = new ValueNoiseParameters();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _noiseRenderer = new Noise2DRenderer(preview.ColorBufferWidth, preview.ColorBufferHeight);
        _noiseRenderer.RefreshDurationInMs = 0;
        preview.AddRenderer(_noiseRenderer);
        Noise2DLite noiseLite = (Noise2DLite)EditableProperty.EditValue;
        _noise = noiseLite.Clone();
        _noiseArgs[(int)_noise.NoiseType].CopyFrom(_noise.NoiseArguments);
        _noiseRenderer.Noise = _noise;
        NoiseType = new TempEditableProperty(_noise, _noise.GetType().GetProperty("NoiseType"), new EditEnumAttribute());
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.ValueChanged += OnNoiseChanged;
        OnNoiseTypeChanged(noiseLite.NoiseType);
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _noise.NoiseType = (NoiseTypes)NoiseType.EditValue;
            foreach (var prop in EditableProperties)
            {
                if (prop.Instance is Noise2DLite)
                {
                    object boxed = _noise;
                    prop.PropertyInfo.SetValue(boxed, prop.EditValue);
                    _noise = (Noise2DLite)boxed;
                }
            }
            _noise.NoiseArguments.CopyFrom(_noiseArgs[(int)_noise.NoiseType]);
            if (EditableProperty != null && (Noise2DLite)EditableProperty.EditValue != _noise)
            {
                EditableProperty.EditValue = _noise;
            }
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    void OnNoiseChanged(object noise)
    {
        Noise2DLite noiseLite = (Noise2DLite)noise;
        _noise = noiseLite.Clone();
        _noiseRenderer.Noise = _noise;
        NoiseType.EditValue = _noise.NoiseType;
        _noiseArgs[(int)_noise.NoiseType].CopyFrom(_noise.NoiseArguments);
        if (_uiNoiseType != noiseLite.NoiseType)
        {
            OnNoiseTypeChanged(noiseLite.NoiseType);
        }
        else
        {
            foreach (var prop in EditableProperties)
            {
                if (prop.Instance is Noise2DLite)
                {
                    prop.EditValue = prop.PropertyInfo.GetValue(_noise);
                }
                else
                {
                    prop.EditValue = prop.PropertyInfo.GetValue(_noise.NoiseArguments);
                }
            }
        }
    }

    void OnNoiseTypeChanged(NoiseTypes noiseType)
    {
        if (_uiNoiseType != noiseType)
        {
            EditableProperties = ObjectEditableCollector.CollectTemporaryEditableProperties(_noise);
            var uniqueProps = ObjectEditableCollector.CollectTemporaryEditableProperties(_noiseArgs[(int)noiseType]);
            foreach (var prop in uniqueProps)
            {
                EditableProperties.Add(prop);
            }
            pvNoise.DataContext = new PropertiesViewModel(EditableProperties);
            _uiNoiseType = noiseType;
        }
    }

    Noise2DRenderer _noiseRenderer;
    Noise2DLite _noise;
    NoiseTypes? _uiNoiseType;
    INoiseParameters[] _noiseArgs = new INoiseParameters[Enum.GetValues<NoiseTypes>().Length];
}