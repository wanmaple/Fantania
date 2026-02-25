using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class EndResizeCommand : ICanvasCommand
{
    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        context.ResizeContext.End();
    }
}
