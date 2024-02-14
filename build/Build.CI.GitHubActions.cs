using Nuke.Common.CI.GitHubActions;
using Nuke.Components;

[GitHubActions(
        "pr",
        GitHubActionsImage.MacOsLatest,
        GitHubActionsImage.UbuntuLatest,
        GitHubActionsImage.WindowsLatest,
        OnPullRequestBranches = ["main"],
        OnPullRequestIncludePaths = ["**/*"],
        PublishArtifacts = false,
        InvokedTargets = [nameof(ICompile.Compile), nameof(ITest.Test), nameof(IPack.Pack)],
        CacheKeyFiles = []
    )
]
[GitHubActions(
        "build",
        GitHubActionsImage.MacOsLatest,
        GitHubActionsImage.UbuntuLatest,
        GitHubActionsImage.WindowsLatest,
        OnPullRequestBranches = ["main"],
        OnPullRequestIncludePaths = ["**/*"],
        PublishArtifacts = false,
        InvokedTargets = [nameof(ICompile.Compile), nameof(ITest.Test), nameof(IPack.Pack)],
        CacheKeyFiles = []
    )
]
public partial class Build;