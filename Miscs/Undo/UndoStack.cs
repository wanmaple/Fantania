using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class UndoStack : ObservableObject
{
    public bool IsUndoable => _num > 0;
    public bool IsRedoable => _totalNum > _num;

    public UndoStack(int maxOps = 512)
    {
        _operations = new IUndoable[maxOps];
    }

    public void AddOperation(IUndoable op)
    {
        if (_num > 0)
        {
            // try merge operations.
            IUndoable lastOp = _operations[_topIndex];
            if (!_forceStopMerging && lastOp.TryMerge(op, out var merged))
            {
                _operations[_topIndex] = merged;
                return;
            }
        }
        _forceStopMerging = false;
        if (_num == _operations.Length)
        {
            _topIndex = _startIndex;
            _startIndex = (_startIndex + 1) % _operations.Length;
            _operations[_topIndex] = op;
        }
        else
        {
            _topIndex = (_num + _startIndex) % _operations.Length;
            _operations[_topIndex] = op;
            ++_num;
            _totalNum = _num;
        }
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    public void Undo()
    {
        if (!IsUndoable) return;
        _operations[_topIndex].Undo();
        --_num;
        _topIndex = (_topIndex - 1 + _operations.Length) % _operations.Length;
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    public void Redo()
    {
        if (!IsRedoable) return;
        _operations[(_topIndex + 1) % _operations.Length].Redo();
        _topIndex = (_topIndex + 1) % _operations.Length;
        ++_num;
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    public void Clear()
    {
        _num = _totalNum = 0;
        _startIndex = 0;
        _topIndex = 0;
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    public void ForceStopMerging()
    {
        _forceStopMerging = true;
    }

    IUndoable[] _operations;
    int _topIndex = 0;
    int _startIndex = 0;
    int _num = 0;
    int _totalNum = 0;

    List<IUndoable> _cacheGroup = new List<IUndoable>(16);
    bool _forceStopMerging = false;
}