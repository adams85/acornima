using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Nuke.Components;

[CustomGitHubActions(
        "pr",
        GitHubActionsImage.MacOsLatest,
        GitHubActionsImage.UbuntuLatest,
        GitHubActionsImage.WindowsLatest,
        OnPullRequestBranches = ["main", "master"],
        OnPullRequestIncludePaths = ["**/*"],
        PublishArtifacts = false,
        InvokedTargets = [nameof(ICompile.Compile), nameof(ITest.Test), nameof(IPack.Pack)],
        CacheKeyFiles = [],
        ConcurrencyCancelInProgress = true
    )
]
[CustomGitHubActions(
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

class CustomGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images)
    {
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        // only need to list the ones that are missing from default image
        newSteps.Insert(0, new GitHubActionsSetupDotNetStep(["10.0"]));
        // cache generated Test262 suite (keyed on settings file hash)
        newSteps.Insert(2, new GitHubActionsCacheStep(
            "test/Acornima.Tests.Test262/Generated",
            "test262-generated-${{ hashFiles('test/Acornima.Tests.Test262/Test262Harness.settings.json') }}"));
        job.Steps = newSteps.ToArray();
        return job;
    }
}

class GitHubActionsCacheStep : GitHubActionsStep
{
    public GitHubActionsCacheStep(string path, string key)
    {
        Path = path;
        Key = key;
    }

    string Path { get; }
    string Key { get; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Cache Test262 generated suite");
        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/cache@v5");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"path: {Path}");
                writer.WriteLine($"key: {Key}");
            }
        }
    }
}

class GitHubActionsSetupDotNetStep : GitHubActionsStep
{
    public GitHubActionsSetupDotNetStep(string[] versions)
    {
        Versions = versions;
    }

    string[] Versions { get; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- uses: actions/setup-dotnet@v5");

        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("dotnet-version: |");
                using (writer.Indent())
                {
                    foreach (var version in Versions)
                    {
                        writer.WriteLine(version);
                    }
                }
            }
        }
    }
}
