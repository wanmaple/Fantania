using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Fantania.Models;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public class RenderPass
{
    public int ObjectCount => _renderableIndexes.Count;

    public int PassID { get; private set; }
    public DrawTemplate Template { get; private set; }

    public RenderPass([NotNull] DrawTemplate template, int passId)
    {
        Template = template;
        PassID = passId;
    }

    public void AddRenderable(IRenderable renderable)
    {
        int vertStart = _vertNum;
        int indiceStart = _indiceNum;
        var batchBuilder = renderable.Template.BatchBuilderOfPassId(PassID);
        ReserveSpace(batchBuilder.VertexCount, batchBuilder.IndiceCount);
        batchBuilder.OnBatching(renderable, _vertices, _indices, vertStart, indiceStart, _additionalData);
        _renderableIndexes.Add(renderable, (vertStart, indiceStart));
        _inverseMap.Add(vertStart, renderable);
        _vertNum += batchBuilder.VertexCount;
        _indiceNum += batchBuilder.IndiceCount;
    }

    public void RemoveRenderable(IRenderable renderable)
    {
        (int, int) indexes = _renderableIndexes[renderable];
        int vertStart = indexes.Item1;
        int indiceStart = indexes.Item2;
        _renderableIndexes.Remove(renderable);
        _inverseMap.Remove(vertStart);
        var batchBuilder = renderable.Template.BatchBuilderOfPassId(PassID);
        if (vertStart + batchBuilder.VertexCount != _vertNum)
        {
            // 所有renderable必定是同一类型的Mesh, 将后面的顶点按三角形的个数
            int lastVertStart = _vertNum - batchBuilder.VertexCount;
            IRenderable last = _inverseMap[lastVertStart];
            _inverseMap.Remove(lastVertStart);
            _inverseMap.Add(vertStart, last);
            _renderableIndexes[last] = (vertStart, indiceStart);
            for (int i = 0; i < batchBuilder.VertexCount; i++)
            {
                _vertices[vertStart++] = _vertices[lastVertStart++];
            }
        }
        _vertNum -= batchBuilder.VertexCount;
        _indiceNum -= batchBuilder.IndiceCount;
    }

    public void UpdateRenderable(IRenderable renderable)
    {
        if (!_renderableIndexes.ContainsKey(renderable)) return;
        (int, int) indexes = _renderableIndexes[renderable];
        var batchBuilder = renderable.Template.BatchBuilderOfPassId(PassID);
        batchBuilder.OnBatching(renderable, _vertices, _indices, indexes.Item1, indexes.Item2, _additionalData);
    }

    public void UpdateAllRenderables()
    {
        foreach (var renderable in _renderableIndexes.Keys)
        {
            UpdateRenderable(renderable);
        }
    }

    public void SetAdditionalData(string key, object value)
    {
        _additionalData[key] = value;
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
            int vertSize = Marshal.SizeOf<PositionUVColor>();
            gl.VertexAttribPointer(PositionUVColor.LOCATION_POSITION, 3, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 0);
            gl.VertexAttribPointer(PositionUVColor.LOCATION_UV, 2, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 3 * sizeof(float));
            gl.VertexAttribPointer(PositionUVColor.LOCATION_COLOR, 4, GL_FLOAT, OpenGLApiEx.GL_TRUE, vertSize, 5 * sizeof(float));
            gl.VertexAttribPointer(PositionUVColor.LOCATION_TILING, 4, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 9 * sizeof(float));
            gl.VertexAttribPointer(PositionUVColor.LOCATION_CUSTOM, 4, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 13 * sizeof(float));
            gl.VertexAttribPointer(PositionUVColor.LOCATION_CUSTOM2, 4, GL_FLOAT, OpenGLApiEx.GL_FALSE, vertSize, 17 * sizeof(float));
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_POSITION);
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_UV);
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_COLOR);
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_TILING);
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_CUSTOM);
            gl.EnableVertexAttribArray(PositionUVColor.LOCATION_CUSTOM2);
            OpenGLHelper.CheckError(gl);
        }
        _currentMaterial = Template.MaterialOfPassId(PassID);
    }

    public unsafe void UpdateVertices(GlInterface gl)
    {
        if (ObjectCount > 0)
        {
            gl.BindVertexArray(_vao);
            gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
            fixed (void* data = _vertices)
                gl.BufferData(GL_ARRAY_BUFFER, _vertNum * Marshal.SizeOf<PositionUVColor>(), new IntPtr(data), GL_STATIC_DRAW);
            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ibo);
            fixed (void* data = _indices)
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, _indiceNum * sizeof(ushort), new IntPtr(data), GL_STATIC_DRAW);
            OpenGLHelper.CheckError(gl);
        }
    }

    public unsafe void Render(GlInterface gl, Matrix3x3 viewMatrix, Vector4 resolution)
    {
        if (ObjectCount > 0)
        {
            gl.BindVertexArray(_vao);
            gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ibo);
            _currentMaterial.SetUniform("uView", viewMatrix);
            _currentMaterial.SetUniform("uResolution", resolution);
            Template.Draw(gl, this, _currentMaterial, _indiceNum);
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

    void SetupMaterial(GlInterface gl, int materialId)
    {
        _currentMaterial = BuiltinMaterials.Singleton[materialId];
    }

    void ReserveSpace(int vertNum, int indiceNum)
    {
        if (_vertNum + vertNum > _vertices.Length)
        {
            PositionUVColor[] old = _vertices;
            _vertices = new PositionUVColor[_vertices.Length * 2];
            Array.Copy(old, _vertices, _vertNum);
        }
        if (_indiceNum + indiceNum > _indices.Length)
        {
            ushort[] old = _indices;
            _indices = new ushort[_indices.Length * 2];
            Array.Copy(old, _indices, _indiceNum);
        }
    }

    PositionUVColor[] _vertices = new PositionUVColor[4 * 32];
    ushort[] _indices = new ushort[6 * 32];
    int _vertNum = 0;
    int _indiceNum = 0;
    int _lastMaterialId = -1;
    RenderMaterial _currentMaterial;

    Dictionary<IRenderable, (int, int)> _renderableIndexes = new Dictionary<IRenderable, (int, int)>(32);
    Dictionary<int, IRenderable> _inverseMap = new Dictionary<int, IRenderable>(32);
    Dictionary<string, object> _additionalData = new Dictionary<string, object>(32);

    int _vbo = -1;
    int _ibo = -1;
    int _vao = -1;
}