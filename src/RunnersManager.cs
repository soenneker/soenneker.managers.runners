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
using Soenneker.Utils.Dotnet.NuGet.Abstract;
using Soenneker.Utils.Directory.Abstract;

namespace Soenneker.Managers.Runners;

/// <inheritdoc cref="IRunnersManager"/>
public sealed class RunnersManager : IRunnersManager
{
    private readonly ILogger<RunnersManager> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IHashCheckingManager _hashChecker;
    private readonly INuGetPackageManager _packageManager;
    private readonly IHashSavingManager _hashSaver;
    private readonly IGitHubRepositoriesReleasesUtil _releasesUtil;
    private readonly IDotnetNuGetUtil _dotnetNuGetUtil;
    private readonly IDirectoryUtil _directoryUtil;

    private const string _hashFilename = "hash.txt";

    public RunnersManager(ILogger<RunnersManager> logger, IGitUtil gitUtil, IHashCheckingManager hashChecker, INuGetPackageManager packageManager,
        IHashSavingManager hashSaver, IGitHubRepositoriesReleasesUtil releasesUtil, IDotnetNuGetUtil dotnetNuGetUtil, IDirectoryUtil directoryUtil)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _hashChecker = hashChecker;
        _packageManager = packageManager;
        _hashSaver = hashSaver;
        _releasesUtil = releasesUtil;
        _dotnetNuGetUtil = dotnetNuGetUtil;
        _directoryUtil = directoryUtil;
    }

    public async ValueTask PushIfChangesNeeded(string filePath, string fileName, string libraryName, string gitRepoUri,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pushing if changes are needed for {FileName} in {LibraryName} from {GitRepoUri}...", fileName, libraryName, gitRepoUri);

        // 1) Clone the Git repo
        string gitDirectory = await _gitUtil.CloneToTempDirectory(gitRepoUri, cancellationToken).NoSync();

        // 2) Calculate target path
        string targetExePath = Path.Combine(gitDirectory, "src", "Resources", fileName);

        // 3) Check for hash differences
        (bool needToUpdate, string? newHash) = await _hashChecker.CheckForHashDifferences(gitDirectory, filePath, _hashFilename, cancellationToken).NoSync();

        if (!needToUpdate)
            return;

        string gitName = EnvironmentUtil.GetVariableStrict("GIT__NAME");
        string gitEmail = EnvironmentUtil.GetVariableStrict("GIT__EMAIL");
        string ghUsername = EnvironmentUtil.GetVariableStrict("GH__USERNAME");
        string nuGetToken = EnvironmentUtil.GetVariableStrict("NUGET__TOKEN");
        string version = EnvironmentUtil.GetVariableStrict("BUILD_VERSION");
        string gitHubToken = EnvironmentUtil.GetVariableStrict("GH__TOKEN");

        // 4) Build, pack, and push if needed
        await _packageManager.BuildPackAndPushFile(gitDirectory, libraryName, targetExePath, filePath, version, nuGetToken, cancellationToken).NoSync();

        // 5) Save the new hash back into the Git repo
        await _hashSaver.SaveHashToGitRepoAsFile(gitDirectory, newHash!, fileName, _hashFilename, gitName, gitEmail, ghUsername, gitHubToken, cancellationToken)
                        .NoSync();

        await CreateGitHubRelease(filePath, libraryName, version, ghUsername, cancellationToken).NoSync();

        await PublishToGitHubPackages(gitDirectory, libraryName, version, gitHubToken, cancellationToken).NoSync();
    }

    public async ValueTask PushIfChangesNeededForDirectory(string resourcesRelativeDir, string sourceDir, string libraryName, string gitRepoUri,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pushing if changes are needed for {resourcesRelativeDir} in {LibraryName} from {GitRepoUri}...", resourcesRelativeDir,
            libraryName, gitRepoUri);

        string gitDirectory = await _gitUtil.CloneToTempDirectory(gitRepoUri, cancellationToken).NoSync();

        string targetDir = Path.Combine(gitDirectory, "src", "Resources", resourcesRelativeDir);

        _directoryUtil.CreateIfDoesNotExist(targetDir);

        (bool needToUpdate, string? newHash) =
            await _hashChecker.CheckForHashDifferencesOfDirectory(gitDirectory, sourceDir, _hashFilename, cancellationToken).NoSync();

        if (!needToUpdate)
            return;

        string gitName = EnvironmentUtil.GetVariableStrict("GIT__NAME");
        string gitEmail = EnvironmentUtil.GetVariableStrict("GIT__EMAIL");
        string ghUsername = EnvironmentUtil.GetVariableStrict("GH__USERNAME");
        string nuGetToken = EnvironmentUtil.GetVariableStrict("NUGET__TOKEN");
        string version = EnvironmentUtil.GetVariableStrict("BUILD_VERSION");
        string gitHubToken = EnvironmentUtil.GetVariableStrict("GH__TOKEN");

        // 4) Build, pack, and push if needed
        await _packageManager.BuildPackAndPushDirectory(gitDirectory, libraryName, targetDir, sourceDir, version, nuGetToken, cancellationToken).NoSync();

        // 5) Save the new hash back into the Git repo
        await _hashSaver
              .SaveHashToGitRepoAsDirectory(gitDirectory, newHash!, targetDir, _hashFilename, gitName, gitEmail, ghUsername, gitHubToken, cancellationToken)
              .NoSync();

        await PublishToGitHubPackages(gitDirectory, libraryName, version, gitHubToken, cancellationToken).NoSync();
    }

    private ValueTask CreateGitHubRelease(string filePath, string libraryName, string version, string username, CancellationToken cancellationToken)
    {
        return _releasesUtil.Create(username, libraryName.ToLowerInvariantFast(), version, version, "Automated release update", filePath, false, false,
            cancellationToken);
    }

    private async ValueTask PublishToGitHubPackages(string gitDirectory, string libraryName, string version, string gitHubToken,
        CancellationToken cancellationToken)
    {
        string nuGetPackagePath = Path.Combine(gitDirectory, $"{libraryName}.{version}.nupkg");

        await _dotnetNuGetUtil.Push(nuGetPackagePath, source: "https://nuget.pkg.github.com/soenneker/index.json", apiKey: gitHubToken,
                                  cancellationToken: cancellationToken)
                              .NoSync();
    }
}