namespace FantaniaLib;

[BindingScript]
public enum PipelineHookUniformTypes
{
    Int1,
    Float1,
    Float2,
    Float3,
    Float4,
    Int1Array,
    Float1Array,
    Float2Array,
    Float3Array,
    Float4Array,
    FrameBufferColorAttachment0 = 1000,
    FrameBufferDepthAttachment = 1100,
}

public struct PipelineHookUniform
{
    public string Name;
    public PipelineHookUniformTypes Type;
    public object Value;
}

public class PipelineHook
{
    public IReadOnlyList<PipelineHookUniform> Uniforms { get; set; } = Array.Empty<PipelineHookUniform>();
}