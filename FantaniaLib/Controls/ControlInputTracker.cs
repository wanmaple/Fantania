using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace FantaniaLib;

public class KeySet
{
    public bool IsKeyPressing(Key key)
    {
        return _keys.Contains(key);
    }

    public void Add(Key key)
    {
        _keys.Add(key);
    }

    public void Remove(Key key)
    {
        _keys.Remove(key);
    }

    public void Clear()
    {
        _keys.Clear();
    }

    HashSet<Key> _keys = new HashSet<Key>(8);
}

public struct ControlMouseState
{
    public Point Position { get; set; }
    public Vector Movement { get; set; }
    public bool IsLeftButtonPressed { get; set; }
    public bool IsRightButtonPressed { get; set; }
    public bool IsMiddleButtonPressed { get; set; }
    public bool IsLeftButtonJustReleased { get; set; }
    public bool IsRightButtonJustReleased { get; set; }
    public bool IsMiddleButtonJustReleased { get; set; }
    public Vector WheelDelta { get; set; }
}

public struct ControlKeyState
{
    public KeyModifiers KeyModifiers { get; set; }
    public KeySet? KeySet { get; set; }
    public Key JustReleased { get; set; }
}

public class ControlInputEventArgs : EventArgs
{
    public static readonly ControlInputEventArgs Invalid = new ControlInputEventArgs
    {
        IsValid = false,
    };

    public ControlMouseState MouseState { get; set; }
    public ControlKeyState KeyState { get; set; }
    public bool IsValid { get; set; }
    public bool Handled { get; set; }
}

public class ControlInputTracker : IDisposable
{
    public event EventHandler<ControlInputEventArgs>? MouseEntered;
    public event EventHandler<ControlInputEventArgs>? MouseExited;
    public event EventHandler<ControlInputEventArgs>? MouseMoved;
    public event EventHandler<ControlInputEventArgs>? MousePressed;
    public event EventHandler<ControlInputEventArgs>? MouseReleased;
    public event EventHandler<ControlInputEventArgs>? MouseWheelChanged;
    public event EventHandler<ControlInputEventArgs>? MouseCaptureLost;
    public event EventHandler<ControlInputEventArgs>? KeyDown;
    public event EventHandler<ControlInputEventArgs>? KeyUp;

    public Point Position { get; private set; }
    public Vector Movement { get; private set; }
    public bool IsLeftButtonPressed { get; private set; }
    public bool IsRightButtonPressed { get; private set; }
    public bool IsMiddleButtonPressed { get; private set; }
    public bool IsLeftButtonJustReleased { get; private set; }
    public bool IsRightButtonJustReleased { get; private set; }
    public bool IsMiddleButtonJustReleased { get; private set; }
    public Vector WheelDelta { get; private set; }
    public KeyModifiers KeyModifiers { get; private set; }
    public KeySet KeySet { get; private set; } = new KeySet();
    public Key JustReleased { get; private set; }

    public ControlInputTracker(Control control)
    {
        _targetControl = control;
        _targetControl.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
        _targetControl.AddHandler(InputElement.PointerExitedEvent, OnPointerExited);
        _targetControl.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _targetControl.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        _targetControl.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _targetControl.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        _targetControl.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        _targetControl.AddHandler(InputElement.KeyDownEvent, OnKeyDown);
        _targetControl.AddHandler(InputElement.KeyUpEvent, OnKeyUp);
    }

    void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_isDisposed) return;
        UpdatePosition(e);
        UpdateButtonStates(e);
        KeyModifiers = e.KeyModifiers;
        var args = CreateInputEventArgs();
        MouseEntered?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_isDisposed) return;
        ClearInputStates();
        var args = CreateInputEventArgs();
        MouseExited?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDisposed) return;
        UpdateMovement(e);
        UpdatePosition(e);
        // UpdateButtonStates(e);
        KeyModifiers = e.KeyModifiers;
        var args = CreateInputEventArgs();
        MouseMoved?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_isDisposed) return;
        UpdatePosition(e);
        UpdateButtonStates(e);
        KeyModifiers = e.KeyModifiers;
        var args = CreateInputEventArgs();
        MousePressed?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDisposed) return;
        UpdatePosition(e);
        UpdateButtonStates(e);
        KeyModifiers = e.KeyModifiers;
        var args = CreateInputEventArgs();
        MouseReleased?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_isDisposed) return;
        WheelDelta = e.Delta;
        UpdatePosition(e);
        KeyModifiers = e.KeyModifiers;
        var args = CreateInputEventArgs();
        MouseWheelChanged?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_isDisposed) return;
        ClearInputStates();
        var args = CreateInputEventArgs();
        MouseCaptureLost?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_isDisposed || !_targetControl.IsFocused) return;
        KeyModifiers = e.KeyModifiers;
        KeySet.Add(e.Key);
        var args = CreateInputEventArgs();
        KeyDown?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (_isDisposed || !_targetControl.IsFocused) return;
        KeyModifiers = e.KeyModifiers;
        JustReleased = e.Key;
        KeySet.Remove(e.Key);
        var args = CreateInputEventArgs();
        KeyUp?.Invoke(_targetControl, args);
        e.Handled = args.Handled;
    }

    void UpdatePosition(PointerEventArgs e)
    {
        Position = e.GetPosition(_targetControl);
    }

    void UpdateMovement(PointerEventArgs e)
    {
        Movement = e.GetPosition(_targetControl) - Position;
    }

    void UpdateButtonStates(PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(_targetControl).Properties;
        bool oldLeftPressed = IsLeftButtonPressed;
        bool oldRightPressed = IsRightButtonPressed;
        bool oldMiddlePressed = IsMiddleButtonPressed;
        IsLeftButtonPressed = properties.IsLeftButtonPressed;
        IsRightButtonPressed = properties.IsRightButtonPressed;
        IsMiddleButtonPressed = properties.IsMiddleButtonPressed;
        IsLeftButtonJustReleased = oldLeftPressed && !IsLeftButtonPressed;
        IsRightButtonJustReleased = oldRightPressed && !IsRightButtonPressed;
        IsMiddleButtonJustReleased = oldMiddlePressed && !IsMiddleButtonPressed;
    }

    void ClearInputStates()
    {
        Position = new Point(double.NaN, double.NaN);
        Movement = Vector.Zero;
        IsLeftButtonPressed = IsRightButtonPressed = IsMiddleButtonPressed = false;
        IsLeftButtonJustReleased = IsRightButtonJustReleased = IsMiddleButtonJustReleased = false;
        WheelDelta = Vector.Zero;
    }

    public ControlInputEventArgs CreateInputEventArgs()
    {
        return new ControlInputEventArgs
        {
            IsValid = true,
            MouseState = new ControlMouseState
            {
                Position = Position,
                Movement = Movement,
                IsLeftButtonPressed = IsLeftButtonPressed,
                IsRightButtonPressed = IsRightButtonPressed,
                IsMiddleButtonPressed = IsMiddleButtonPressed,
                IsLeftButtonJustReleased = IsLeftButtonJustReleased,
                IsRightButtonJustReleased = IsRightButtonJustReleased,
                IsMiddleButtonJustReleased = IsMiddleButtonJustReleased,
                WheelDelta = WheelDelta,
            },
            KeyState = new ControlKeyState
            {
                KeyModifiers = KeyModifiers,
                JustReleased = JustReleased,
                KeySet = KeySet,
            },
        };
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _targetControl.RemoveHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
        _targetControl.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
        _targetControl.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _targetControl.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        _targetControl.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _targetControl.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        _targetControl.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        _targetControl.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _targetControl.RemoveHandler(InputElement.KeyUpEvent, OnKeyUp);
        _isDisposed = true;
    }

    readonly Control _targetControl;
    bool _isDisposed = false;
}