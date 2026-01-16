using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptablePipelineStage : ScriptableObject, IPipelineStage
{
    public string Name => GetMember("name").String;
    public int Order => (int)GetMember("order").Number;

    public ScriptablePipelineStage(DynValue table) : base(table)
    {
    }

    public void PostRender(IRenderContext context)
    {
        Call("postRender", context);
    }

    public void PreRender(IRenderContext context)
    {
        Call("preRender", context);
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables)
    {
        Call("render", context, renderables);
    }
}