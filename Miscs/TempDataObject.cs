using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace Fantania;

public class TempDataObject : IDataObject
{
    public object Wrapped { get; set; }

    public TempDataObject(object obj)
    {
        Wrapped = obj;
    }

    public bool Contains(string dataFormat)
    {
        return false;
    }

    public object? Get(string dataFormat)
    {
        return null;
    }

    public IEnumerable<string> GetDataFormats()
    {
        return Array.Empty<string>();
    }
}