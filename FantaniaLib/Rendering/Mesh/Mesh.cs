using System.Runtime.InteropServices;

namespace FantaniaLib;

public unsafe class Mesh : IRenderResource
{
    public MeshDescriptor Descriptor { get; private set; }

    public static Mesh From<T>(T[] verts, ushort[] indices) where T : unmanaged
    {
        VertexDescriptor vertDesc = VertexAnalyzer.GenerateDescriptor<T>();
        MeshDescriptor meshDesc = new MeshDescriptor(verts.Length, indices.Length, vertDesc);
        fixed (void* vertPtr = verts)
        {
            fixed (ushort* indicePtr = indices)
            {
                return From(meshDesc, vertPtr, indicePtr);
            }
        }
    }

    public static Mesh From(MeshDescriptor meshDesc, void* verts, ushort* indices)
    {
        var mesh = new Mesh(meshDesc);
        nuint vertSize = (nuint)(meshDesc.VertexDescriptor.SizeofVertex * meshDesc.VertexCount);
        mesh._verts = NativeMemory.Alloc(vertSize);
        NativeMemory.Copy(verts, mesh._verts, vertSize);
        nuint indiceSize = (nuint)(sizeof(ushort) * meshDesc.IndiceCount);
        mesh._indices = (ushort*)NativeMemory.Alloc(indiceSize);
        NativeMemory.Copy(indices, mesh._indices, indiceSize);
        return mesh;
    }

    private Mesh(MeshDescriptor meshDesc)
    {
        Descriptor = meshDesc;
    }

    public T GetVerticeAt<T>(int index) where T : unmanaged
    {
        T* ptr = (T*)_verts;
        return *(ptr + index);
    }

    public void SetVerticeAt<T>(int index, T vertice) where T : unmanaged
    {
        T* ptr = (T*)_verts;
        *(ptr + index) = vertice;
    }

    public void SendToVertexStream(void* vertBuffer, ushort* indiceBuffer, ushort indiceStart)
    {
        NativeMemory.Copy(_verts, vertBuffer, (nuint)(Descriptor.VertexCount * Descriptor.VertexDescriptor.SizeofVertex));
        for (int i = 0; i < Descriptor.IndiceCount; i++)
        {
            indiceBuffer[i] = (ushort)(indiceStart + _indices[i]);
        }
    }

    public void Dispose(IRenderDevice device)
    {
        NativeMemory.Free(_verts);
        NativeMemory.Free(_indices);
    }

    void* _verts;
    ushort* _indices;
}