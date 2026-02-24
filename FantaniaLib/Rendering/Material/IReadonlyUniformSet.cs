namespace FantaniaLib;

public interface IReadonlyUniformSet : IEnumerable<KeyValuePair<string, MaterialUniform>>
{
    public IReadOnlyCollection<string> Names { get; }
    public MaterialUniform this[string name] { get; }
}