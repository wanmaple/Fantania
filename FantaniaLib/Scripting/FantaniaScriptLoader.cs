using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace FantaniaLib;

public class FantaniaScriptLoader : ScriptLoaderBase
{
    public FantaniaScriptLoader()
    {
        foreach (var asset in AvaloniaHelper.EnumerateAssetFolder(EMBEDDED_SCRIPTS_FOLDER))
        {
            string assetPath = asset.ToString();
            if (assetPath.EndsWith(".lua"))
                _embeddedResources.Add(assetPath);
        }
        ModulePaths = [
            "?.lua",
            $"{EMBEDDED_SCRIPTS_FOLDER}/?.lua"
        ];
    }

    public override object LoadFile(string file, Table globalContext)
    {
        return AvaloniaHelper.ReadAssetText(file);
    }

    public override bool ScriptFileExists(string name)
    {
        return _embeddedResources.Contains(name);
    }

    HashSet<string> _embeddedResources = new HashSet<string>(32);

    const string EMBEDDED_SCRIPTS_FOLDER = "avares://Fantania/Assets/scripts";
}