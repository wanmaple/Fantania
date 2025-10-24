using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public class FullScreenVertices
{
    public int IndiceCount => _indices.Length;

    public FullScreenVertices()
    {
        _verts[0] = new FullScreenVertice
        {
            Position = Vector2.Zero,
            UV = Vector2.Zero,
        };
        _verts[1] = new FullScreenVertice
        {
            Position = Vector2.UnitX,
            UV = Vector2.UnitX,
        };
        _verts[2] = new FullScreenVertice
        {
            Position = Vector2.One,
            UV = Vector2.One,
        };
        _verts[3] = new FullScreenVertice
        {
            Position = Vector2.UnitY,
            UV = Vector2.UnitY,
        };
        _indices[0] = (ushort)0;
        _indices[1] = (ushort)1;
        _indices[2] = (ushort)2;
        _indices[3] = (ushort)0;
        _indices[4] = (ushort)2;
        _indices[5] = (ushort)3;
    }

    public void SetUV(params Vector2[] uvs)
    {
        for (int i = 0; i < _verts.Length; i++)
        {
            if (_verts[i].UV != uvs[i])
            {
                _verts[i].UV = uvs[i];
                _dirty = true;
            }
        }
    }

    public void Prepare(GlInterface gl)
    {
        if (_vbo == -1)
        {
            _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);
            _vbo = gl.GenBuffer();
            _ibo = gl.GenBuffer();
            gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
            int vertSize = Marshal.SizeOf<FullScreenVertice>();
            gl.VertexAttribPointer(FullScreenVertice.LOCATION_POSITION, 3, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 0);
            gl.VertexAttribPointer(FullScreenVertice.LOCATION_UV, 2, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 2 * sizeof(float));
            gl.EnableVertexAttribArray(FullScreenVertice.LOCATION_POSITION);
            gl.EnableVertexAttribArray(FullScreenVertice.LOCATION_UV);
            OpenGLHelper.CheckError(gl);
        }
    }

    public unsafe void Use(GlInterface gl)
    {
        OpenGLHelper.CheckError(gl);
        gl.BindVertexArray(_vao);
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ibo);
        if (_dirty)
        {
            fixed (void* data = _verts)
                gl.BufferData(GL_ARRAY_BUFFER, _verts.Length * Marshal.SizeOf<FullScreenVertice>(), new IntPtr(data), GL_STATIC_DRAW);
            fixed (void* data = _indices)
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, _indices.Length * sizeof(ushort), new IntPtr(data), GL_STATIC_DRAW);
            OpenGLHelper.CheckError(gl);
            _dirty = false;
        }
    }

    public void Dispose(GlInterface gl)
    {
        if (_vbo != -1)
            gl.DeleteBuffer(_vbo);
        if (_ibo != -1)
            gl.DeleteBuffer(_ibo);
        if (_vao != -1)
            gl.DeleteVertexArray(_vao);
    }

    FullScreenVertice[] _verts = new FullScreenVertice[4];
    ushort[] _indices = new ushort[6];

    int _vbo = -1;
    int _ibo = -1;
    int _vao = -1;

    bool _dirty = true;
}