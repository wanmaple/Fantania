namespace FantaniaLib;

public interface IWorkspace
{
    string RootFolder { get; }
    UndoStack UndoStack { get; }
    ulong FrameCount { get; }

    DatabaseModule DatabaseModule { get; }
    PlacementModule PlacementModule { get; }
    EditorModule EditorModule { get; }
    LogModule LogModule { get; }
    ScriptingModule ScriptingModule { get; }

    string GetAbsolutePath(params string[] pathes);
}