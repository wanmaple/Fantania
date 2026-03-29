using System.Numerics;

namespace FantaniaLib;

public class ExportContext
{
    public ExportSettings? ExportSettings { get; set; } = null;

    public void ExportTo(IWorkspace workspace)
    {
        if (ExportSettings == null) return;
        if (!Directory.Exists(ExportSettings.ProjectFolder))
            throw new DirectoryNotFoundException($"Export folder '{ExportSettings.ProjectFolder}' does not exist.");
        if (!Directory.Exists(ExportSettings.SourceCodeFolder))
            throw new DirectoryNotFoundException($"Source code folder '{ExportSettings.SourceCodeFolder}' does not exist.");
        workspace.Log("Starting export...");
        List<ExportVariant> toWrite = ExportSettings.Template.GetOrCallMember("gamedataVariants", ExportSettings).ToObject<List<ExportVariant>>();
        List<string> gameDataGroups = ExportSettings.Template.GetOrCallMember("exportedGameDataGroups", ExportSettings).ToObject<List<string>>();
        Dictionary<string, int> groupMapping = new Dictionary<string, int>(gameDataGroups.Count);
        for (int i = 0; i < gameDataGroups.Count; i++)
        {
            groupMapping[gameDataGroups[i]] = i;
        }
        Dictionary<string, string> replacedEmbedded = ExportSettings.Template.GetOrCallMember("replacedEmbeddedAssets", ExportSettings).GetObjectOrDefault(new Dictionary<string, string>());
        Dictionary<string, int> strMapping = new Dictionary<string, int>();
        HashSet<string> assets = new HashSet<string>();
        int strIndex = 0;
        foreach (var variant in toWrite)
        {
            if (variant.Type == FieldTypes.String)
            {
                string str = (string)variant.Value!;
                if (str.EndsWith(".json") || str.EndsWith(".png") || str.EndsWith(".jpg") || str.EndsWith(".jpeg"))
                {
                    if (replacedEmbedded.TryGetValue(str, out string? replacement))
                    {
                        str = replacement;
                    }
                    else
                    {
                        assets.Add(str);
                    }
                    if (str.EndsWith(".json"))
                    {
                        string atlasPath = workspace.GetAbsolutePath(str);
                        try
                        {
                            SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                            if (atlas.IsValid)
                            {
                                atlasPath = workspace.GetRelativePath(atlas.AtlasPath);
                                assets.Add(atlasPath);
                            }
                        }
                        catch (Exception)
                        {
                            workspace.LogWarning("Atlas {0} is invalid and will be skipped.", str);
                        }
                    }
                }
                if (!strMapping.TryGetValue(str, out int index))
                {
                    strMapping[str] = strIndex++;
                }
            }
            else if (variant.Type == FieldTypes.StringArray)
            {
                FantaniaArray<string> arr = (FantaniaArray<string>)variant.Value!;
                for (int i = 0; i < arr.Count; i++)
                {
                    string str = arr[i];
                    if (str.EndsWith(".json") || str.EndsWith(".png") || str.EndsWith(".jpg") || str.EndsWith(".jpeg"))
                    {
                        if (replacedEmbedded.TryGetValue(str, out string? replacement))
                        {
                            str = replacement;
                        }
                        else
                        {
                            assets.Add(str);
                        }
                        if (str.EndsWith(".json"))
                        {
                            string atlasPath = workspace.GetAbsolutePath(str);
                            try
                            {
                                SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                                if (atlas.IsValid)
                                {
                                    atlasPath = workspace.GetRelativePath(atlas.AtlasPath);
                                    assets.Add(atlasPath);
                                }
                            }
                            catch (Exception)
                            {
                                workspace.LogWarning("Atlas {0} is invalid and will be skipped.", str);
                            }
                        }
                    }
                    if (!strMapping.TryGetValue(str, out int index))
                    {
                        strMapping[str] = strIndex++;
                    }
                }
            }
            else if (variant.Type == FieldTypes.Texture)
            {
                TextureDefinition texDef = (TextureDefinition)variant.Value!;
                if (texDef.TextureType == TextureTypes.Image)
                {
                    string str = texDef.TextureParameters.ImageParams.ImagePath;
                    if (replacedEmbedded.TryGetValue(str, out string? replacement))
                    {
                        str = replacement;
                    }
                    else
                    {
                        assets.Add(str);
                    }
                    if (!strMapping.TryGetValue(str, out int index))
                    {
                        strMapping[str] = strIndex++;
                    }
                }
                else if (texDef.TextureType == TextureTypes.Atlas)
                {
                    string str = texDef.TextureParameters.AtlasParams.AtlasPath;
                    if (!strMapping.TryGetValue(str, out int index))
                    {
                        strMapping[str] = strIndex++;
                    }
                    string keyStr = texDef.TextureParameters.AtlasParams.FrameKey;
                    if (!strMapping.TryGetValue(keyStr, out int keyIndex))
                    {
                        strMapping[keyStr] = strIndex++;
                    }
                    string atlasPath = workspace.GetAbsolutePath(str);
                    try
                    {
                        SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                        if (atlas.IsValid)
                        {
                            atlasPath = workspace.GetRelativePath(atlas.AtlasPath);
                            assets.Add(atlasPath);
                        }
                    }
                    catch (Exception)
                    {
                        workspace.LogWarning("Atlas {0} is invalid and will be skipped.", str);
                    }
                }
            }
            else if (variant.Type == FieldTypes.TextureArray)
            {
                FantaniaArray<TextureDefinition> arr = (FantaniaArray<TextureDefinition>)variant.Value!;
                for (int i = 0; i < arr.Count; i++)
                {
                    TextureDefinition texDef = arr[i];
                    if (texDef.TextureType == TextureTypes.Image)
                    {
                        string str = texDef.TextureParameters.ImageParams.ImagePath;
                        if (replacedEmbedded.TryGetValue(str, out string? replacement))
                        {
                            str = replacement;
                        }
                        else
                        {
                            assets.Add(str);
                        }
                        if (!strMapping.TryGetValue(str, out int index))
                        {
                            strMapping[str] = strIndex++;
                        }
                    }
                    else if (texDef.TextureType == TextureTypes.Atlas)
                    {
                        string str = texDef.TextureParameters.AtlasParams.AtlasPath;
                        if (!strMapping.TryGetValue(str, out int index))
                        {
                            strMapping[str] = strIndex++;
                        }
                        string keyStr = texDef.TextureParameters.AtlasParams.FrameKey;
                        if (!strMapping.TryGetValue(keyStr, out int keyIndex))
                        {
                            strMapping[keyStr] = strIndex++;
                        }
                        string atlasPath = workspace.GetAbsolutePath(str);
                        try
                        {
                            SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                            if (atlas.IsValid)
                            {
                                atlasPath = workspace.GetRelativePath(atlas.AtlasPath);
                                assets.Add(atlasPath);
                            }
                        }
                        catch (Exception)
                        {
                            workspace.LogWarning("Atlas {0} is invalid and will be skipped.", str);
                        }
                    }
                }
            }
            else if (variant.Type == FieldTypes.TypeReference)
            {
                TypeReference typeRef = (TypeReference)variant.Value!;
                string str = typeRef.ReferenceType;
                if (!strMapping.TryGetValue(str, out int index))
                {
                    strMapping[str] = strIndex++;
                }
            }
            else if (variant.Type == FieldTypes.TypeReferenceArray)
            {
                FantaniaArray<TypeReference> arr = (FantaniaArray<TypeReference>)variant.Value!;
                for (int i = 0; i < arr.Count; i++)
                {
                    TypeReference typeRef = arr[i];
                    string str = typeRef.ReferenceType;
                    if (!strMapping.TryGetValue(str, out int index))
                    {
                        strMapping[str] = strIndex++;
                    }
                }
            }
        }
        string gamedataFile = ExportSettings.Template.GetOrCallMember("gamedataPath", ExportSettings).GetStringOrDefault(string.Empty);
        string gamedataFolder = Path.GetDirectoryName(gamedataFile)!;
        if (!Directory.Exists(gamedataFolder))
        {
            throw new DirectoryNotFoundException($"Gamedata folder '{gamedataFolder}' does not exist.");
        }
        workspace.Log("Generating " + gamedataFile);
        using (var fs = new FileStream(gamedataFile, FileMode.Create, FileAccess.Write))
        {
            using (var bw = new BinaryWriter(fs))
            {
                foreach (var variant in toWrite)
                {
                    Write(bw, variant, strMapping, groupMapping, replacedEmbedded);
                }
            }
        }
        string assetFolder = ExportSettings.Template.GetOrCallMember("assetFolder", ExportSettings).GetStringOrDefault(string.Empty);
        if (!Directory.Exists(assetFolder))
        {
            throw new DirectoryNotFoundException($"Asset folder '{assetFolder}' does not exist.");
        }
        foreach (var asset in assets)
        {
            if (asset.StartsWith("avares://")) continue;
            string ext = Path.GetExtension(asset);
            bool isImage = ext == ".png" || ext == ".jpg" || ext == ".jpeg";
            string srcPath = workspace.GetAbsolutePath(asset);
            string dstPath = Path.Combine(assetFolder, asset);
            string folder = Path.GetDirectoryName(dstPath)!;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.Copy(srcPath, dstPath, true);
            if (isImage)
            {
                string metaPath = dstPath + ".import";
                if (!File.Exists(metaPath))
                {
                    using (var fs = new FileStream(metaPath, FileMode.Create, FileAccess.Write))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[params]");
                            sw.WriteLine();
                            sw.WriteLine("compress/mode=0");
                            sw.WriteLine("compress/high_quality=false");
                            sw.WriteLine("compress/lossy_quality=0.7");
                            sw.WriteLine("compress/uastc_level=0");
                            sw.WriteLine("compress/rdo_quality_loss=0.0");
                            sw.WriteLine("compress/hdr_compression=1");
                            sw.WriteLine("compress/normal_map=0");
                            sw.WriteLine("compress/channel_pack=0");
                            sw.WriteLine("mipmaps/generate=false");
                            sw.WriteLine("mipmaps/limit=-1");
                            sw.WriteLine("roughness/mode=0");
                            sw.WriteLine("roughness/src_normal=\"\"");
                            sw.WriteLine("process/channel_remap/red=0");
                            sw.WriteLine("process/channel_remap/green=1");
                            sw.WriteLine("process/channel_remap/blue=2");
                            sw.WriteLine("process/channel_remap/alpha=3");
                            sw.WriteLine("process/fix_alpha_border=false");
                            sw.WriteLine("process/premult_alpha=false");
                            sw.WriteLine("process/normal_map_invert_y=false");
                            sw.WriteLine("process/hdr_as_srgb=false");
                            sw.WriteLine("process/hdr_clamp_exposure=false");
                            sw.WriteLine("process/size_limit=0");
                            sw.WriteLine("detect_3d/compress_to=1");
                        }
                    }
                }
            }
        }
        List<IDRemap> placementArr = ExportSettings.Template.GetOrCallMember("beforeExportLevels", ExportSettings, strMapping).GetObjectOrDefault(new List<IDRemap>());
        string lvFolder = ExportSettings.Template.GetOrCallMember("levelFolder", ExportSettings).GetStringOrDefault(string.Empty);
        if (!Directory.Exists(lvFolder))
        {
            Directory.CreateDirectory(lvFolder);
        }
        var lvDescs = workspace.LevelModule.LevelDescriptions;
        foreach (var desc in lvDescs)
        {
            string lvName = desc.Name;
            var level = workspace.LevelModule.GetLevel(lvName);
            var syncer = new BinaryDataSyncer<LevelEntity>(level.MutableEntities, SerializationRule.Default);
            syncer.SyncFromFile(workspace.LevelModule.GetLevelFilePath(lvName)).GetAwaiter().GetResult();
            string dstFilePath = Path.Combine(lvFolder, lvName + ".lvbin");
            using (var fs = new FileStream(dstFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    int count = 0;
                    var tileMgr = new TiledEntityManager();
                    foreach (var entity in level.MutableEntities)
                    {
                        UserPlacement placement = entity.GetReferencedPlacement(workspace);
                        if (placement.ID < 0) continue;
                        if (entity is TiledEntity tiled)
                            tileMgr.AddEntity(workspace, tiled);
                        else
                            ++count;
                    }
                    // Add tile data first.
                    var groups = tileMgr.TileGroups;
                    bw.Write(groups.Count);
                    for (int i = 0; i < groups.Count; i++)
                    {
                        TileGroup group = groups[i];
                        var bounds = group.BoundingBox;
                        bw.Write(bounds.X);
                        bw.Write(bounds.Y);
                        int width = bounds.Width / group.TileSize.X;
                        int height = bounds.Height / group.TileSize.Y;
                        bw.Write(width);
                        bw.Write(height);
                        bw.Write(group.TileSize.X);
                        bw.Write(group.TileSize.Y);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                TiledEntity? e = group.GetTiledEntityAtGrid(x, y);
                                if (e == null)
                                {
                                    bw.Write((byte)0);
                                }
                                else
                                {
                                    bw.Write((byte)1);
                                    var placement = e.GetReferencedPlacement(workspace);
                                    int index = placementArr.FindIndex(x => x.Name == placement.TypeName);
                                    if (index < 0) throw new Exception("The placement is not in the entity list?");
                                    bw.Write(index);
                                    IDRemap remap = placementArr[index];
                                    bw.Write(remap.Remap[placement.ID]);
                                    int x2e = (bounds.X + x * group.TileSize.X - e.Position.X) / group.TileSize.X;
                                    int y2e = (bounds.Y + y * group.TileSize.Y - e.Position.Y) / group.TileSize.Y;
                                    Rectf uvRect = e.GetTileUVRect(workspace, x2e, y2e);
                                    bw.Write(uvRect.X);
                                    bw.Write(uvRect.Y);
                                    bw.Write(uvRect.Width);
                                    bw.Write(uvRect.Height);
                                }
                            }
                        }
                    }
                    // Write other entities
                    bw.Write(count);
                    foreach (var entity in level.MutableEntities)
                    {
                        if (entity is TiledEntity tiled) continue;
                        UserPlacement placement = entity.GetReferencedPlacement(workspace);
                        if (placement.ID < 0) continue;
                        int index = placementArr.FindIndex(x => x.Name == placement.TypeName);
                        if (index < 0) throw new Exception("The placement is not in the entity list?");
                        bw.Write(index);
                        IDRemap remap = placementArr[index];
                        bw.Write(remap.Remap[entity.PlacementReference.ReferenceID]);
                        entity.OnExport(bw);
                    }
                }
            }
        }
        GC.Collect();
        workspace.Log("Export completed.");
    }

    void Write(BinaryWriter writer, ExportVariant variant, IReadOnlyDictionary<string, int> strMapping, IReadOnlyDictionary<string, int> groupMapping, Dictionary<string, string> replacedEmbedded)
    {
        switch (variant.Type)
        {
            case FieldTypes.Boolean:
                writer.Write((bool)variant.Value!);
                break;
            case FieldTypes.Integer:
                writer.Write((int)variant.Value!);
                break;
            case FieldTypes.Float:
                writer.Write((float)variant.Value!);
                break;
            case FieldTypes.String:
                string str = (string)variant.Value!;
                int index = strMapping[str];
                writer.Write(index);
                break;
            case FieldTypes.Vector2:
                Vector2 vec2 = (Vector2)variant.Value!;
                writer.Write(vec2.X);
                writer.Write(vec2.Y);
                break;
            case FieldTypes.Vector2Int:
                Vector2Int vec2i = (Vector2Int)variant.Value!;
                writer.Write(vec2i.X);
                writer.Write(vec2i.Y);
                break;
            case FieldTypes.Vector3:
                Vector3 vec3 = (Vector3)variant.Value!;
                writer.Write(vec3.X);
                writer.Write(vec3.Y);
                writer.Write(vec3.Z);
                break;
            case FieldTypes.Color:
                Vector4 color = (Vector4)variant.Value!;
                writer.Write(color.X);
                writer.Write(color.Y);
                writer.Write(color.Z);
                writer.Write(color.W);
                break;
            case FieldTypes.Direction3D:
                Direction3D dir = (Direction3D)variant.Value!;
                Vector3 dirVec = MathHelper.AnglesToVector(dir.Azimuth, dir.Elevation);
                writer.Write(dirVec.X);
                writer.Write(dirVec.Y);
                writer.Write(dirVec.Z);
                break;
            case FieldTypes.Texture:
                TextureDefinition texDef = (TextureDefinition)variant.Value!;
                if (texDef.TextureType == TextureTypes.Image)
                {
                    writer.Write((uint)texDef.TextureType);
                    string imgPath = texDef.TextureParameters.ImageParams.ImagePath;
                    if (replacedEmbedded.TryGetValue(imgPath, out string? replacement))
                    {
                        imgPath = replacement;
                    }
                    int imgIndex = strMapping[imgPath];
                    writer.Write(imgIndex);
                    writer.Write(-1);
                }
                else if (texDef.TextureType == TextureTypes.Atlas)
                {
                    writer.Write((uint)texDef.TextureType);
                    string atlasPath = texDef.TextureParameters.AtlasParams.AtlasPath;
                    int atlasIndex = strMapping[atlasPath];
                    writer.Write(atlasIndex);
                    string frameKey = texDef.TextureParameters.AtlasParams.FrameKey;
                    int keyIndex = strMapping[frameKey];
                    writer.Write(keyIndex);
                }
                break;
            case FieldTypes.Curve:
                break;
            case FieldTypes.GroupReference:
                GroupReference groupRef = (GroupReference)variant.Value!;
                int groupIndex = groupMapping[groupRef.ReferenceGroup];
                writer.Write(groupIndex);
                writer.Write(groupRef.ReferenceID);
                break;
            case FieldTypes.TypeReference:
                TypeReference typeRef = (TypeReference)variant.Value!;
                string typeStr = typeRef.ReferenceType;
                int typeIndex = strMapping[typeStr];
                writer.Write(typeIndex);
                writer.Write(typeRef.ReferenceID);
                break;
            case FieldTypes.Enum:
                writer.Write(Convert.ToInt32(variant.Value!));
                break;
            case FieldTypes.BooleanArray:
                FantaniaArray<bool> boolArr = (FantaniaArray<bool>)variant.Value!;
                writer.Write(boolArr.Count);
                for (int i = 0; i < boolArr.Count; i++)
                {
                    writer.Write(boolArr[i]);
                }
                break;
            case FieldTypes.IntegerArray:
                FantaniaArray<int> intArr = (FantaniaArray<int>)variant.Value!;
                writer.Write(intArr.Count);
                for (int i = 0; i < intArr.Count; i++)
                {
                    writer.Write(intArr[i]);
                }
                break;
            case FieldTypes.FloatArray:
                FantaniaArray<float> floatArr = (FantaniaArray<float>)variant.Value!;
                writer.Write(floatArr.Count);
                for (int i = 0; i < floatArr.Count; i++)
                {
                    writer.Write(floatArr[i]);
                }
                break;
            case FieldTypes.StringArray:
                FantaniaArray<string> strArr = (FantaniaArray<string>)variant.Value!;
                writer.Write(strArr.Count);
                for (int i = 0; i < strArr.Count; i++)
                {
                    string s = strArr[i];
                    int sIndex = strMapping[s];
                    writer.Write(sIndex);
                }
                break;
            case FieldTypes.Vector2Array:
                FantaniaArray<Vector2> vec2Arr = (FantaniaArray<Vector2>)variant.Value!;
                writer.Write(vec2Arr.Count);
                for (int i = 0; i < vec2Arr.Count; i++)
                {
                    Vector2 v = vec2Arr[i];
                    writer.Write(v.X);
                    writer.Write(v.Y);
                }
                break;
            case FieldTypes.Vector2IntArray:
                FantaniaArray<Vector2Int> vec2iArr = (FantaniaArray<Vector2Int>)variant.Value!;
                writer.Write(vec2iArr.Count);
                for (int i = 0; i < vec2iArr.Count; i++)
                {
                    Vector2Int v = vec2iArr[i];
                    writer.Write(v.X);
                    writer.Write(v.Y);
                }
                break;
            case FieldTypes.Vector3Array:
                FantaniaArray<Vector3> vec3Arr = (FantaniaArray<Vector3>)variant.Value!;
                writer.Write(vec3Arr.Count);
                for (int i = 0; i < vec3Arr.Count; i++)
                {
                    Vector3 v = vec3Arr[i];
                    writer.Write(v.X);
                    writer.Write(v.Y);
                    writer.Write(v.Z);
                }
                break;
            case FieldTypes.ColorArray:
                FantaniaArray<Vector4> colorArr = (FantaniaArray<Vector4>)variant.Value!;
                writer.Write(colorArr.Count);
                for (int i = 0; i < colorArr.Count; i++)
                {
                    Vector4 c = colorArr[i];
                    writer.Write(c.X);
                    writer.Write(c.Y);
                    writer.Write(c.Z);
                    writer.Write(c.W);
                }
                break;
            case FieldTypes.Direction3DArray:
                FantaniaArray<Direction3D> dirArr = (FantaniaArray<Direction3D>)variant.Value!;
                writer.Write(dirArr.Count);
                for (int i = 0; i < dirArr.Count; i++)
                {
                    Direction3D d = dirArr[i];
                    Vector3 vec = MathHelper.AnglesToVector(d.Azimuth, d.Elevation);
                    writer.Write(vec.X);
                    writer.Write(vec.Y);
                    writer.Write(vec.Z);
                }
                break;
            case FieldTypes.TextureArray:
                FantaniaArray<TextureDefinition> texArr = (FantaniaArray<TextureDefinition>)variant.Value!;
                writer.Write(texArr.Count);
                for (int i = 0; i < texArr.Count; i++)
                {
                    TextureDefinition def = texArr[i];
                    writer.Write((int)def.TextureType);
                    if (def.TextureType == TextureTypes.Image)
                    {
                        string imgPath = def.TextureParameters.ImageParams.ImagePath;
                        if (replacedEmbedded.TryGetValue(imgPath, out string? replacement))
                        {
                            imgPath = replacement;
                        }
                        int imgIndex = strMapping[imgPath];
                        writer.Write(imgIndex);
                    }
                    else if (def.TextureType == TextureTypes.Atlas)
                    {
                        string atlasPath = def.TextureParameters.AtlasParams.AtlasPath;
                        int atlasIndex = strMapping[atlasPath];
                        writer.Write(atlasIndex);
                        string frameKey = def.TextureParameters.AtlasParams.FrameKey;
                        int keyIndex = strMapping[frameKey];
                        writer.Write(keyIndex);
                    }
                }
                break;
            case FieldTypes.CurveArray:
                break;
            case FieldTypes.GroupReferenceArray:
                FantaniaArray<GroupReference> groupRefArr = (FantaniaArray<GroupReference>)variant.Value!;
                writer.Write(groupRefArr.Count);
                for (int i = 0; i < groupRefArr.Count; i++)
                {
                    GroupReference gRef = groupRefArr[i];
                    int gIdx = groupMapping[gRef.ReferenceGroup];
                    writer.Write(gIdx);
                    writer.Write(gRef.ReferenceID);
                }
                break;
            case FieldTypes.TypeReferenceArray:
                FantaniaArray<TypeReference> typeRefArr = (FantaniaArray<TypeReference>)variant.Value!;
                writer.Write(typeRefArr.Count);
                for (int i = 0; i < typeRefArr.Count; i++)
                {
                    TypeReference tRef = typeRefArr[i];
                    string tStr = tRef.ReferenceType;
                    int tIdx = strMapping[tStr];
                    writer.Write(tIdx);
                    writer.Write(tRef.ReferenceID);
                }
                break;
            case FieldTypes.EnumArray:
                Array enumArr = (Array)variant.Value!;
                writer.Write(enumArr.Length);
                for (int i = 0; i < enumArr.Length; i++)
                {
                    object e = enumArr.GetValue(i)!;
                    writer.Write(Convert.ToInt32(e));
                }
                break;
            default:
                break;
        }
    }
}