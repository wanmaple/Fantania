namespace FantaniaLib;

public struct VertexAttribute
{
    public int Location { get; set; }
    public int ElementCount { get; set; }
    public bool Normalized { get; set; }
}

[BindingScript]
public class VertexDescriptor
{
    public int SizeofVertex => _sumElements * sizeof(float);
    public IReadOnlyList<VertexAttribute> Attributes => _attribs;

    internal VertexDescriptor(IEnumerable<VertexAttribute> attribs)
    {
        foreach (var attrib in attribs)
        {
            AddAttribute(attrib);
        }
    }

    void AddAttribute(VertexAttribute attrib)
    {
        _attribs.Add(attrib);
        _sumElements += attrib.ElementCount;
    }

    List<VertexAttribute> _attribs = new List<VertexAttribute>(0);
    int _sumElements = 0;
}