using Microsoft.Extensions.Logging;
using Soenneker.Git.Util.Abstract;
using Soenneker.Managers.Runners.Abstract;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Extensions.String;
using Soenneker.Managers.HashChecking.Abstract;
using Soenneker.Managers.NuGetPackage.Abstract;
using Soenneker.Managers.HashSaving.Abstract;
using Soenneker.Utils.Environment;
using Soenneker.Extensions.ValueTask;
using Soenneker.GitHub.Repositories.Releases.Abstract;
using Soenneker.GitHub.Client.Abstract;
using Soenneker.Utils.Dotnet.NuGet.Abstract;

namespace Soenneker.Managers.Runners;

/// <inheritdoc cref="IRunnersManager"/>
public class RunnersManager : IRunnersManager
{
    private readonly ILogger<RunnersManager> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IHashCheckingManager _hashChecker;
    private readonly INuGetPackageManager _packageManager;
    private readonly IHashSavingManager _hashSaver;
    private readonly IGitHubRepositoriesReleasesUtil _releasesUtil;
    private readonly IGitHubClientUtil _gitHubClientUtil;
    private readonly IDotnetNuGetUtil _dotnetNuGetUtil;

    private const string _hashFilename = "hash.txt";

    public RunnersManager(
        ILogger<RunnersManager> logger,
        IGitUtil gitUtil,
        IHashCheckingManager hashChecker,
        INuGetPackageManager packageManager,
        IHashSavingManager hashSaver,
        IGitHubRepositoriesReleasesUtil releasesUtil, IGitHubClientUtil gitHubClientUtil, IDotnetNuGetUtil dotnetNuGetUtil)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _hashChecker = hashChecker;
        _packageManager = packageManager;
        _hashSaver = hashSaver;
        _releasesUtil = releasesUtil;
        _gitHubClientUtil = gitHubClientUtil;
        _dotnetNuGetUtil = dotnetNuGetUtil;
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
        string nuGetToken = EnvironmentUtil.GetVariableStrict("NUGET_TOKEN");
        string version = EnvironmentUtil.GetVariableStrict("BUILD_VERSION");
        string gitHubToken = EnvironmentUtil.GetVariableStrict("GH_TOKEN");

        // 4) Build, pack, and push if needed
        await _packageManager.BuildPackAndPushExe(gitDirectory, libraryName, targetExePath, filePath, version, nuGetToken, cancellationToken).NoSync();

        // 5) Save the new hash back into the Git repo
        await _hashSaver.SaveHashToGitRepo(gitDirectory, newHash!, fileName, _hashFilename, name, email, username, gitHubToken, cancellationToken).NoSync();

        await CreateGitHubRelease(filePath, libraryName, version, username, gitHubToken, cancellationToken).NoSync();

        await PublishToGitHubPackages(gitDirectory, libraryName, version, gitHubToken, cancellationToken).NoSync();
    }

    private async ValueTask CreateGitHubRelease(string filePath, string libraryName, string version, string username, string gitHubToken, CancellationToken cancellationToken)
    {
        // It's important that this gets called before any GitHub calls, due to setting of the token
        _ = await _gitHubClientUtil.Get(gitHubToken, cancellationToken);

        await _releasesUtil.Create(username, libraryName.ToLowerInvariantFast(),
            version, version, "Automated release update", filePath, false, false, cancellationToken).NoSync();
    }

    private async ValueTask PublishToGitHubPackages(string gitDirectory, string libraryName, string version, string gitHubToken, CancellationToken cancellationToken)
    {
        string nuGetPackagePath = Path.Combine(gitDirectory, $"{libraryName}.{version}.nupkg");

        await _dotnetNuGetUtil.Push(nuGetPackagePath, source: "https://nuget.pkg.github.com/soenneker/index.json", apiKey: gitHubToken, cancellationToken: cancellationToken).NoSync();
    }
}