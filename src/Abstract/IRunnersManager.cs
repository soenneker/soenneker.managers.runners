using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Managers.Runners.Abstract;

/// <summary>
/// Handles Runner operations and coordination
/// </summary>
public interface IRunnersManager
{
    ValueTask AddFileAtPathToRepoIfNeeded(string filePath, string fileName, string libraryName, string gitRepoUri,
        CancellationToken cancellationToken = default);
    ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri, CancellationToken cancellationToken = default);

    ValueTask PushIfChangesNeededForDirectory(string resourcesRelativeDir, string sourceDir, string libraryName, string gitRepoUri, CancellationToken cancellationToken = default);
}