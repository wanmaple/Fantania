using System.Numerics;
using Avalonia.OpenGL;
using static FantaniaLib.GLConstants;

namespace FantaniaLib;

public class GLDevice : IRenderDevice
{
    public GLDevice(GlInterface gl)
    {
        _gl = gl;
    }

    public void CheckError()
    {
        int err;
        while ((err = _gl.GetError()) != GL_NO_ERROR)
        {
            Console.WriteLine($"OpenGL Error: {err}");
        }
    }

    public string GetString(int target)
    {
        string? str = _gl.GetString(target);
        DebugError();
        return str ?? string.Empty;
    }

    public void ClearColor(Vector4 color)
    {
        _gl.ClearColor(color.X, color.Y, color.Z, color.W);
        DebugError();
    }

    public void ClearBufferBits(BufferBits mask)
    {
        if (mask != BufferBits.None)
        {
            int bits = 0;
            if (mask.HasFlag(BufferBits.Color))
                bits |= GL_COLOR_BUFFER_BIT;
            if (mask.HasFlag(BufferBits.Depth))
                bits |= GL_DEPTH_BUFFER_BIT;
            if (mask.HasFlag(BufferBits.Stencil))
                bits |= GL_STENCIL_BUFFER_BIT;
            _gl.Clear(bits);
        }
        DebugError();
    }

    public void Viewport(int x, int y, int width, int height)
    {
        _gl.Viewport(x, y, width, height);
        DebugError();
    }

    public void SetupFrameBufferSRGB(bool enabled)
    {
        if (enabled)
            _gl.Enable(GL_FRAMEBUFFER_SRGB);
        else
            _gl.Disable(GL_FRAMEBUFFER_SRGB);
        DebugError();
    }

    public int CreateTexture2D(TextureDescription desc, nint data = 0)
    {
        int id = _gl.GenTexture();
        _gl.BindTexture(GL_TEXTURE_2D, id);
        var (internalFormat, format, type) = desc.Format switch
        {
            TextureFormats.R8 => (GL_R8, GL_RED, GL_UNSIGNED_BYTE),
            TextureFormats.RGB8 => (GL_RGB8, GL_RGB, GL_UNSIGNED_BYTE),
            TextureFormats.RGBA8 => (GL_RGBA8, GL_RGBA, GL_UNSIGNED_BYTE),
            TextureFormats.SRGB8 => (GL_SRGB8, GL_RGB, GL_UNSIGNED_BYTE),
            TextureFormats.SRGB8_ALPHA8 => (GL_SRGB8_ALPHA8, GL_RGBA, GL_UNSIGNED_BYTE),
            TextureFormats.R16F => (GL_R16F, GL_RED, GL_HALF_FLOAT),
            TextureFormats.RG16F => (GL_RG16F, GL_RG, GL_HALF_FLOAT),
            TextureFormats.RGBA16F => (GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT),
            _ => (GL_RGBA8, GL_RGBA, GL_UNSIGNED_BYTE),
        };
        _gl.TexImage2D(GL_TEXTURE_2D, 0, internalFormat, desc.Width, desc.Height, 0, format, type, data);
        _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, desc.MinFilter switch
        {
            TextureMinFilters.Nearest => GL_NEAREST,
            TextureMinFilters.Linear => GL_LINEAR,
            TextureMinFilters.NearestMipmapNearest => GL_NEAREST_MIPMAP_NEAREST,
            TextureMinFilters.LinearMipmapNearest => GL_LINEAR_MIPMAP_NEAREST,
            TextureMinFilters.NearestMipmapLinear => GL_NEAREST_MIPMAP_LINEAR,
            TextureMinFilters.LinearMipmapLinear => GL_LINEAR_MIPMAP_LINEAR,
            _ => GL_LINEAR,
        });
        _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, desc.MagFilter switch
        {
            TextureMagFilters.Nearest => GL_NEAREST,
            TextureMagFilters.Linear => GL_LINEAR,
            _ => GL_LINEAR,
        });
        _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, desc.WrapS switch
        {
            TextureWraps.ClampToEdge => GL_CLAMP_TO_EDGE,
            TextureWraps.ClampToBorder => GL_CLAMP_TO_BORDER,
            TextureWraps.Repeat => GL_REPEAT,
            TextureWraps.MirroredRepeat => GL_MIRRORED_REPEAT,
            TextureWraps.MirrorClampToEdge => GL_MIRROR_CLAMP_TO_EDGE,
            _ => GL_REPEAT,
        });
        _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, desc.WrapT switch
        {
            TextureWraps.ClampToEdge => GL_CLAMP_TO_EDGE,
            TextureWraps.ClampToBorder => GL_CLAMP_TO_BORDER,
            TextureWraps.Repeat => GL_REPEAT,
            TextureWraps.MirroredRepeat => GL_MIRRORED_REPEAT,
            TextureWraps.MirrorClampToEdge => GL_MIRROR_CLAMP_TO_EDGE,
            _ => GL_REPEAT,
        });
        if (desc.GenerateMipmap)
            GLApiEx.GenerateMipmap(_gl, GL_TEXTURE_2D);
        DebugError();
        return id;
    }

    public FrameBuffer CreateFrameBuffer(FrameBufferDescription desc)
    {
        int fbo = _gl.GenFramebuffer();
        // _gl.GetIntegerv(GL_FRAMEBUFFER_BINDING, out int currentFbo);
        _gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
        IReadOnlyList<FrameBufferColorDescription> descs = desc.ColorDescriptions is { Count: > 0 }
            ? desc.ColorDescriptions
            : [desc.ColorDescription];
        var colorAttachmentIds = new int[descs.Count];
        var drawBuffers = new int[descs.Count];
        for (int i = 0; i < descs.Count; i++)
        {
            TextureDescription colorDesc = new TextureDescription
            {
                Width = desc.Width,
                Height = desc.Height,
                Format = descs[i].Format,
                GenerateMipmap = false,
                MinFilter = descs[i].MinFilter,
                MagFilter = descs[i].MagFilter,
                WrapS = descs[i].WrapS,
                WrapT = descs[i].WrapT,
            };
            int colorAttachmentId = CreateTexture2D(colorDesc);
            colorAttachmentIds[i] = colorAttachmentId;
            int attachment = GL_COLOR_ATTACHMENT0 + i;
            drawBuffers[i] = attachment;
            _gl.FramebufferTexture2D(GL_FRAMEBUFFER, attachment, GL_TEXTURE_2D, colorAttachmentId, 0);
        }
        int depthAttachmentId = 0;
        GLApiEx.DrawBuffers(_gl, drawBuffers);
        if (desc.DepthFormat != DepthFormats.None)
        {
            depthAttachmentId = _gl.GenRenderbuffer();
            _gl.BindRenderbuffer(GL_RENDERBUFFER, depthAttachmentId);
            _gl.RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, desc.Width, desc.Height);
            _gl.FramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, depthAttachmentId);
        }
        // _gl.BindFramebuffer(GL_FRAMEBUFFER, currentFbo);
        DebugError();
        return new FrameBuffer(desc, fbo, colorAttachmentIds, depthAttachmentId);
    }

    public bool IsFrameBufferReady()
    {
        bool ret = _gl.CheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE;
        DebugError();
        return ret;
    }

    public void SetRenderTarget(int fbo)
    {
        _gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
        DebugError();
    }

    public void SetRenderTargets(int fbo, int colorAttachmentCount)
    {
        _gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
        int count = Math.Max(1, colorAttachmentCount);
        int[] drawBuffers = new int[count];
        for (int i = 0; i < count; i++)
        {
            drawBuffers[i] = GL_COLOR_ATTACHMENT0 + i;
        }
        GLApiEx.DrawBuffers(_gl, drawBuffers);
        DebugError();
    }

    public void DeleteTexture(int id)
    {
        _gl.DeleteTexture(id);
        DebugError();
    }

    public void DeleteRenderBuffer(int id)
    {
        _gl.DeleteRenderbuffer(id);
        DebugError();
    }

    public void DeleteFrameBuffer(int id)
    {
        _gl.DeleteFramebuffer(id);
        DebugError();
    }

    public ShaderProgram? CreateProgram(string vertSrc, string fragSrc)
    {
        int shaderVertId = _gl.CreateShader(GL_VERTEX_SHADER);
        int shaderFragId = _gl.CreateShader(GL_FRAGMENT_SHADER);
        try
        {
            string? err = _gl.CompileShaderAndGetError(shaderVertId, vertSrc);
            if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"Compile VS Error: {err}");
                return null;
            }
            err = _gl.CompileShaderAndGetError(shaderFragId, fragSrc);
            if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"Compile FS Error: {err}");
                return null;
            }
            int program = _gl.CreateProgram();
            _gl.AttachShader(program, shaderVertId);
            _gl.AttachShader(program, shaderFragId);
            err = _gl.LinkProgramAndGetError(program);
            if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"Link Program Error: {err}");
                DeleteProgram(program);
                return null;
            }
            return new ShaderProgram(program, vertSrc, fragSrc);
        }
        finally
        {
            _gl.DeleteShader(shaderVertId);
            _gl.DeleteShader(shaderFragId);
            DebugError();
        }
    }

    public void ApplyShaderProgram(ShaderProgram program)
    {
        _gl.UseProgram(program.ProgramID);
        DebugError();
    }

    public void DeleteProgram(int id)
    {
        _gl.DeleteProgram(id);
        DebugError();
    }

    public void ApplyRenderState(RenderState state)
    {
        if (_currentState == null || state.DepthTestEnabled != _currentState.Value.DepthTestEnabled)
        {
            if (state.DepthTestEnabled)
                _gl.Enable(GL_DEPTH_TEST);
            else
                _gl.Disable(GL_DEPTH_TEST);
        }
        if (_currentState == null || state.DepthWriteEnabled != _currentState.Value.DepthWriteEnabled)
        {
            if (state.DepthWriteEnabled)
                _gl.DepthMask(GL_TRUE);
            else
                _gl.DepthMask(GL_FALSE);
        }
        if (_currentState == null || state.BlendingEnabled != _currentState.Value.BlendingEnabled)
        {
            if (state.BlendingEnabled)
            {
                _gl.Enable(GL_BLEND);
                if (_currentState == null || state.BlendSrcFactor != _currentState.Value.BlendSrcFactor || state.BlendDstFactor != _currentState.Value.BlendDstFactor)
                {
                    int srcFunc = state.BlendSrcFactor switch
                    {
                        BlendFactors.Zero => GL_ZERO,
                        BlendFactors.One => GL_ONE,
                        BlendFactors.SrcAlpha => GL_SRC_ALPHA,
                        BlendFactors.OneMinusSrcAlpha => GL_ONE_MINUS_SRC_ALPHA,
                        BlendFactors.SrcColor => GL_SRC_COLOR,
                        BlendFactors.OneMinusSrcColor => GL_ONE_MINUS_SRC_COLOR,
                        BlendFactors.DstAlpha => GL_DST_ALPHA,
                        BlendFactors.OneMinusDstAlpha => GL_ONE_MINUS_DST_ALPHA,
                        BlendFactors.DstColor => GL_DST_COLOR,
                        BlendFactors.OneMinusDstColor => GL_ONE_MINUS_DST_COLOR,
                        _ => GL_SRC_ALPHA,
                    };
                    int dstFunc = state.BlendDstFactor switch
                    {
                        BlendFactors.Zero => GL_ZERO,
                        BlendFactors.One => GL_ONE,
                        BlendFactors.SrcAlpha => GL_SRC_ALPHA,
                        BlendFactors.OneMinusSrcAlpha => GL_ONE_MINUS_SRC_ALPHA,
                        BlendFactors.SrcColor => GL_SRC_COLOR,
                        BlendFactors.OneMinusSrcColor => GL_ONE_MINUS_SRC_COLOR,
                        BlendFactors.DstAlpha => GL_DST_ALPHA,
                        BlendFactors.OneMinusDstAlpha => GL_ONE_MINUS_DST_ALPHA,
                        BlendFactors.DstColor => GL_DST_COLOR,
                        BlendFactors.OneMinusDstColor => GL_ONE_MINUS_DST_COLOR,
                        _ => GL_ONE_MINUS_SRC_ALPHA,
                    };
                    GLApiEx.BlendFunc(_gl, srcFunc, dstFunc);
                }
            }
            else
                _gl.Disable(GL_BLEND);
        }
        DebugError();
        _currentState = state;
    }

    public void ApplyUniform(ShaderProgram shader, string name, MaterialUniform uniform)
    {
        int location = _gl.GetUniformLocationString(shader.ProgramID, name);
        if (location < 0) return;
        switch (uniform.Type)
        {
            case UniformTypes.Int1:
                GLApiEx.Uniform1i(_gl, location, uniform.Get<int>());
                break;
            case UniformTypes.Float1:
                _gl.Uniform1f(location, uniform.Get<float>());
                break;
            case UniformTypes.Float2:
                GLApiEx.Uniform2f(_gl, location, uniform.Get<Vector2>());
                break;
            case UniformTypes.Float3:
                GLApiEx.Uniform3f(_gl, location, uniform.Get<Vector3>());
                break;
            case UniformTypes.Float4:
                GLApiEx.Uniform4f(_gl, location, uniform.Get<Vector4>());
                break;
            case UniformTypes.Matrix3x3:
                GLApiEx.UniformMatrix3fv(_gl, location, uniform.Get<Matrix3x3>());
                break;
            case UniformTypes.Int1Array:
                GLApiEx.Uniform1iv(_gl, location, uniform.Get<int[]>());
                break;
            case UniformTypes.Float1Array:
                GLApiEx.Uniform1fv(_gl, location, uniform.Get<float[]>());
                break;
            case UniformTypes.Float2Array:
                GLApiEx.Uniform2fv(_gl, location, uniform.Get<Vector2[]>());
                break;
            case UniformTypes.Float3Array:
                GLApiEx.Uniform3fv(_gl, location, uniform.Get<Vector3[]>());
                break;
            case UniformTypes.Float4Array:
                GLApiEx.Uniform4fv(_gl, location, uniform.Get<Vector4[]>());
                break;
            case UniformTypes.Matrix3x3Array:
                GLApiEx.UniformMatrix3fv(_gl, location, uniform.Get<Matrix3x3[]>());
                break;
            case UniformTypes.Texture:
                var info = uniform.Get<UniformSet.TextureInformation>();
                _gl.ActiveTexture(GL_TEXTURE0 + info.TextureSlot);
                _gl.BindTexture(GL_TEXTURE_2D, info.TextureID);
                GLApiEx.Uniform1i(_gl, location, info.TextureSlot);
                break;
            case UniformTypes.TextureArray:
                var arrInfo = uniform.Get<UniformSet.TextureArrayInformation>();
                for (int i = 0; i < arrInfo.TextureSlots.Length; i++)
                {
                    _gl.ActiveTexture(GL_TEXTURE0 + arrInfo.TextureSlots[i]);
                    _gl.BindTexture(GL_TEXTURE_2D, arrInfo.TextureIDs[i]);
                }
                GLApiEx.Uniform1iv(_gl, location, arrInfo.TextureSlots);
                break;
        }
        DebugError();
    }

    public void ApplyMaterial(ShaderProgram shader, params IReadonlyUniformSet[] uniforms)
    {
        ApplyShaderProgram(shader);
        foreach (var uniformSet in uniforms)
        {
            foreach (var pair in uniformSet)
            {
                ApplyUniform(shader, pair.Key, pair.Value);
            }
        }
        DebugError();
    }

    public VertexStream CreateVertexStream(VertexDescriptor vertDesc, int maxVertBufferBytes = 160 * 1024, int maxIndiceBufferBytes = 80 * 1024)
    {
        int vao = _gl.GenVertexArray();
        int vbo = _gl.GenBuffer();
        int ibo = _gl.GenBuffer();
        _gl.BindVertexArray(vao);
        _gl.BindBuffer(GL_ARRAY_BUFFER, vbo);
        int stride = 0;
        foreach (var attrib in vertDesc.Attributes)
        {
            _gl.VertexAttribPointer(attrib.Location, attrib.ElementCount, GL_FLOAT, attrib.Normalized ? GL_TRUE : GL_FALSE, vertDesc.SizeofVertex, stride);
            _gl.EnableVertexAttribArray(attrib.Location);
            stride += attrib.ElementCount * sizeof(float);
        }
        DebugError();
        return new VertexStream(vertDesc, vao, vbo, ibo, maxVertBufferBytes, maxIndiceBufferBytes);
    }

    public void ApplyVertexStream(VertexStream vertStream)
    {
        _gl.BindVertexArray(vertStream.VAO);
        _gl.BindBuffer(GL_ARRAY_BUFFER, vertStream.VBO);
        _gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, vertStream.IBO);
        DebugError();
    }

    public void SyncVertexStream(VertexStream vertStream)
    {
        ApplyVertexStream(vertStream);
        _gl.BufferData(GL_ARRAY_BUFFER, vertStream.UsedVertexBytes, vertStream.VertexBuffer, GL_STATIC_DRAW);
        _gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, vertStream.UsedIndiceBytes, vertStream.IndiceBuffer, GL_STATIC_DRAW);
        DebugError();
    }

    public void Draw(VertexStream vertStream, ShaderProgram shader, params IReadonlyUniformSet[] uniforms)
    {
        ApplyVertexStream(vertStream);
        ApplyMaterial(shader, uniforms);
        _gl.DrawElements(GL_TRIANGLES, vertStream.IndiceCount, GL_UNSIGNED_SHORT, 0);
        DebugError();
    }

    public void DeleteVertexArray(int id)
    {
        _gl.DeleteVertexArray(id);
        DebugError();
    }

    public void DeleteBuffer(int id)
    {
        _gl.DeleteBuffer(id);
        DebugError();
    }

    public void DeleteResource(IRenderResource res)
    {
        res.Dispose(this);
        DebugError();
    }

    void DebugError()
    {
#if DEBUG
        CheckError();
#endif
    }

    GlInterface _gl;
    RenderState? _currentState = null;
}