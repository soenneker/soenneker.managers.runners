using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Managers.Runners.Abstract;

/// <summary>
/// Handles Runner operations and coordination
/// </summary>
public interface IRunnersManager
{
    /// <summary>
    /// Adds file at path to repo if needed.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="libraryName">The library name.</param>
    /// <param name="gitRepoUri">The git repo uri.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask AddFileAtPathToRepoIfNeeded(string filePath, string fileName, string libraryName, string gitRepoUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the push if changes needed operation.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="libraryName">The library name.</param>
    /// <param name="gitRepoUri">The git repo uri.</param>
    /// <param name="ignoreHashing">The ignore hashing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri, bool ignoreHashing = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the push if changes needed for directory operation.
    /// </summary>
    /// <param name="resourcesRelativeDir">The resources relative dir.</param>
    /// <param name="sourceDir">The source dir.</param>
    /// <param name="libraryName">The library name.</param>
    /// <param name="gitRepoUri">The git repo uri.</param>
    /// <param name="ignoreHashing">The ignore hashing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask PushIfChangesNeededForDirectory(string resourcesRelativeDir, string sourceDir, string libraryName, string gitRepoUri, bool ignoreHashing = false, CancellationToken cancellationToken = default);
}