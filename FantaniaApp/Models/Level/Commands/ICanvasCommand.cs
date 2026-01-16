using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public interface ICanvasCommand
{
    void Execute(LevelRenderContext context, ConfigurableRenderPipeline pipeline);
}