using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class Stylegrounds : ObservableObject
{
    public event Action<Stylegrounds, Styleground, Styleground> BackgroundAdded;
    public event Action<Stylegrounds, Styleground, Styleground> BackgroundRemoved;
    public event Action<Stylegrounds, Styleground, Styleground, Styleground> BackgroundMoved;
    public event Action<Stylegrounds, Styleground, Styleground> ForegroundAdded;
    public event Action<Stylegrounds, Styleground, Styleground> ForegroundRemoved;
    public event Action<Stylegrounds, Styleground, Styleground, Styleground> ForegroundMoved;

    public IReadOnlyList<Styleground> Backgrounds => _bgs;
    public IReadOnlyList<Styleground> Foregrounds => _fgs;

    public bool IsModified { get; set; } = false;

    public bool IsBackground(Styleground styleground)
    {
        return _bgs.Contains(styleground);
    }

    public bool IsForeground(Styleground styleground)
    {
        return _fgs.Contains(styleground);
    }

    public void RemoveStyleground(Styleground styleground)
    {
        if (_bgs.Contains(styleground))
        {
            RemoveBackground(styleground);
        }
        else if (_fgs.Contains(styleground))
        {
            RemoveForeground(styleground);
        }
    }

    public Styleground AddBackground(StylegroundTemplate template)
    {
        var styleground = Activator.CreateInstance(template.StylegroundType, [template,]) as Styleground;
        styleground.Name = $"[{template.Name}]";
        Styleground prev = _bgs.LastOrDefault();
        _bgs.Add(styleground);
        IsModified = true;
        BackgroundAdded?.Invoke(this, styleground, prev);
        return styleground;
    }

    public void RemoveBackground(Styleground bg)
    {
        int index = _bgs.IndexOf(bg);
        if (index >= 0)
        {
            Styleground prev = index > 0 ? _bgs[index - 1] : null;
            _bgs.RemoveAt(index);
            IsModified = true;
            BackgroundRemoved?.Invoke(this, bg, prev);
        }
    }

    public Styleground AddForeground(StylegroundTemplate template)
    {
        var styleground = new Styleground(template);
        styleground.Name = $"[{template.Name}]";
        Styleground prev = _fgs.LastOrDefault();
        _fgs.Add(styleground);
        IsModified = true;
        ForegroundAdded?.Invoke(this, styleground, prev);
        return styleground;
    }

    public void RemoveForeground(Styleground fg)
    {
        int index = _fgs.IndexOf(fg);
        if (index >= 0)
        {
            Styleground prev = index > 0 ? _fgs[index - 1] : null;
            _fgs.RemoveAt(index);
            IsModified = true;
            ForegroundRemoved?.Invoke(this, fg, prev);
        }
    }

    public void InsertBackgroundAfter(Styleground bg, Styleground prev)
    {
        if (prev != null)
        {
            int index = _bgs.IndexOf(prev);
            _bgs.Insert(index + 1, bg);
        }
        else
        {
            _bgs.Insert(0, bg);
        }
        IsModified = true;
        BackgroundAdded?.Invoke(this, bg, prev);
    }

    public void InsertBackgroundBefore(Styleground bg, Styleground after)
    {
        Styleground prev = null;
        if (after != null)
        {
            int index = _bgs.IndexOf(after);
            prev = index == 0 ? null : _bgs[index - 1];
            _bgs.Insert(index, bg);
        }
        else
        {
            prev = _bgs.LastOrDefault();
            _bgs.Add(bg);
        }
        IsModified = true;
        BackgroundAdded?.Invoke(this, bg, prev);
    }

    public void MoveBackgroundBefore(Styleground bg, Styleground after)
    {
        int index1 = _bgs.IndexOf(bg);
        Styleground oldPrev = index1 == 0 ? null : _bgs[index1 - 1];
        _bgs.Remove(bg);
        int index2 = _bgs.IndexOf(after);
        Styleground newPrev = index2 == 0 ? null : _bgs[index2 - 1];
        _bgs.Insert(index2, bg);
        IsModified = true;
        BackgroundMoved?.Invoke(this, bg, oldPrev, newPrev);
    }

    public void MoveBackgroundAfter(Styleground bg, Styleground before)
    {
        int index1 = _bgs.IndexOf(bg);
        Styleground oldPrev = index1 == 0 ? null : _bgs[index1 - 1];
        _bgs.Remove(bg);
        int index2 = _bgs.IndexOf(before);
        Styleground newPrev = index2 == 0 ? null : _bgs[index2 - 1];
        _bgs.Insert(index2 + 1, bg);
        IsModified = true;
        BackgroundMoved?.Invoke(this, bg, oldPrev, newPrev);
    }

    public void InsertForegroundAfter(Styleground fg, Styleground prev)
    {
        if (prev != null)
        {
            int index = _fgs.IndexOf(prev);
            _fgs.Insert(index + 1, fg);
        }
        else
        {
            _fgs.Insert(0, fg);
        }
        IsModified = true;
        ForegroundAdded?.Invoke(this, fg, prev);
    }

    public void InsertForegroundBefore(Styleground fg, Styleground after)
    {
        Styleground prev = null;
        if (after != null)
        {
            int index = _fgs.IndexOf(after);
            prev = index == 0 ? null : _bgs[index - 1];
            _fgs.Insert(index, fg);
        }
        else
        {
            prev = _fgs.LastOrDefault();
            _fgs.Add(fg);
        }
        IsModified = true;
        ForegroundAdded?.Invoke(this, fg, prev);
    }

    public void MoveForegroundBefore(Styleground fg, Styleground after)
    {
        int index1 = _fgs.IndexOf(fg);
        Styleground oldPrev = index1 == 0 ? null : _fgs[index1 - 1];
        _fgs.Remove(fg);
        int index2 = _fgs.IndexOf(after);
        Styleground newPrev = index2 == 0 ? null : _fgs[index2 - 1];
        _fgs.Insert(index2, fg);
        IsModified = true;
        ForegroundMoved?.Invoke(this, fg, oldPrev, newPrev);
    }

    public void MoveForegroundAfter(Styleground fg, Styleground before)
    {
        int index1 = _fgs.IndexOf(fg);
        Styleground oldPrev = index1 == 0 ? null : _fgs[index1 - 1];
        _fgs.Remove(fg);
        int index2 = _fgs.IndexOf(before);
        Styleground newPrev = index2 == 0 ? null : _fgs[index2 - 1];
        _fgs.Insert(index2 + 1, fg);
        IsModified = true;
        ForegroundMoved?.Invoke(this, fg, oldPrev, newPrev);
    }

    public bool CheckModified()
    {
        return IsModified;
    }

    public void Serialize(ObjectSerializer serializer, BinaryWriter writer)
    {
        writer.Write(_bgs.Count);
        foreach (var bg in _bgs)
        {
            serializer.Serialize(writer, bg);
        }
        writer.Write(_fgs.Count);
        foreach (var fg in _fgs)
        {
            serializer.Serialize(writer, fg);
        }
    }

    public void Deserialize(ObjectSerializer serializer, BinaryReader reader, Workspace workspace)
    {
        _bgs.Clear();
        _fgs.Clear();
        int bgNum = reader.ReadInt32();
        for (int i = 0; i < bgNum; i++)
        {
            var bg = serializer.Deserialize(reader, workspace) as Styleground;
            _bgs.Add(bg);
        }
        int fgNum = reader.ReadInt32();
        for (int i = 0; i < fgNum; i++)
        {
            var fg = serializer.Deserialize(reader, workspace) as Styleground;
            _fgs.Add(fg);
        }
    }

    ObservableCollection<Styleground> _bgs = new ObservableCollection<Styleground>();
    ObservableCollection<Styleground> _fgs = new ObservableCollection<Styleground>();
}