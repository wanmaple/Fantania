namespace FantaniaLib;

[BindingScript]
public class MaterialSet
{
    public MaterialSet(ShaderProgram fallback)
    {
        _fallback = fallback;
    }

    public RenderMaterial AcquireMaterial(string key, UniformSet uniforms)
    {
        if (!_shaders.TryGetValue(key, out var shader))
        {
            throw new RenderingException("Shader not found for material key: " + key);
        }
        var matKey = (shader, uniforms);
        if (!_materials.TryGetValue(matKey, out var matRef))
        {
            RenderMaterial material = new RenderMaterial(shader, uniforms);
            matRef = new ReferenceCounter<RenderMaterial>(material);
            _materials.Add(matKey, matRef);
            return material;
        }
        else
        {
            matRef.Acquire();
            return matRef.Item;
        }
    }

    public void ReleaseMaterial(RenderMaterial material)
    {
        var matKey = (material.Shader, (UniformSet)material.Uniforms);
        if (_materials.TryGetValue(matKey, out var matRef))
        {
            matRef.Release();
            if (matRef.IsFree)
            {
                _materials.Remove(matKey);
            }
        }
    }

    public RenderMaterial GetTemporaryMaterial(string key)
    {
        if (!_shaders.TryGetValue(key, out var shader))
        {
            throw new RenderingException("Shader not found for material key: " + key);
        }
        return new RenderMaterial(shader);
    }

    public ShaderProgram GetShader(string key)
    {
        if (_shaders.TryGetValue(key, out var shader))
        {
            return shader;
        }
        else
        {
            return _fallback;
        }
    }

    public void AddShader(string key, ShaderProgram shader)
    {
        _shaders[key] = shader;
    }

    Dictionary<string, ShaderProgram> _shaders = new Dictionary<string, ShaderProgram>(32);
    Dictionary<(ShaderProgram, UniformSet), ReferenceCounter<RenderMaterial>> _materials = new Dictionary<(ShaderProgram, UniformSet), ReferenceCounter<RenderMaterial>>(32);
    ShaderProgram _fallback;
}