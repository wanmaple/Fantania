namespace FantaniaLib;

[BindingScript]
public class ShaderCache : IDisposable
{
    public ShaderCache(IRenderDevice device)
    {
        _device = device;
        string vertFallback = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/vert_standard.vs");
        string fragFallback = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/frag_fallback.fs");
        _fallback = _device.CreateProgram(vertFallback, fragFallback)!;
    }

    public ShaderProgram Acquire(string vertSrc, string fragSrc)
    {
        var tuple = (vertSrc, fragSrc);
        if (_blacklist.Contains(tuple))
            return _fallback;
        if (!_programs.TryGetValue(tuple, out var counter))
        {
            ShaderProgram? program = _device.CreateProgram(vertSrc, fragSrc);
            if (program == null)
            {
                _blacklist.Add(tuple);
                return _fallback;
            }
            counter = new ReferenceCounter<ShaderProgram>(program);
            _programs.Add(tuple, counter);
        }
        else
            counter.Acquire();
        return counter.Item;
    }

    public void Release(ShaderProgram program)
    {
        var tuple = (program.VertexShaderSource, program.FragmentShaderSource);
        if (_programs.TryGetValue(tuple, out var counter))
        {
            counter.Release();
            if (counter.IsFree)
            {
                _programs.Remove(tuple);
                program.Dispose(_device);
            }
        }
    }

    public void Dispose()
    {
        _device.DeleteProgram(_fallback.ProgramID);
    }

    IRenderDevice _device;
    ShaderProgram _fallback;
    Dictionary<(string, string), ReferenceCounter<ShaderProgram>> _programs = new Dictionary<(string, string), ReferenceCounter<ShaderProgram>>(32);
    HashSet<(string, string)> _blacklist = new HashSet<(string, string)>(8);
}