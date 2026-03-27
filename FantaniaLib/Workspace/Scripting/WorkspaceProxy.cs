using MoonSharp.Interpreter;

namespace FantaniaLib;

[BindingScript(CanInstantiate = false)]
public class WorkspaceProxy
{
    public string RootFolder => _workspace.RootFolder;
    public IReadOnlyList<string> AllLevelNames => _workspace.LevelModule.LevelDescriptions.Select(desc => desc.Name).ToList();

    internal IWorkspace RealWorkspace => _workspace;

    public WorkspaceProxy(IWorkspace workspace)
    {
        _workspace = workspace;
    }

    public string GetAbsolutePath(params string[] pathes)
    {
        return _workspace.GetAbsolutePath(pathes);
    }

    public DynValue CreateScriptableInstance()
    {
        return _workspace.ScriptingModule.CreateScriptableInstance();
    }

    public void Log(string content)
    {
        _workspace.Log(content);
    }

    public void LogOptional(string content)
    {
        _workspace.LogOptional(content);
    }

    public void LogWarning(string content)
    {
        _workspace.LogWarning(content);
    }

    public void LogError(string content)
    {
        _workspace.LogError(content);
    }

    public void ThrowException(string content)
    {
        throw new Exception(content);
    }

    public void ClearDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
    }

    public IReadOnlyList<ExportProperty> GetExportMetadata(string lvName)
    {
        var level = _workspace.LevelModule.GetLevel(lvName);
        string metaPath = _workspace.LevelModule.GetLevelMetaPath(lvName);
        string metaContent = File.ReadAllText(metaPath);
        var syncer = new JsonDataSyncer<LevelMetadata>(level.Metadata, SerializationRule.Default);
        syncer.SyncFromJson(metaContent);
        return GetExportProperties(level.Metadata);
    }

    public IReadOnlyList<ExportEntity> GetExportEntities(string lvName)
    {
        var level = _workspace.LevelModule.GetLevel(lvName);
        var syncer = new BinaryDataSyncer<LevelEntity>(level.MutableEntities, SerializationRule.Default);
        syncer.SyncFromFile(_workspace.LevelModule.GetLevelFilePath(lvName)).GetAwaiter().GetResult();
        var ret = new List<ExportEntity>();
        foreach (var entity in level.Entities)
        {
            string eType = entity.GetType().FullName!;
            var placement = entity.GetReferencedPlacement(_workspace);
            if (placement.ID < 0)
                continue;
            IReadOnlyList<ExportProperty> eProps = GetExportProperties(entity);
            IReadOnlyList<ExportProperty> tProps = GetExportProperties(placement);
            ret.Add(new ExportEntity
            {
                EntityType = eType,
                EntityProperties = eProps,
                TemplateProperties = tProps,
            });
        }
        return ret;
    }

    public IReadOnlyList<ExportGameData> GetExportGameData()
    {
        var ret = new List<ExportGameData>();
        foreach (string group in _workspace.DatabaseModule.GameDataGroups)
        {
            var groupObjects = _workspace.DatabaseModule.GetObjectsOfGroup(group);
            foreach (var obj in groupObjects)
            {
                if (obj is not UserGameData gameData)
                    continue;
                ret.Add(new ExportGameData
                {
                    TypeName = gameData.TypeName,
                    GroupName = gameData.GroupName,
                    Properties = GetExportProperties(gameData),
                });
            }
        }
        return ret;
    }

    IReadOnlyList<ExportProperty> GetExportProperties(ISerializableData data)
    {
        var fields = data.SerializableFields;
        var ret = new List<ExportProperty>(fields.Count);
        if (data is UserPlacement placement)
        {
            ret.Add(new ExportProperty
            {
                Name = "id",
                Variant = new ExportVariant
                {
                    Type = FieldTypes.Integer,
                    Value = placement.ID,
                },
            });
        }
        else if (data is UserGameData gameData)
        {
            ret.Add(new ExportProperty
            {
                Name = "id",
                Variant = new ExportVariant
                {
                    Type = FieldTypes.Integer,
                    Value = gameData.ID,
                },
            });
        }
        foreach (var field in fields)
        {
            string name = field.FieldName;
            if (name == "Name" || name == "Tooltip") continue;
            object? value = data.GetFieldValue(name);
            ret.Add(new ExportProperty
            {
                Name = name,
                Variant = new ExportVariant
                {
                    Type = field.FieldType,
                    Value = value,
                },
            });
        }
        return ret;
    }

    IWorkspace _workspace;
}