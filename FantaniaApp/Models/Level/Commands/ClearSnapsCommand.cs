using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class ClearSnapsCommand : ICanvasCommand
{
    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        context.Workspace.EditorModule.ClearSnapPoints();
    }
}