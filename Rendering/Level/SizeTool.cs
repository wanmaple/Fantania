using System;
using System.Diagnostics.CodeAnalysis;
using Fantania.Models;

namespace Fantania;

public class SizeTool
{
    [Flags]
    public enum SizeDirections : uint
    {
        None = 0x0,
        Left = 0x1,
        Right = 0x2,
        Top = 0x4,
        Bottom = 0x8,
        TopLeft = Left | Top,
        TopRight = Right | Top,
        BottomLeft = Left | Bottom,
        BottomRight = Right | Bottom,
    }

    public ISizeableObject SizingObject => _sizing;

    public void Prepare([NotNull] ISizeableObject obj, SizeDirections direction)
    {
        _sizing = obj;
        _dir = direction;
        double x = 0.0, y = 0.0;
        if (_dir.HasFlag(SizeDirections.Left))
            x = _sizing.Right;
        else if (_dir.HasFlag(SizeDirections.Right))
            x = _sizing.Left;
        if (_dir.HasFlag(SizeDirections.Top))
            y = _sizing.Bottom;
        else if (_dir.HasFlag(SizeDirections.Bottom))
            y = _sizing.Top;
        _orig = new Avalonia.Vector(x, y);
    }

    public void Cancel()
    {
        _sizing = null;
    }

    public void Apply(Avalonia.Vector currentWorldPos)
    {
        double sizeX = _sizing.CustomSize.X;
        double sizeY = _sizing.CustomSize.Y;
        double l = _sizing.Left;
        double t = _sizing.Top;
        if (_dir.HasFlag(SizeDirections.Left) || _dir.HasFlag(SizeDirections.Right))
        {
            double changeX = currentWorldPos.X - _orig.X;
            sizeX = Math.Abs(changeX);
            l = Math.Min(_orig.X, currentWorldPos.X);
        }
        if (_dir.HasFlag(SizeDirections.Top) || _dir.HasFlag(SizeDirections.Bottom))
        {
            double changeY = currentWorldPos.Y - _orig.Y;
            sizeY = Math.Abs(changeY);
            t = Math.Min(_orig.Y, currentWorldPos.Y);
        }
        Avalonia.Vector size = new Avalonia.Vector(sizeX, sizeY);
        _sizing.CustomSize = size;
        _sizing.Position = new Avalonia.Vector(l + _sizing.Anchor.X * _sizing.CustomSize.X, t + _sizing.Anchor.Y * _sizing.CustomSize.Y);
    }

    ISizeableObject _sizing = null;
    SizeDirections _dir;
    Avalonia.Vector _orig;
}