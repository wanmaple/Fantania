using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public class OpenGLShader : IDisposableGL, IEquatable<OpenGLShader>
{
    public static readonly OpenGLShader FullScreenTexture = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_fullscreen.vs",
        "avares://Fantania/Assets/shaders/frag_texture.fs"
    );
    public static readonly OpenGLShader FullScreenTextureBlur = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_fullscreen.vs",
        "avares://Fantania/Assets/shaders/frag_texture_blur.fs"
    );
    public static readonly OpenGLShader UnlitSprite = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_batching.vs",
        "avares://Fantania/Assets/shaders/frag_vertcolor.fs"
    );
    public static readonly OpenGLShader UnlitCurvedEdge = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_batching.vs",
        "avares://Fantania/Assets/shaders/frag_curved_edge.fs"
    );
    public static readonly OpenGLShader UnlitNoisedCurvedEdge = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_batching.vs",
        "avares://Fantania/Assets/shaders/frag_noised_curved_edge.fs"
    );
    public static readonly OpenGLShader UnlitGlowSprite = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_batching.vs",
        "avares://Fantania/Assets/shaders/frag_glow_vertcolor.fs"
    );

    // Editor Only
    public static readonly OpenGLShader Selection = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_batching.vs",
        "avares://Fantania/Assets/shaders/frag_selection.fs"
    );
    public static readonly OpenGLShader FinalBlit = new OpenGLShader(
        "avares://Fantania/Assets/shaders/vert_fullscreen.vs",
        "avares://Fantania/Assets/shaders/frag_finalblit.fs"
    );

    public bool IsValid => _valid;
    public string VertexShaderPath => _vertPath;
    public string FragmentShaderPath => _fragPath;
    public int Program => _program;

    public OpenGLShader(string vertPath, string fragPath)
    {
        _vertPath = vertPath;
        _fragPath = fragPath;
        _vertSource = IOHelper.ReadAllText(_vertPath);
        _fragSource = IOHelper.ReadAllText(_fragPath);
    }

    public void CompileAndLink(GlInterface gl)
    {
        if (_compiled) return;
        
        Dispose(gl);
        _vert = gl.CreateShader(GL_VERTEX_SHADER);
        string err = gl.CompileShaderAndGetError(_vert, _vertSource);
        if (!string.IsNullOrEmpty(err))
        {
            System.Console.WriteLine(err);
            _compiled = true;
            return;
        }
        OpenGLHelper.CheckError(gl);
        _frag = gl.CreateShader(GL_FRAGMENT_SHADER);
        err = gl.CompileShaderAndGetError(_frag, _fragSource);
        if (!string.IsNullOrEmpty(err))
        {
            System.Console.WriteLine(err);
            gl.DeleteShader(_vert);
            _vert = -1;
            _compiled = true;
            return;
        }
        _program = gl.CreateProgram();
        gl.AttachShader(_program, _vert);
        gl.AttachShader(_program, _frag);
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_POSITION, "aPosition");
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_UV, "aUV");
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_COLOR, "aColor");
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_TILING, "aTiling");
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_CUSTOM, "aCustom");
        gl.BindAttribLocationString(_program, PositionUVColor.LOCATION_CUSTOM2, "aCustom2");
        err = gl.LinkProgramAndGetError(_program);
        if (!string.IsNullOrEmpty(err))
        {
            System.Console.WriteLine(err);
            _compiled = true;
            gl.DeleteShader(_vert);
            gl.DeleteShader(_frag);
            _vert = _frag = -1;
            return;
        }
        gl.DeleteShader(_vert);
        gl.DeleteShader(_frag);
        _vert = _frag = -1;
        _compiled = true;
        _valid = true;
    }

    public void Use(GlInterface gl)
    {
        gl.UseProgram(_program);
        OpenGLHelper.CheckError(gl);
    }

    public bool Equals(OpenGLShader? other)
    {
        if (other == null) return false;
        return _vertPath == other._vertPath && _fragPath == other._fragPath;
    }

    public override int GetHashCode()
    {
        return (_vertPath.GetHashCode() * 397) ^ _fragPath.GetHashCode();
    }

    public void Dispose(GlInterface gl)
    {
        if (_vert != -1)
            gl.DeleteShader(_vert);
        if (_frag != -1)
            gl.DeleteShader(_frag);
        if (_program != -1)
            gl.DeleteProgram(_program);
    }

    string _vertPath, _fragPath;
    string _vertSource, _fragSource;
    int _vert = -1;
    int _frag = -1;
    int _program = -1;
    bool _valid;
    bool _compiled;
}