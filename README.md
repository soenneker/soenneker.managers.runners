[![](https://img.shields.io/nuget/v/soenneker.managers.runners.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.managers.runners/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.managers.runners/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.managers.runners/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.managers.runners.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.managers.runners/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.managers.runners/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.managers.runners/actions/workflows/codeql.yml)

# Soenneker.Managers.Runners

Handles Runner operations and coordination.

> This is an automation runner, not a package intended for application consumption.

## What the runner does

- `IRunnersManager.AddFileAtPathToRepoIfNeeded(filePath, fileName, libraryName, gitRepoUri, cancellationToken)` — Adds file at path to repo if needed.
- `IRunnersManager.PushIfChangesNeeded(filePath, fileName, libraryName, gitRepoUri, ignoreHashing, cancellationToken)` — Pushes if Changes Needed.
- `IRunnersManager.PushIfChangesNeededForDirectory(resourcesRelativeDir, sourceDir, libraryName, gitRepoUri, ignoreHashing, cancellationToken)` — Pushes if Changes Needed For Directory.
- `RunnersManagerRegistrar.AddRunnersManagerAsSingleton(services)` — Adds `IRunnersManager` as a singleton service.
- `RunnersManagerRegistrar.AddRunnersManagerAsScoped(services)` — Adds `IRunnersManager` as a scoped service.

## What you get

- `IRunnersManager` — Handles Runner operations and coordination.
- `RunnersManagerRegistrar` — Handles Runner operations and coordination.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IRunnersManager.PushIfChangesNeeded(filePath, fileName, libraryName, gitRepoUri, ignoreHashing, cancellationToken)` | Pushes if Changes Needed. | A task that completes when the push if changes needed operation is complete. |
| `IRunnersManager.PushIfChangesNeededForDirectory(resourcesRelativeDir, sourceDir, libraryName, gitRepoUri, ignoreHashing, cancellationToken)` | Pushes if Changes Needed For Directory. | A task that completes when the push if changes needed for directory operation is complete. |
| `RunnersManagerRegistrar.AddRunnersManagerAsSingleton(services)` | Adds `IRunnersManager` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `RunnersManagerRegistrar.AddRunnersManagerAsScoped(services)` | Adds `IRunnersManager` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
