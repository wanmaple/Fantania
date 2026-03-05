namespace FantaniaLib;

public class SetRenderTargetCommand : IRenderCommand
{
    public string RenderTargetName { get; set; } = string.Empty;

    public SetRenderTargetCommand(string rtName)
    {
        RenderTargetName = rtName;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        var fb = pipeline.GetFrameBuffer(RenderTargetName);
        if (fb != null)
        {
            pipeline.Device.SetRenderTargets(fb.ID, fb.ColorAttachments.Count);
            pipeline.Device.Viewport(0, 0, fb.Description.Width, fb.Description.Height);
        }
        else
            throw new InvalidOperationException($"Render target '{RenderTargetName}' not found.");
    }
}