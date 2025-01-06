using Microsoft.Extensions.Logging;
using Soenneker.Git.Util.Abstract;
using Soenneker.Managers.Runners.Abstract;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Managers.HashChecking.Abstract;
using Soenneker.Managers.NuGetPackage.Abstract;
using Soenneker.Managers.HashSaving.Abstract;
using Soenneker.Utils.Environment;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Managers.Runners;

/// <inheritdoc cref="IRunnersManager"/>
public class RunnersManager : IRunnersManager
{
    private readonly ILogger<RunnersManager> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IHashCheckingManager _hashChecker;
    private readonly INuGetPackageManager _packageManager;
    private readonly IHashSavingManager _hashSaver;

    private const string _hashFilename = "hash.txt";

    public RunnersManager(
        ILogger<RunnersManager> logger,
        IGitUtil gitUtil,
        IHashCheckingManager hashChecker,
        INuGetPackageManager packageManager,
        IHashSavingManager hashSaver)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _hashChecker = hashChecker;
        _packageManager = packageManager;
        _hashSaver = hashSaver;
    }

    public async ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pushing if changes are needed for {FileName} in {LibraryName} from {GitRepoUri}...", fileName, libraryName, gitRepoUri);

        // 1) Clone the Git repo
        string gitDirectory = _gitUtil.CloneToTempDirectory(gitRepoUri);

        // 2) Calculate target path
        string targetExePath = Path.Combine(gitDirectory, "src", "Resources", fileName);

        // 3) Check for hash differences
        (bool needToUpdate, string? newHash) = await _hashChecker.CheckForHashDifferences(gitDirectory, filePath, _hashFilename, cancellationToken).NoSync();

        if (!needToUpdate)
            return;

        string name = EnvironmentUtil.GetVariableStrict("NAME");
        string email = EnvironmentUtil.GetVariableStrict("EMAIL");
        string username = EnvironmentUtil.GetVariableStrict("USERNAME");
        string token = EnvironmentUtil.GetVariableStrict("TOKEN");
        string version = EnvironmentUtil.GetVariableStrict("BUILD_VERSION");
        string githubToken = EnvironmentUtil.GetVariableStrict("GH_TOKEN");

        // 4) Build, pack, and push if needed
        await _packageManager.BuildPackAndPushExe(gitDirectory, $"{libraryName}.csproj", targetExePath, filePath, version, token, cancellationToken).NoSync();

        // 5) Save the new hash back into the Git repo
        await _hashSaver.SaveHashToGitRepo(gitDirectory, newHash!, fileName, _hashFilename, name, email, username, githubToken, cancellationToken).NoSync();
    }
}