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
    /// <param name="filePath">Path of the file to use.</param>
    /// <param name="fileName">Name of the target file.</param>
    /// <param name="libraryName">Name of the library to load.</param>
    /// <param name="gitRepoUri">Git Repo URI for the add file at path to repo if needed operation.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the file at path to repo if needed addition is complete.</returns>
    ValueTask AddFileAtPathToRepoIfNeeded(string filePath, string fileName, string libraryName, string gitRepoUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes if Changes Needed.
    /// </summary>
    /// <param name="filePath">Path of the file to use.</param>
    /// <param name="fileName">Name of the target file.</param>
    /// <param name="libraryName">Name of the library to load.</param>
    /// <param name="gitRepoUri">Git Repo URI for the push if changes needed operation.</param>
    /// <param name="ignoreHashing">Whether ignore hashing.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the push if changes needed operation is complete.</returns>
    ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri, bool ignoreHashing = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes if Changes Needed For Directory.
    /// </summary>
    /// <param name="resourcesRelativeDir">Resources Relative Dir for the push if changes needed for directory operation.</param>
    /// <param name="sourceDir">source Dir to read or transform.</param>
    /// <param name="libraryName">Name of the library to load.</param>
    /// <param name="gitRepoUri">Git Repo URI for the push if changes needed for directory operation.</param>
    /// <param name="ignoreHashing">Whether ignore hashing.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the push if changes needed for directory operation is complete.</returns>
    ValueTask PushIfChangesNeededForDirectory(string resourcesRelativeDir, string sourceDir, string libraryName, string gitRepoUri, bool ignoreHashing = false, CancellationToken cancellationToken = default);
}
