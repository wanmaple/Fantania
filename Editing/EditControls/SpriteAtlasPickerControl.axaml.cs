using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public partial class SpriteAtlasPickerControl : UserControl
{
    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public SpriteAtlasPickerControl()
    {
        InitializeComponent();
    }

    public async Task PickSpriteAtlas()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var options = new FilePickerOpenOptions
        {
            Title = "Pick sprite atlas file.",
            AllowMultiple = false,
            FileTypeFilter = [
                new FilePickerFileType("Sprite atlas Configures") {
                    Patterns = [ "*.json", ]
                },
            ],
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files != null && files.Count > 0)
        {
            string jsonPath = files[0].Path.AbsolutePath.Replace('\\', '/');
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            string workspaceRoot = workspace.RootFolder;
            string relativePath = null;
            if (!jsonPath.StartsWith(workspaceRoot))
            {
                string jsonName = Path.GetFileName(jsonPath);
                string dstPath = Path.Combine(workspace.AtlasFolder, jsonName);
                File.Copy(jsonPath, dstPath, true);
                string imgName = Path.GetFileNameWithoutExtension(jsonName) + ".png";
                string atlasImgPath = Path.Combine(Path.GetDirectoryName(jsonPath), imgName);
                string atlasDstPath = Path.Combine(workspace.AtlasFolder, imgName);
                File.Copy(atlasImgPath, atlasDstPath, true);
                relativePath = dstPath.Substring(workspaceRoot.Length);
            }
            else
            {
                relativePath = jsonPath.Substring(workspaceRoot.Length);
            }
            EditableProperty.EditValue = relativePath;
        }
    }
}