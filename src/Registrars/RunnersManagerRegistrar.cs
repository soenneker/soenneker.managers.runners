using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Repositories.Releases.Registrars;
using Soenneker.Managers.HashChecking.Registrars;
using Soenneker.Managers.HashSaving.Registrars;
using Soenneker.Managers.NuGetPackage.Registrars;
using Soenneker.Managers.Runners.Abstract;

namespace Soenneker.Managers.Runners.Registrars;

/// <summary>
/// Handles Runner operations and coordination
/// </summary>
public static class RunnersManagerRegistrar
{
    /// <summary>
    /// Adds <see cref="IRunnersManager"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddRunnersManagerAsSingleton(this IServiceCollection services)
    {
        services.AddHashCheckingManagerAsSingleton()
            .AddHashSavingManagerAsSingleton()
            .AddNuGetPackageManagerAsSingleton()
            .AddGitHubRepositoriesReleasesUtilAsSingleton();

        services.TryAddSingleton<IRunnersManager, RunnersManager>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IRunnersManager"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddRunnersManagerAsScoped(this IServiceCollection services)
    {
        services.AddHashCheckingManagerAsScoped()
            .AddHashSavingManagerAsScoped()
            .AddNuGetPackageManagerAsScoped()
            .AddGitHubRepositoriesReleasesUtilAsScoped();

        services.TryAddScoped<IRunnersManager, RunnersManager>();

        return services;
    }
}