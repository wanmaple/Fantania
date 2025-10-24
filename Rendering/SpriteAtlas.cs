using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class SpriteAtlas
{
    public string AtlasPath { get; set; }
    public int AtlasWidth { get; set; }
    public int AtlasHeight { get; set; }

    public SpriteAtlas(string atlasJsonPath)
    {
        Workspace workspace = WorkspaceViewModel.Current.Workspace;
        atlasJsonPath = workspace.GetAbsolutePath(atlasJsonPath);
        if (!File.Exists(atlasJsonPath))
        {
            throw new ArgumentException($"Invalid atlas path: '{atlasJsonPath}'");
        }
        string json = File.ReadAllText(atlasJsonPath);
        BuildAtlas(json, Path.GetDirectoryName(atlasJsonPath));
    }

    public Rect RandomFrame()
    {
        return RandomFrame(_random);
    }

    public Rect RandomFrame(Random random)
    {
        int idx = random.Next(_keys.Count);
        return _frameMap[_keys[idx]];
    }

    public Rect ParseFrameToUVRect(Rect frame)
    {
        return new Rect(frame.X / AtlasWidth, frame.Y / AtlasHeight, frame.Width / AtlasWidth, frame.Height / AtlasHeight);
    }

    void BuildAtlas(string json, string folder)
    {
        JsonDocument doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var frames = root.GetProperty("frames");
        var meta = root.GetProperty("meta");
        string atlasRelativePath = meta.GetProperty("image").GetString();
        var size = meta.GetProperty("size");
        AtlasPath = Path.Join(folder, atlasRelativePath);
        AtlasWidth = size.GetProperty("w").GetInt32();
        AtlasHeight = size.GetProperty("h").GetInt32();
        _frameMap = new Dictionary<string, Rect>(frames.GetArrayLength());
        _keys = new List<string>(frames.GetArrayLength());
        for (int i = 0; i < frames.GetArrayLength(); i++)
        {
            var frameData = frames[i];
            string key = frameData.GetProperty("filename").GetString();
            key = Path.GetFileNameWithoutExtension(key);
            var rect = frameData.GetProperty("frame");
            double x = rect.GetProperty("x").GetDouble();
            double y = rect.GetProperty("y").GetDouble();
            double w = rect.GetProperty("w").GetDouble();
            double h = rect.GetProperty("h").GetDouble();
            Rect frame = new Rect(x, y, w, h);
            _frameMap.Add(key, frame);
            _keys.Add(key);
        }
    }

    Dictionary<string, Rect> _frameMap;
    List<string> _keys;
    Random _random = new Random();
}