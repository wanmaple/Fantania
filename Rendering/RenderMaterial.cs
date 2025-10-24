using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania;

public class RenderMaterial : IEquatable<RenderMaterial>
{
    private struct AdditionalTexture
    {
        public static AdditionalTexture Empty = new AdditionalTexture { texId = -1, texType = -1, uniformName = string.Empty, };

        public bool IsValid => texId > 0 && !string.IsNullOrEmpty(uniformName) && (texType == GlConsts.GL_TEXTURE_2D || texType == OpenGLApiEx.GL_TEXTURE_1D);

        public int texId;
        public int texType;
        public string uniformName;
    }

    public OpenGLState RenderState { get; set; }
    public OpenGLShader Shader { get; set; }

    public RenderMaterial()
    {
        for (int i = 0; i < MAX_EXTRA_TEXTURES; i++)
        {
            _additionalTextures[i] = AdditionalTexture.Empty;
        }
    }

    public RenderMaterial Clone()
    {
        var clone = new RenderMaterial
        {
            RenderState = this.RenderState.Clone(),
            Shader = this.Shader,
            _uniforms = new Dictionary<string, OpenGLUniform>(this._uniforms.Count),
            _mainTexId = this._mainTexId,
        };
        Array.Copy(_additionalTextures, clone._additionalTextures, MAX_EXTRA_TEXTURES);
        foreach (var pair in _uniforms)
        {
            clone._uniforms.Add(pair.Key, pair.Value.Clone());
        }
        return clone;
    }

    public bool Use(GlInterface gl)
    {
        RenderState.Use(gl);
        Shader.CompileAndLink(gl);
        if (Shader.IsValid)
        {
            Shader.Use(gl);
            int texId = _mainTexId;
            if (texId < 0)
            {
                texId = TextureCache.Singleton.GetTextureID(gl, TextureCache.WHITE);
            }
            gl.ActiveTexture(GlConsts.GL_TEXTURE0);
            gl.BindTexture(GlConsts.GL_TEXTURE_2D, texId);
            for (int i = 0; i < MAX_EXTRA_TEXTURES; i++)
            {
                AdditionalTexture additionalTex = _additionalTextures[i];
                if (!additionalTex.IsValid) continue;
                int additionalTexId = additionalTex.texId;
                int texType = additionalTex.texType;
                if (additionalTexId >= 0)
                {
                    gl.ActiveTexture(GlConsts.GL_TEXTURE0 + i + 1);
                    gl.BindTexture(texType, additionalTexId);
                }
            }
            if (_uniformDirty)
            {
                unsafe
                {
                    int texLocation = gl.GetUniformLocationString(Shader.Program, "uMainTexture");
                    OpenGLApiEx.Uniform1i(gl, texLocation, 0);
                    for (int i = 0; i < MAX_EXTRA_TEXTURES; i++)
                    {
                        AdditionalTexture additionalTex = _additionalTextures[i];
                        if (!additionalTex.IsValid) continue;
                        int loc = gl.GetUniformLocationString(Shader.Program, additionalTex.uniformName);
                        OpenGLApiEx.Uniform1i(gl, loc, i + 1);
                    }
                }
                foreach (string uniformName in _uniforms.Keys)
                {
                    unsafe
                    {
                        int location = gl.GetUniformLocationString(Shader.Program, uniformName);
                        if (location >= 0)
                        {
                            _uniforms[uniformName].Sync(gl, location);
                        }
                    }
                }
                _uniformDirty = false;
            }
            OpenGLHelper.CheckError(gl);
            return true;
        }
        return false;
    }

    public void SetMainTexture(int textureId)
    {
        _mainTexId = textureId;
        _uniformDirty = true;
    }

    public void SetAdditionalTexture(string uniformName, int slot, int textureId, int textureType = GlConsts.GL_TEXTURE_2D)
    {
        if (slot < 1 || slot > MAX_EXTRA_TEXTURES)
            throw new ArgumentOutOfRangeException("slot");
        _additionalTextures[slot - 1] = new AdditionalTexture
        {
            texId = textureId,
            texType = textureType,
            uniformName = uniformName,
        };
        _uniformDirty = true;
    }

    public void SetUniform(string uniformName, Matrix3x3 matrix)
    {
        _uniforms[uniformName] = new OpenGLUniform(OpenGLUniform.UniformType.Matrix3x3, matrix);
        _uniformDirty = true;
    }

    public void SetUniform(string uniformName, float num)
    {
        _uniforms[uniformName] = new OpenGLUniform(OpenGLUniform.UniformType.Float1, num);
        _uniformDirty = true;
    }

    public void SetUniform(string uniformName, Vector2 vec)
    {
        _uniforms[uniformName] = new OpenGLUniform(OpenGLUniform.UniformType.Float2, vec);
        _uniformDirty = true;
    }

    public void SetUniform(string uniformName, Vector3 vec)
    {
        _uniforms[uniformName] = new OpenGLUniform(OpenGLUniform.UniformType.Float3, vec);
        _uniformDirty = true;
    }

    public void SetUniform(string uniformName, Vector4 vec)
    {
        _uniforms[uniformName] = new OpenGLUniform(OpenGLUniform.UniformType.Float4, vec);
        _uniformDirty = true;
    }

    public bool Equals(RenderMaterial? other)
    {
        if (other == null) return false;
        return RenderState.Equals(other.RenderState) && Shader.Equals(other.Shader);
    }

    public override int GetHashCode()
    {
        return (RenderState.GetHashCode() * 397) ^ Shader.GetHashCode();
    }

    Dictionary<string, OpenGLUniform> _uniforms = new Dictionary<string, OpenGLUniform>(8);
    int _mainTexId = -1;
    bool _uniformDirty = true;
    AdditionalTexture[] _additionalTextures = new AdditionalTexture[MAX_EXTRA_TEXTURES];

    const int MAX_EXTRA_TEXTURES = 7;
}