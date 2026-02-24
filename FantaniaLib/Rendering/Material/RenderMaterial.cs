namespace FantaniaLib;

public class RenderMaterial
{
    public ShaderProgram Shader { get; set; }
    public IReadonlyUniformSet Uniforms => _uniforms;
    /// <summary>
    /// 除非你知道自己在干什么，否则不要使用它修改Uniforms。
    /// 一般来说，修改Uniform需要重新创建新的Material实例，这里只是为了修改Texture Uniform的TextureID而提供的接口，其他Uniform的修改请直接创建新的Material实例。
    /// </summary>
    public UniformSet MutableUniforms => _uniforms;

    public RenderMaterial(ShaderProgram shader)
    : this(shader, new UniformSet())
    {
    }

    public RenderMaterial(ShaderProgram shader, UniformSet uniforms)
    {
        Shader = shader;
        _uniforms = uniforms;
    }

    UniformSet _uniforms;
}