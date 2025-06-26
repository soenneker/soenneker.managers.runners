using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Managers.Runners.Abstract;

/// <summary>
/// Handles Runner operations and coordination
/// </summary>
public interface IRunnersManager
{
    ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri, CancellationToken cancellationToken = default);

    ValueTask PushIfChangesNeededForDirectory(string directory, string sourceDir, string libraryName, string gitRepoUri, CancellationToken cancellationToken = default);
}