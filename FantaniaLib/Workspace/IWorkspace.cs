namespace FantaniaLib;

public interface IWorkspace
{
    string RootFolder { get; }
    UndoStack UndoStack { get; }
    ulong FrameCount { get; }

    DatabaseModule DatabaseModule { get; }
    PlacementModule PlacementModule { get; }
    LogModule LogModule { get; }

    string GetAbsolutePath(params string[] pathes);
}