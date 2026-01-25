using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class SetupSelectionCommand : ICanvasCommand
{
    public SelectionSetups SetupType { get; private set; }

    public SetupSelectionCommand(SelectionSetups setupType)
    {
        SetupType = setupType;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        switch (SetupType)
        {
            case SelectionSetups.Begin:
            context.SelectionContext.Begin();
            break;
            case SelectionSetups.End:
            context.SelectionContext.End();
            break;
        }
    }
}