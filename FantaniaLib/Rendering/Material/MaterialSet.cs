namespace FantaniaLib;

[BindingScript]
public class MaterialSet
{
    public IEnumerable<RenderMaterial> AllMaterials => _materials.Values;

    public MaterialSet()
    {
    }

    public RenderMaterial? GetMaterial(string key)
    {
        if (_materials.TryGetValue(key, out var mat))
        {
            return mat.Clone();
        }
        return null;
    }

    public void AddMaterial(string key, RenderMaterial material)
    {
        _materials[key] = material;
    }

    Dictionary<string, RenderMaterial> _materials = new Dictionary<string, RenderMaterial>(16);
}