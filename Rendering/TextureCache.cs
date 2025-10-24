using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Fantania.Models;
using Fantania.ViewModels;
using SkiaSharp;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public class TextureInfo
{
    public static readonly TextureInfo Empty = new TextureInfo
    {
        TextureID = -1,
        TextureSource = string.Empty,
        TextureWidth = 0,
        TextureHeight = 0,
    };

    public int TextureID { get; set; }
    public string TextureSource { get; set; }
    public int TextureWidth { get; set; }
    public int TextureHeight { get; set; }
}

public class TextureCache
{
    public const string WHITE = "avares://Fantania/Assets/white4x4.png";

    private static TextureCache _singleton = null;
    public static TextureCache Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = new TextureCache();
            return _singleton;
        }
    }

    public int GetTextureID(GlInterface gl, string texturePath)
    {
        var texInfo = GetTextureInfo(gl, texturePath);
        return texInfo.TextureID;
    }

    public Vector GetTextureSize(GlInterface gl, string texturePath)
    {
        var texInfo = GetTextureInfo(gl, texturePath);
        return new Vector(texInfo.TextureWidth, texInfo.TextureHeight);
    }

    public void RefreshTexture(string texturePath)
    {
        _cache.Remove(texturePath);
    }

    private TextureCache()
    {
    }

    TextureInfo GetTextureInfo(GlInterface gl, string texturePath)
    {
        if (!_cache.TryGetValue(texturePath, out TextureInfo texInfo))
        {
            texInfo = new TextureInfo();
            byte[] data = GetTextureData(texturePath, out int w, out int h);
            if (data == null) return GetTextureInfo(gl, WHITE);
            int texId = OpenGLHelper.CreateTexture2DSrgb(gl, w, h, GL_LINEAR, GL_LINEAR, OpenGLApiEx.GL_REPEAT, OpenGLApiEx.GL_REPEAT, true);
            texInfo.TextureID = texId;
            texInfo.TextureSource = texturePath;
            texInfo.TextureWidth = w;
            texInfo.TextureHeight = h;
            unsafe
            {
                fixed (void* ptr = data)
                {
                    gl.TexImage2D(GL_TEXTURE_2D, 0, OpenGLApiEx.GL_SRGB8_ALPHA8, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(ptr));
                    OpenGLApiEx.GenerateMipmap(gl, GL_TEXTURE_2D);
                }
            }
            _cache.Add(texturePath, texInfo);
        }
        return texInfo;
    }

    byte[] GetTextureData(string path, out int width, out int height)
    {
        SKBitmap bitmap = null;
        if (path.StartsWith("avares://"))
        {
            var uri = new Uri(path);
            using (var assets = AssetLoader.Open(uri))
            {
                var codec = SKCodec.Create(assets);
                var info = codec.Info;
                info.ColorType = SKColorType.Rgba8888;
                // info.ColorSpace = SKColorSpace.CreateSrgbLinear();
                bitmap = SKBitmap.Decode(codec, info);
            }
        }
        else if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                var codec = SKCodec.Create(stream);
                var info = codec.Info;
                info.ColorType = SKColorType.Rgba8888;
                // info.ColorSpace = SKColorSpace.CreateSrgbLinear();
                bitmap = SKBitmap.Decode(codec, info);
            }
        }
        else
        {
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            if (workspace != null)
            {
                string absolutePath = workspace.GetAbsolutePath(path);
                if (File.Exists(absolutePath))
                {
                    using (var stream = File.OpenRead(absolutePath))
                    {
                        var codec = SKCodec.Create(stream);
                        var info = codec.Info;
                        info.ColorType = SKColorType.Rgba8888;
                        // info.ColorSpace = SKColorSpace.CreateSrgbLinear();
                        bitmap = SKBitmap.Decode(codec, info);
                    }
                }
            }
        }
        if (bitmap != null)
        {
            width = bitmap.Width;
            height = bitmap.Height;
            return bitmap.Bytes;
        }
        width = height = 0;
        return null;
    }

    Dictionary<string, TextureInfo> _cache = new Dictionary<string, TextureInfo>();
}