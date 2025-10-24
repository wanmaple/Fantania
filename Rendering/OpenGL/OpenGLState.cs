namespace Fantania;

using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

public class OpenGLState : IEquatable<OpenGLState>
{
    public static readonly OpenGLState Default = new OpenGLState();
    public static readonly OpenGLState NoDepth = new OpenGLState
    {
        DepthTestEnabled = false,
        DepthWriteEnabled = false,
        BlendingEnabled = true,
    };
    public static readonly OpenGLState Blit = new OpenGLState
    {
        DepthTestEnabled = false,
        DepthWriteEnabled = false,
        BlendingEnabled = false,
    };

    public bool DepthTestEnabled { get; set; } = true;
    public bool DepthWriteEnabled { get; set; } = true;
    public bool BlendingEnabled { get; set; } = true;
    public int BlendFuncSrc { get; set; } = OpenGLApiEx.GL_SRC_ALPHA;
    public int BlendFuncDst { get; set; } = OpenGLApiEx.GL_ONE_MINUS_SRC_ALPHA;
    public bool CullingEnabled { get; set; } = false;
    public int CullingTarget { get; set; } = OpenGLApiEx.GL_BACK;

    public void Use(GlInterface gl)
    {
        if (DepthTestEnabled)
        {
            gl.Enable(GL_DEPTH_TEST);
            // gl.DepthFunc(GL_LESS);
        }
        else
            gl.Disable(GL_DEPTH_TEST);
        if (DepthWriteEnabled)
            gl.DepthMask(OpenGLApiEx.GL_TRUE);
        else
            gl.DepthMask(OpenGLApiEx.GL_FALSE);
        if (BlendingEnabled)
        {
            gl.Enable(OpenGLApiEx.GL_BLEND);
            OpenGLApiEx.BlendFunc(gl, BlendFuncSrc, BlendFuncDst);
        }
        else
            gl.Disable(OpenGLApiEx.GL_BLEND);
        if (CullingEnabled)
        {
            gl.Enable(GL_CULL_FACE);
            OpenGLApiEx.CullFace(gl, CullingTarget);
        }
        else
            gl.Disable(GL_CULL_FACE);
    }

    public OpenGLState Clone()
    {
        return new OpenGLState
        {
            DepthTestEnabled = this.DepthTestEnabled,
            DepthWriteEnabled = this.DepthWriteEnabled,
            BlendingEnabled = this.BlendingEnabled,
            BlendFuncSrc = this.BlendFuncSrc,
            BlendFuncDst = this.BlendFuncDst,
            CullingEnabled = this.CullingEnabled,
            CullingTarget = this.CullingTarget,
        };
    }

    public bool Equals(OpenGLState? other)
    {
        if (other == null) return false;
        return DepthTestEnabled == other.DepthTestEnabled;
    }

    public override int GetHashCode()
    {
        return DepthTestEnabled.GetHashCode();
    }
}