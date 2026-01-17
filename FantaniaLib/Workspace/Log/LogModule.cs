using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;

namespace FantaniaLib;

public class LogModule : WorkspaceModule
{
    public IList<LogContent> Logs => _logs;

    public LogModule(IWorkspace workspace, int maxLogs = 1000) : base(workspace)
    {
        _maxLogs = maxLogs;
    }

    public async Task LogAsync(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() => AddNormalLog(content));
    }

    public void Log(string content)
    {
        Dispatcher.UIThread.Invoke(() => AddNormalLog(content));
    }

    public async Task LogOptionalAsync(string content)
    {
#if DEBUG
        await Dispatcher.UIThread.InvokeAsync(() => AddOptionalLog(content));
#endif
    }

    public void LogOptional(string content)
    {
#if DEBUG
        Dispatcher.UIThread.Invoke(() => AddOptionalLog(content));
#endif
    }

    public async Task LogWarningAsync(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() => AddWarningLog(content));
    }

    public void LogWarning(string content)
    {
        Dispatcher.UIThread.Invoke(() => AddWarningLog(content));
    }

    public async Task LogErrorAsync(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() => AddErrorLog(content));
    }

    public void LogError(string content)
    {
        Dispatcher.UIThread.Invoke(() => AddErrorLog(content));
    }

    public async Task ClearAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(_logs.Clear);
    }

    public void Clear()
    {
        Dispatcher.UIThread.Invoke(_logs.Clear);
    }

    void AddNormalLog(string content)
    {
        var log = new LogContent
        {
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = FontWeight.Light,
            Color = Brushes.White,
            Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
        };
        AddLog(log);
    }

    void AddOptionalLog(string content)
    {
        var log = new LogContent
        {
            FontSize = 14,
            FontStyle = FontStyle.Italic,
            FontWeight = FontWeight.Light,
            Color = Brushes.LightGray,
            Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
        };
        AddLog(log);
    }

    void AddWarningLog(string content)
    {
        var log = new LogContent
        {
            FontSize = 14,
            FontStyle = FontStyle.Italic,
            FontWeight = FontWeight.Light,
            Color = Brushes.Yellow,
            Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
        };
        AddLog(log);
    }

    void AddErrorLog(string content)
    {
        var log = new LogContent
        {
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = FontWeight.Medium,
            Color = Brushes.IndianRed,
            Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
        };
        AddLog(log);
    }

    void AddLog(LogContent log)
    {
        if (_logs.Count == _maxLogs - 1)
        {
            _logs.RemoveAt(0);
        }
        _logs.Add(log);
    }

    ObservableCollection<LogContent> _logs = new ObservableCollection<LogContent>();
    int _maxLogs;
}