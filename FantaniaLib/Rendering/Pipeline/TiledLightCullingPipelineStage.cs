
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
        Vector2Int tileSize = new Vector2Int(context.LightCullingTileSize, context.LightCullingTileSize);
        FrameBuffer colorBuffer = context.GetFrameBuffer(ConfigurableRenderPipeline.COLOR_BUFFER)!;
        int renderWidth = colorBuffer.Description.Width;
        int renderHeight = colorBuffer.Description.Height;
        int tilesX = Math.Max(1, (renderWidth + tileSize.X - 1) / tileSize.X);
        int tilesY = Math.Max(1, (renderHeight + tileSize.Y - 1) / tileSize.Y);
        int tileCount = tilesX * tilesY;
        var tileLightLists = new List<int>?[tileCount];
        var lightPosRadius = new List<Vector4>(64);
        float cameraZoom = MathF.Max(camera.Zoom, 0.0001f);
        foreach (IRenderable renderable in renderables)
        {
            Rectf bounds = renderable.BoundingBox;
            Vector2 lightPosScreen = camera.WorldPositionToScreenPosition(bounds.Center);
            float lightRadiusWorld = MathF.Max(bounds.Width, bounds.Height) * 0.5f;
            float lightRadiusScreen = lightRadiusWorld * cameraZoom;
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
            lightPosRadius.Add(new Vector4(lightPosScreen.X, lightPosScreen.Y, lightRadiusScreen, 1.0f));
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
                tileCounts[tileId] = lightList.Count;
                packedIndices.AddRange(lightList);
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
        context.GlobalUniforms.SetUniform("u_TileGridInfo", new Vector4(tileSize.X, tilesX, tilesY, lightPosRadius.Count));
        context.GlobalUniforms.SetUniform("u_TileOffsets", cullingData.TileOffsets);
        context.GlobalUniforms.SetUniform("u_TileCounts", cullingData.TileCounts);
        context.GlobalUniforms.SetUniform("u_TileLightIndices", cullingData.TileLightIndices);
        context.GlobalUniforms.SetUniform("u_LightPosRadius", cullingData.LightPosRadius);
    }

    const int MAX_LIGHTS_PER_TILE = int.MaxValue;
}