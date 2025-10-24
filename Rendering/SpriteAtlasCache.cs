using System;
using System.Collections.Generic;
using System.IO;
using Fantania.ViewModels;

namespace Fantania;

public class SpriteAtlasCache
{
    private static SpriteAtlasCache _singleton = null;
    public static SpriteAtlasCache Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = new SpriteAtlasCache();
            return _singleton;
        }
    }

    public SpriteAtlas GetSpriteAtlas(string atlasPath)
    {
        if (!_atlasCache.TryGetValue(atlasPath, out var weakPtr) || !weakPtr.TryGetTarget(out var atlas))
        {
            var workspace = WorkspaceViewModel.Current.Workspace;
            atlasPath = workspace.GetAbsolutePath(atlasPath);
            if (!File.Exists(atlasPath))
                return null;
            atlas = new SpriteAtlas(atlasPath);
            weakPtr = new WeakReference<SpriteAtlas>(atlas);
            _atlasCache[atlasPath] = weakPtr;
        }
        return atlas;
    }

    Dictionary<string, WeakReference<SpriteAtlas>> _atlasCache = new Dictionary<string, WeakReference<SpriteAtlas>>();
}