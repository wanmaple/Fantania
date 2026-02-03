namespace FantaniaLib;

[BindingScript]
public class MaterialSet
{
    public IEnumerable<RenderMaterial> AllMaterials => _materials.Values;

    public MaterialSet(ShaderProgram fallback)
    {
        _fallback = new RenderMaterial
        {
            Shader = fallback,
        };
    }

    public RenderMaterial GetMaterial(string key)
    {
        if (_materials.TryGetValue(key, out var mat))
        {
            return mat.Clone();
        }
        return _fallback.Clone();
    }

    public void AddMaterial(string key, RenderMaterial material)
    {
        _materials[key] = material;
    }

    Dictionary<string, RenderMaterial> _materials = new Dictionary<string, RenderMaterial>(16);
    RenderMaterial _fallback;
}