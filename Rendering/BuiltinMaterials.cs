using System.Numerics;

namespace Fantania;

public class BuiltinMaterials
{
    public const int STANDARD_BATCHED = 0;
    public const int CURVED_EDGE = 1;
    public const int NOISED_CURVED_EDGE = 2;
    public const int GLOW_BATCHED = 3;
    public const int FULLSCREEN_SAMPLE = 10;
    public const int FULLSCREEN_SAMPLE_BLUR = 11;

    public const int SELECTION = 100;
    public const int FINAL_BLIT = 199;

    private static BuiltinMaterials _singleton = null;
    public static BuiltinMaterials Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = new BuiltinMaterials();
            return _singleton;
        }
    }
    
    public RenderMaterial this[int index]
    {
        get
        {
            return _materials[index].Clone();
        }
    }

    private BuiltinMaterials()
    {
        // for users
        var matStandardBatch = new RenderMaterial
        {
            RenderState = OpenGLState.Default,
            Shader = OpenGLShader.UnlitSprite,
        };
        _materials[STANDARD_BATCHED] = matStandardBatch;
        var matCurvedEdge = new RenderMaterial
        {
            RenderState = OpenGLState.Default,
            Shader = OpenGLShader.UnlitCurvedEdge,
        };
        _materials[CURVED_EDGE] = matCurvedEdge;
        var matNoisedCurvedEdge = new RenderMaterial
        {
            RenderState = OpenGLState.Default,
            Shader = OpenGLShader.UnlitNoisedCurvedEdge,
        };
        _materials[NOISED_CURVED_EDGE] = matNoisedCurvedEdge;
        var matGlowBatch = new RenderMaterial
        {
            RenderState = OpenGLState.NoDepth,
            Shader = OpenGLShader.UnlitGlowSprite,
        };
        _materials[GLOW_BATCHED] = matGlowBatch;
        var matFullScreenSample = new RenderMaterial
        {
            RenderState = OpenGLState.NoDepth,
            Shader = OpenGLShader.FullScreenTexture,
        };
        _materials[FULLSCREEN_SAMPLE] = matFullScreenSample;
        var matFullScreenSampleBlur = new RenderMaterial
        {
            RenderState = OpenGLState.NoDepth,
            Shader = OpenGLShader.FullScreenTextureBlur,
        };
        _materials[FULLSCREEN_SAMPLE_BLUR] = matFullScreenSampleBlur;

        // editor only
        var matSelection = new RenderMaterial
        {
            RenderState = OpenGLState.Default,
            Shader = OpenGLShader.Selection,
        };
        matSelection.SetUniform("uParams", new Vector4(16.0f, 8.0f, 0.0f, 0.0f));
        _materials[SELECTION] = matSelection;
        var matBlit = new RenderMaterial
        {
            RenderState = OpenGLState.Blit,
            Shader = OpenGLShader.FinalBlit,
        };
        _materials[FINAL_BLIT] = matBlit;
    }

    RenderMaterial[] _materials = new RenderMaterial[200];
}