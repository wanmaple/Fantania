using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public static class OpenGLHelper
{
    public static void CheckError(GlInterface gl)
    {
#if DEBUG
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
        {
            System.Console.WriteLine($"OpenGL Error: {err}");
        }
#endif
    }

    public static void CreateFrameBufferColorDepthStencil(GlInterface gl, int width, int height, out int fbo, out int colorAttachment, out int depthAttachment)
    {
        fbo = gl.GenFramebuffer();
        colorAttachment = CreateTexture2DRGBA(gl, width, height, GL_LINEAR, GL_LINEAR, OpenGLApiEx.GL_CLAMP_TO_EDGE, OpenGLApiEx.GL_CLAMP_TO_EDGE, false);
        gl.GetIntegerv(GL_FRAMEBUFFER_BINDING, out int currentFbo);
        gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
        gl.FramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorAttachment, 0);
        depthAttachment = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GL_RENDERBUFFER, depthAttachment);
        gl.RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, width, height);
        gl.FramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, depthAttachment);
        gl.BindFramebuffer(GL_FRAMEBUFFER, currentFbo);
    }

    public static void BindFrameBufferColorDepthStencil(GlInterface gl, int fbo, int colorAttachment, int depthAttachment)
    {
        gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
        gl.FramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorAttachment, 0);
        gl.FramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, depthAttachment);
    }

    public static void ResizeAttachments(GlInterface gl, int colorAttachment, int depthAttachment, int width, int height)
    {
        gl.BindTexture(GL_TEXTURE_2D, colorAttachment);
        gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, 0);
        gl.BindRenderbuffer(GL_RENDERBUFFER, depthAttachment);
        gl.RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, width, height);
    }

    public static int CreateTexture2DRGBA(GlInterface gl, int width, int height, int minFilter, int magFilter, int wrapS, int wrapT, bool genMipmap = false)
    {
        var texId = gl.GenTexture();
        gl.BindTexture(GL_TEXTURE_2D, texId);
        gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, 0);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_S, wrapS);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_T, wrapT);
        if (genMipmap)
        {
            OpenGLApiEx.GenerateMipmap(gl, GL_TEXTURE_2D);
        }
        return texId;
    }

    public static int CreateTexture2DSrgb(GlInterface gl, int width, int height, int minFilter, int magFilter, int wrapS, int wrapT, bool genMipmap = false)
    {
        var texId = gl.GenTexture();
        gl.BindTexture(GL_TEXTURE_2D, texId);
        gl.TexImage2D(GL_TEXTURE_2D, 0, OpenGLApiEx.GL_SRGB8_ALPHA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, 0);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_S, wrapS);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_T, wrapT);
        if (genMipmap)
        {
            OpenGLApiEx.GenerateMipmap(gl, GL_TEXTURE_2D);
        }
        return texId;
    }

    public static int CreateTexture2DR8(GlInterface gl, int width, int height, int minFilter, int magFilter, int wrapS, int wrapT, IntPtr data)
    {
        var texId = gl.GenTexture();
        gl.BindTexture(GL_TEXTURE_2D, texId);
        gl.TexImage2D(GL_TEXTURE_2D, 0, OpenGLApiEx.GL_R8, width, height, 0, OpenGLApiEx.GL_RED, GL_UNSIGNED_BYTE, data);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_S, wrapS);
        gl.TexParameteri(GL_TEXTURE_2D, OpenGLApiEx.GL_TEXTURE_WRAP_T, wrapT);
        return texId;
    }

    public static int CreateTexture1DR8(GlInterface gl, int length, int minFilter, int magFilter, int wrapS, IntPtr data)
    {
        var texId = gl.GenTexture();
        gl.BindTexture(OpenGLApiEx.GL_TEXTURE_1D, texId);
        OpenGLApiEx.TexImage1D(gl, OpenGLApiEx.GL_TEXTURE_1D, 0, OpenGLApiEx.GL_R8, length, OpenGLApiEx.GL_RED, GL_UNSIGNED_BYTE, data);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, GL_TEXTURE_MIN_FILTER, minFilter);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, GL_TEXTURE_MAG_FILTER, magFilter);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, OpenGLApiEx.GL_TEXTURE_WRAP_S, wrapS);
        return texId;
    }

    public static int CreateTexture1DRGBA(GlInterface gl, int length, int minFilter, int magFilter, int wrapS, IntPtr data)
    {
        var texId = gl.GenTexture();
        gl.BindTexture(OpenGLApiEx.GL_TEXTURE_1D, texId);
        OpenGLApiEx.TexImage1D(gl, OpenGLApiEx.GL_TEXTURE_1D, 0, GL_RGBA8, length, GL_RGBA, GL_UNSIGNED_BYTE, data);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, GL_TEXTURE_MIN_FILTER, minFilter);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, GL_TEXTURE_MAG_FILTER, magFilter);
        gl.TexParameteri(OpenGLApiEx.GL_TEXTURE_1D, OpenGLApiEx.GL_TEXTURE_WRAP_S, wrapS);
        return texId;
    }
}