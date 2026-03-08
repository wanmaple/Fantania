using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace FantaniaLib;

public partial class FolderBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public FolderBox()
    {
        InitializeComponent();
    }

    public async Task SelectFolder()
    {
        var top = TopLevel.GetTopLevel(AvaloniaHelper.GetTopWindow())!;
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Field!.Workspace.LocalizeString(_title),
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            string folder = folders[0].Path.AbsoluteUri;
            folder = AvaloniaHelper.ConvertAvaloniaUriToStandardUri(folder).ToStandardPath();
            Field.FieldValue = folder;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (Field != null)
        {
            string args = Field.EditInfo.EditParameter;
            if (!string.IsNullOrEmpty(args))
            {
                var argAry = args.Split(';');
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < argAry.Length; i++)
                {
                    var kv = argAry[i].Split(':');
                    if (kv.Length == 2)
                    {
                        dict[kv[0].Trim()] = kv[1].Trim();
                    }
                }
                if (dict.TryGetValue("title", out string? title))
                {
                    _title = title;
                }
                else
                {
                    _title = string.Empty;
                }
            }
        }
    }

    string _title = string.Empty;
}