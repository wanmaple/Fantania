namespace FantaniaLib;

using System.Numerics;

[BindingScript]
public interface IRenderDevice
{
    void CheckError();

    string GetString(int target);
    void ClearColor(Vector4 color);
    void ClearBufferBits(BufferBits mask);
    void Viewport(int x, int y, int width, int height);
    void SetupFrameBufferSRGB(bool enabled);

    int CreateTexture2D(TextureDescription desc, nint data);
    void DeleteTexture(int id);
    FrameBuffer CreateFrameBuffer(FrameBufferDescription desc);
    bool IsFrameBufferReady();
    void DeleteRenderBuffer(int id);
    void DeleteFrameBuffer(int id);
    void SetRenderTarget(int fbo);
    void SetRenderTargets(int fbo, int colorAttachmentCount);
    ShaderProgram? CreateProgram(string vertSrc, string fragSrc);
    void ApplyShaderProgram(ShaderProgram program);
    void DeleteProgram(int id);
    void ApplyRenderState(RenderState state);
    void ApplyUniform(ShaderProgram shader, string name, MaterialUniform uniform);
    void ApplyMaterial(ShaderProgram shader, params IReadonlyUniformSet[] uniforms);
    VertexStream CreateVertexStream(VertexDescriptor vertDesc, int maxVertBufferBytes = 160 * 1024, int maxIndiceBufferBytes = 80 * 1024);
    void ApplyVertexStream(VertexStream vertStream);
    void SyncVertexStream(VertexStream vertStream);
    void Draw(VertexStream vertStream, ShaderProgram shader, params IReadonlyUniformSet[] uniforms);
    void DeleteVertexArray(int id);
    void DeleteBuffer(int id);
    
    void DeleteResource(IRenderResource res);
}