using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FantaniaLib;

public static class VertexAnalyzer
{
    public static VertexDescriptor GenerateDescriptor<T>() where T : unmanaged
    {
        Type type = typeof(T);
        if (!_cache.TryGetValue(type, out VertexDescriptor? descriptor))
        {
            var attr = type.StructLayoutAttribute;
            if (attr == null)
                throw new RenderingException("Unsupport type.");
            var attribs = new List<VertexAttribute>();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            int location = 0;
            foreach (var field in fields)
            {
#if DEBUG
                if (!SUPPORT_ATTRIBUTE_TYPES.Contains(field.FieldType))
                    throw new RenderingException($"Unsupport field type: {field.FieldType}");
#endif
                attribs.Add(new VertexAttribute
                {
                    Location = location,
                    ElementCount = Marshal.SizeOf(field.FieldType) / Marshal.SizeOf<float>(),
                    Normalized = false,
                });
                ++location;
            }
            descriptor = new VertexDescriptor(attribs);
            _cache.Add(type, descriptor);
        }
        return descriptor;
    }

    static readonly Type[] SUPPORT_ATTRIBUTE_TYPES =
    {
        typeof(float),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
    };

    static Dictionary<Type, VertexDescriptor> _cache = new Dictionary<Type, VertexDescriptor>(8);
}