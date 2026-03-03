
using System.Numerics;

namespace FantaniaLib;

public class TiledLightCullingPipelineStage : IPipelineStage
{
    public string Name => "Tiled Light Culling";

    public int Order => 1000;

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2D camera)
    {
        int tileSizeValue = Math.Max(context.LightCullingTileSize, 1);
        Vector2Int tileSize = new Vector2Int(tileSizeValue, tileSizeValue);
        FrameBuffer colorBuffer = context.GetFrameBuffer(ConfigurableRenderPipeline.COLOR_BUFFER)!;
        int renderWidth = colorBuffer.Description.Width;
        int renderHeight = colorBuffer.Description.Height;
        int tilesX = Math.Max(1, (renderWidth + tileSize.X - 1) / tileSize.X);
        int tilesY = Math.Max(1, (renderHeight + tileSize.Y - 1) / tileSize.Y);
        while (tilesX * tilesY > MAX_TILES)
        {
            tileSizeValue *= 2;
            tileSize = new Vector2Int(tileSizeValue, tileSizeValue);
            tilesX = Math.Max(1, (renderWidth + tileSize.X - 1) / tileSize.X);
            tilesY = Math.Max(1, (renderHeight + tileSize.Y - 1) / tileSize.Y);
        }
        int tileCount = tilesX * tilesY;
        var tileLightLists = new List<int>?[tileCount];
        var lightPosRadius = new List<Vector4>(64);
        var lightColors = new List<Vector4>(64);
        var lightTextureIndices = new List<int>(64);
        var uniqueLightTextures = new List<TextureDefinition>(16);
        var uniqueLightTextureSlots = new List<int>(16);
        var textureIndexMap = new Dictionary<TextureDefinition, int>(16);
        int fallbackTexId = context.TextureManager.FallbackTextureID;
        TextureDefinition fallbackTexDef = TextureDefinition.CreateGpuDefinition(fallbackTexId);
        uniqueLightTextures.Add(fallbackTexDef);
        uniqueLightTextureSlots.Add(LIGHT_TEXTURE_SLOT_BASE);
        textureIndexMap[fallbackTexDef] = 0;
        float cameraZoom = MathF.Max(camera.Zoom, 0.0001f);
        foreach (IRenderable renderable in renderables)
        {
            if (renderable is not LightSource light) continue;
            LightSourceInfo lightInfo = light.LightInfo;
            Vector2 lightPosWorld = new Vector2(lightInfo.Position.X, lightInfo.Position.Y);
            Vector2 lightPosScreen = camera.WorldPositionToScreenPosition(lightPosWorld);
            float lightRadiusWorld = lightInfo.Radius;
            float lightRadiusScreen = lightRadiusWorld * cameraZoom;
            if (lightRadiusScreen <= 0.0f)
                continue;
            int minX = MathHelper.FloorToInt((lightPosScreen.X - lightRadiusScreen) / tileSize.X);
            int minY = MathHelper.FloorToInt((lightPosScreen.Y - lightRadiusScreen) / tileSize.Y);
            int maxX = MathHelper.FloorToInt((lightPosScreen.X + lightRadiusScreen) / tileSize.X);
            int maxY = MathHelper.FloorToInt((lightPosScreen.Y + lightRadiusScreen) / tileSize.Y);
            if (maxX < 0 || maxY < 0 || minX >= tilesX || minY >= tilesY)
                continue;
            minX = Math.Clamp(minX, 0, tilesX - 1);
            minY = Math.Clamp(minY, 0, tilesY - 1);
            maxX = Math.Clamp(maxX, 0, tilesX - 1);
            maxY = Math.Clamp(maxY, 0, tilesY - 1);
            int lightIndex = lightPosRadius.Count;
            if (lightIndex >= MAX_LIGHTS)
                continue;
            lightPosRadius.Add(new Vector4(lightInfo.Position.X, lightInfo.Position.Y, lightInfo.Position.Z, lightInfo.Radius));
            lightColors.Add(lightInfo.Color);
            int resolvedTexId = lightInfo.LightTextureID;
            if (resolvedTexId == 0)
                resolvedTexId = fallbackTexId;
            TextureDefinition gpuTexDef = TextureDefinition.CreateGpuDefinition(resolvedTexId);
            if (!textureIndexMap.TryGetValue(gpuTexDef, out int texIndex))
            {
                if (uniqueLightTextures.Count < MAX_LIGHT_TEXTURES)
                {
                    texIndex = uniqueLightTextures.Count;
                    textureIndexMap.Add(gpuTexDef, texIndex);
                    uniqueLightTextures.Add(gpuTexDef);
                    uniqueLightTextureSlots.Add(LIGHT_TEXTURE_SLOT_BASE + texIndex);
                }
                else
                {
                    texIndex = 0;
                }
            }
            lightTextureIndices.Add(texIndex);
            for (int tileY = minY; tileY <= maxY; tileY++)
            {
                int rowStart = tileY * tilesX;
                for (int tileX = minX; tileX <= maxX; tileX++)
                {
                    int tileId = rowStart + tileX;
                    List<int>? lightList = tileLightLists[tileId];
                    if (lightList == null)
                    {
                        lightList = new List<int>(8);
                        tileLightLists[tileId] = lightList;
                    }
                    if (lightList.Count < MAX_LIGHTS_PER_TILE)
                    {
                        lightList.Add(lightIndex);
                    }
                }
            }
        }
        int[] tileOffsets = new int[tileCount];
        int[] tileCounts = new int[tileCount];
        var packedIndices = new List<int>(tileCount * 8);
        for (int tileId = 0; tileId < tileCount; tileId++)
        {
            tileOffsets[tileId] = packedIndices.Count;
            List<int>? lightList = tileLightLists[tileId];
            if (lightList != null)
            {
                tileCounts[tileId] = Math.Min(lightList.Count, MAX_LIGHTS_PER_TILE);
                for (int i = 0; i < tileCounts[tileId]; i++)
                {
                    if (packedIndices.Count >= MAX_TILE_LIGHT_INDICES)
                        break;
                    packedIndices.Add(lightList[i]);
                }
                tileCounts[tileId] = packedIndices.Count - tileOffsets[tileId];
            }
            else
            {
                tileCounts[tileId] = 0;
            }
        }
        TiledLightCullingData cullingData = context.TiledLightCullingData;
        cullingData.TileSize = tileSize.X;
        cullingData.RenderWidth = renderWidth;
        cullingData.RenderHeight = renderHeight;
        cullingData.TilesX = tilesX;
        cullingData.TilesY = tilesY;
        cullingData.TileOffsets = tileOffsets;
        cullingData.TileCounts = tileCounts;
        cullingData.TileLightIndices = packedIndices.ToArray();
        cullingData.LightPosRadius = lightPosRadius.ToArray();
        cullingData.LightColors = lightColors.ToArray();
        cullingData.LightTextureIndices = lightTextureIndices.ToArray();
        cullingData.LightTextures = uniqueLightTextures.ToArray();
        cullingData.LightTextureSlots = uniqueLightTextureSlots.ToArray();
        context.GlobalUniforms.SetUniform("u_TileGridInfo", new Vector4(tileSize.X, tilesX, tilesY, lightPosRadius.Count));
        context.GlobalUniforms.SetUniform("u_TileOffsets", cullingData.TileOffsets);
        context.GlobalUniforms.SetUniform("u_TileCounts", cullingData.TileCounts);
        context.GlobalUniforms.SetUniform("u_TileLightIndices", cullingData.TileLightIndices);
        context.GlobalUniforms.SetUniform("u_LightPosRadius", cullingData.LightPosRadius);
        context.GlobalUniforms.SetUniform("u_LightColors", cullingData.LightColors);
        context.GlobalUniforms.SetUniform("u_LightTextureIndices", cullingData.LightTextureIndices);
        context.GlobalUniforms.SetUniform("u_LightTextures", cullingData.LightTextures, cullingData.LightTextureSlots);
    }

    const int MAX_TILES = 64;
    const int MAX_LIGHTS = 32;
    const int MAX_TILE_LIGHT_INDICES = 256;
    const int MAX_LIGHTS_PER_TILE = 16;
    const int MAX_LIGHT_TEXTURES = 8;
    const int LIGHT_TEXTURE_SLOT_BASE = 8;
}