using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptablePipelineStage : ScriptableObject, IPipelineStage
{
    public string Name => GetMember("name").String;
    public int Order => (int)GetMember("order").Number;

    public ScriptablePipelineStage(DynValue table) : base(table)
    {
    }

    public void PostRender(IRenderDevice device)
    {
        Call("postRender", device);
    }

    public void PreRender(IRenderDevice device)
    {
        Call("preRender", device);
    }

    public void Render(IRenderDevice device)
    {
        Call("render", device);
    }
}