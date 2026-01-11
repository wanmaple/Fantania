using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FantaniaLib;

namespace Fantania.Models;

public class RecentAccess
{
    public IReadOnlyList<string> RecentAccessList => _recents;

    public int MaxCount { get; private set; }

    public RecentAccess(int maxCnt = 10)
    {
        MaxCount = maxCnt;
        _recents = new ObservableCollection<string>();
    }

    public void Access(string acc)
    {
        int index = _recents.IndexOf(acc);
        if (index >= 0)
        {
            _recents.RemoveAt(index);
            _recents.Insert(0, acc);
            return;
        }
        while (_recents.Count >= MaxCount)
        {
            _recents.RemoveLast();
        }
        _recents.Insert(0, acc);
    }

    public void Clear()
    {
        _recents.Clear();
    }

    public void Serialize(Stream stream)
    {
        using (var sw = new StreamWriter(stream))
        {
            for (int i = 0; i < _recents.Count; i++)
            {
                sw.WriteLine(_recents[i]);
            }
        }
    }

    public void Deserialize(Stream stream)
    {
        _recents.Clear();
        using (var sr = new StreamReader(stream))
        {
            string? line = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) break;
                _recents.Add(line);
            }
        }
    }

    ObservableCollection<string> _recents;
}