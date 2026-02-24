namespace FantaniaLib;

public class ShaderProgram : IEquatable<ShaderProgram>, IRenderResource
{
    public int ProgramID => _programId;
    public string VertexShaderSource => _srcVert;
    public string FragmentShaderSource => _srcFrag;

    internal ShaderProgram(int programId, string vertSrc, string fragSrc)
    {
        _srcVert = vertSrc;
        _srcFrag = fragSrc;
        _programId = programId;
    }

    public void Dispose(IRenderDevice device)
    {
        device.DeleteProgram(_programId);
    }

    public bool Equals(ShaderProgram? other)
    {
        return other != null && _programId == other._programId;
    }

    public override bool Equals(object? obj)
    {
        return obj is ShaderProgram && Equals((ShaderProgram)obj);
    }

    public override int GetHashCode()
    {
        return _programId.GetHashCode();
    }

    public override string ToString()
    {
        return _programId.ToString();
    }

    string _srcVert, _srcFrag;
    int _programId = 0;
}