namespace FantaniaLib;

using System.Numerics;

[BindingScript]
public interface IRenderDevice
{
    void CheckError();

    void ClearColor(Vector4 color);
    void ClearBufferBits(BufferBits mask);
    void Viewport(int x, int y, int width, int height);

    int CreateTexture2D(TextureDescription desc, nint data);
    void DeleteTexture(int id);
    FrameBuffer CreateFrameBuffer(FrameBufferDescription desc);
    void DeleteRenderBuffer(int id);
    void DeleteFrameBuffer(int id);
    void SetRenderTarget(int fbo);
    ShaderProgram? CompileShader(string vertSrc, string fragSrc);
    void ApplyShaderProgram(ShaderProgram program);
    void DeleteProgram(int id);
    void ApplyRenderState(RenderState state);
    void ApplyUniform(ShaderProgram shader, string name, RenderMaterial.MaterialUniform uniform);
    void ApplyMaterial(RenderMaterial material);
    VertexStream CreateVertexStream(VertexDescriptor vertDesc);
    void ApplyVertexStream(VertexStream vertStream);
    void SyncVertexStream(VertexStream vertStream);
    void Draw(VertexStream vertStream, RenderMaterial material);
    void DeleteVertexArray(int id);
    void DeleteBuffer(int id);
    
    void DeleteResource(IRenderResource res);
}