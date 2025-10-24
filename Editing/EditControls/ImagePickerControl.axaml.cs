using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public partial class ImagePickerControl : UserControl
{
    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public ImagePickerControl()
    {
        InitializeComponent();
    }

    public async Task PickImage()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var options = new FilePickerOpenOptions
        {
            Title = "Pick image file.",
            AllowMultiple = false,
            FileTypeFilter = [
                FilePickerFileTypes.ImagePng,
                // new FilePickerFileType("TGA Images") {
                //     Patterns = ["*.tga",]
                // },
            ],
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files != null && files.Count > 0)
        {
            string imagePath = files[0].Path.AbsolutePath.Replace('\\', '/');
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            string workspaceRoot = workspace.RootFolder;
            string relativePath = null;
            if (!imagePath.StartsWith(workspaceRoot))
            {
                string imageName = Path.GetFileName(imagePath);
                string dstPath = Path.Combine(workspace.TextureFolder, imageName);
                File.Copy(imagePath, dstPath);
                relativePath = dstPath.Substring(workspaceRoot.Length);
            }
            else
            {
                relativePath = imagePath.Substring(workspaceRoot.Length);
            }
            EditableProperty.EditValue = relativePath;
        }
    }
}