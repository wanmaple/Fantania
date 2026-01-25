using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct BinaryHeaderV1
{
    public int Version;
    public int SerializedTypeCount;
    public int SerializedEntityCount;
    public int TypeSegmentOffset;
    public int TypeSegmentSize;
    public int EntitySegmentOffset;
    public int EntitySegmentSize;
}