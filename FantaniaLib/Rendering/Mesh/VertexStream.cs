using System.Runtime.InteropServices;

namespace FantaniaLib;

public unsafe class VertexStream : IRenderResource
{
    public VertexDescriptor Descriptor { get; private set; }
    public int VAO => _vao;
    public int VBO => _vbo;
    public int IBO => _ibo;

    public int UsedVertexBytes => (int)((byte*)_vertPtr - (byte*)_streamVertex);
    public int UsedIndiceBytes => (int)((byte*)_indicePtr - (byte*)_streamIndice);
    public nint VertexBuffer => (nint)_streamVertex;
    public nint IndiceBuffer => (nint)_streamIndice;
    public int VertexCount => UsedVertexBytes / Descriptor.SizeofVertex;
    public int IndiceCount => UsedIndiceBytes / sizeof(ushort);

    public VertexStream(VertexDescriptor vertDesc, int vao, int vbo, int ibo)
    {
        Descriptor = vertDesc;
        _vao = vao;
        _vbo = vbo;
        _ibo = ibo;
        _streamVertex = NativeMemory.Alloc(MAX_VERTEX_BUFFER_BYTES);
        _streamIndice = (ushort*)NativeMemory.Alloc(MAX_INDICE_BUFFER_BYTES);
        _vertPtr = _streamVertex;
        _indicePtr = _streamIndice;
    }

    public bool TryAppend(Mesh mesh)
    {
        int meshBytes = mesh.Descriptor.VertexCount * mesh.Descriptor.VertexCount;
        int indiceBytes = mesh.Descriptor.IndiceCount * sizeof(ushort);
        if (UsedVertexBytes + meshBytes > MAX_VERTEX_BUFFER_BYTES || UsedIndiceBytes + indiceBytes > MAX_INDICE_BUFFER_BYTES)
            return false;
        ushort indiceStart = (ushort)(UsedVertexBytes / Descriptor.SizeofVertex);
        mesh.SendToVertexStream(_vertPtr, _indicePtr, indiceStart);
        _vertPtr = (byte*)_vertPtr + meshBytes;
        _indicePtr = (ushort*)((byte*)_indicePtr + indiceBytes);
        return true;
    }

    public void Reset()
    {
        _vertPtr = _streamVertex;
        _indicePtr = _streamIndice;
    }

    public void Dispose(IRenderDevice device)
    {
        device.DeleteVertexArray(_vao);
        device.DeleteBuffer(_vbo);
        device.DeleteBuffer(_ibo);
        NativeMemory.Free(_streamVertex);
        NativeMemory.Free(_streamIndice);
    }

    const int MAX_VERTEX_BUFFER_BYTES = 160 * 1024;
    const int MAX_INDICE_BUFFER_BYTES = 80 * 1024;

    void* _vertPtr;
    ushort* _indicePtr;
    void* _streamVertex;
    ushort* _streamIndice;
    int _vao, _vbo, _ibo;
}