namespace FantaniaLib;

public interface IWorkspace
{
    string RootFolder { get; }
    WorkspaceSolution Solution { get; }
    UserTemporary UserTemporary { get; }
    UndoStack UndoStack { get; }
    ulong FrameCount { get; }

    LevelModule LevelModule { get; }
    DatabaseModule DatabaseModule { get; }
    PlacementModule PlacementModule { get; }
    EditorModule EditorModule { get; }
    LogModule LogModule { get; }
    ScriptingModule ScriptingModule { get; }

    string GetAbsolutePath(params string[] pathes);
    string LocalizeString(string content);
}