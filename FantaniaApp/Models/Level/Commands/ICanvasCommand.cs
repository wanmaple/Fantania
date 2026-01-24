using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public interface ICanvasCommand
{
    void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline);
}