using Fantania.Models;

namespace Fantania;

public interface IRequireBackgroundJob
{
    bool JobDirty { get; set; }

    /// <summary>
    /// Check if to queue the job only when being initialized.
    /// </summary>
    bool ShouldQueueJob(Workspace workspace);
    /// <summary>
    /// Make sure the method is thread-safe.
    /// </summary>
    void DoBackgroundJob(Workspace workspace);
    /// <summary>
    /// This method will be called on UI thread after the background job is done.
    /// </summary>
    void OnJobCompleted(Workspace workspace);
    /// <summary>
    /// This method will be called when the object is destroyed.
    /// </summary>
    void OnDestroy(Workspace workspace);
}