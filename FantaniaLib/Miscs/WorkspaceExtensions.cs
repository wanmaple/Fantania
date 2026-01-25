namespace FantaniaLib;

public static class WorkspaceExtensions
{
    public static void Log(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        workspace.LogModule.Log(log);
    }

    public static void LogOptional(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        workspace.LogModule.LogOptional(log);
    }

    public static void LogWarning(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        workspace.LogModule.LogWarning(log);
    }

    public static void LogError(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        workspace.LogModule.LogError(log);
    }

    public static async Task LogAsync(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        await workspace.LogModule.LogAsync(log);
    }

    public static async Task LogOptionalAsync(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        await workspace.LogModule.LogOptionalAsync(log);
    }

    public static async Task LogWarningAsync(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        await workspace.LogModule.LogWarningAsync(log);
    }

    public static async Task LogErrorAsync(this IWorkspace workspace, string content, params object[] args)
    {
        string log = args.Length > 0 ? string.Format(content, args) : content;
        await workspace.LogModule.LogErrorAsync(log);
    }
}