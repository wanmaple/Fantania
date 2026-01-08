using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;

namespace FantaniaLib;

public class LogModule
{
    public IList<LogContent> Logs => _logs;

    public LogModule(int maxLogs = 1000)
    {
        _maxLogs = maxLogs;
    }

    public async Task Log(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
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
        });
    }

    public async Task LogOptional(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
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
        });
    }

    public async Task LogWarning(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
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
        });
    }

    public async Task LogError(string content)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var log = new LogContent
            {
                FontSize = 14,
                FontStyle = FontStyle.Normal,
                FontWeight = FontWeight.Medium,
                Color = Brushes.Red,
                Content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {content}",
            };
            AddLog(log);
        });
    }

    public void Clear()
    {
        _logs.Clear();
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