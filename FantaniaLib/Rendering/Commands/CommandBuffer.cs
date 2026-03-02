using System.Collections;
using System.Numerics;
using MoonSharp.Interpreter;

namespace FantaniaLib;

[BindingScript]
public class CommandBuffer : IEnumerable<IRenderCommand>
{
    public void AddCommand(IRenderCommand cmd)
    {
        _cmds.Add(cmd);
    }

    public void ClearColor(float r, float g, float b, float a)
    {
        AddCommand(new ClearColorCommand(new Vector4(r, g, b, a)));
    }

    public void SetRenderTarget(string rtName)
    {
        AddCommand(new SetRenderTargetCommand(rtName));
    }

    public void SetupState(RenderState state)
    {
        AddCommand(new SetupStateCommand(state));
    }

    public void Draw(IEnumerable<Mesh> meshes, RenderMaterial material)
    {
        AddCommand(new DrawCommand(meshes, material));
    }

    [MoonSharpHidden]
    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        foreach (IRenderCommand cmd in _cmds)
        {
            cmd.Execute(pipeline);
        }
    }

    [MoonSharpHidden]
    public void Clear()
    {
        _cmds.Clear();
    }

    public IEnumerator<IRenderCommand> GetEnumerator()
    {
        return _cmds.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    List<IRenderCommand> _cmds = new List<IRenderCommand>(0);
}