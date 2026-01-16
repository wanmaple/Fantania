namespace FantaniaLib;

[BindingScript]
public class MeshDescriptor
{
    public static readonly MeshDescriptor QuadStandard = new MeshDescriptor(4, 6, VertexAnalyzer.GenerateDescriptor<VertexStandard>());

    public int VertexCount { get; private set; }
    public int IndiceCount { get; private set; }
    public VertexDescriptor VertexDescriptor { get; private set; }

    public MeshDescriptor(int vertNum, int indiceNum, VertexDescriptor vertDesc)
    {
        VertexCount = vertNum;
        IndiceCount = indiceNum;
        VertexDescriptor = vertDesc;
    }
}