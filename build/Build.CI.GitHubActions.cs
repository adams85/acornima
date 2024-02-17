using Nuke.Common.CI.GitHubActions;
using Nuke.Components;

[GitHubActions(
        "pr",
        GitHubActionsImage.MacOsLatest,
        GitHubActionsImage.UbuntuLatest,
        GitHubActionsImage.WindowsLatest,
        OnPullRequestBranches = ["main", "master"],
        OnPullRequestIncludePaths = ["**/*"],
        PublishArtifacts = false,
        InvokedTargets = [nameof(ICompile.Compile), nameof(ITest.Test), nameof(IPack.Pack)],
        CacheKeyFiles = []
    )
]
[GitHubActions(
        "build",
        GitHubActionsImage.UbuntuLatest,
        OnPushBranches = ["main", "master"],
        OnPushTags = ["v*.*.*"],
        PublishArtifacts = true,
        InvokedTargets = [nameof(ICompile.Compile), nameof(ITest.Test), nameof(IPack.Pack), nameof(IPublish.Publish)],
        CacheKeyFiles = [],
        EnableGitHubToken = true,
        ImportSecrets = [nameof(FeedzNuGetApiKey), nameof(PublicNuGetApiKey)]
    )
]
public partial class Build;
