using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class MarkSceneDirtyCommand : ICanvasCommand
{
    public static MarkSceneDirtyCommand Instance { get; } = new MarkSceneDirtyCommand();

    private MarkSceneDirtyCommand() { }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        context.SceneDirty = true;
    }
}