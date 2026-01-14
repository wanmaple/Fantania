namespace FantaniaLib;

[BindingScript]
public static class BuiltinPipelineStages
{
    public static readonly IPipelineStage Opaque = new OpaquePipelineStage();
    public static readonly IPipelineStage Transparent = new TransparentPipelineStage();
}