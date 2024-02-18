using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;
using Serilog;

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild, IPublish
{
    public static int Main() => Execute<Build>(x => ((ICompile) x).Compile);

    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestDirectory => RootDirectory / "test";

    string TagVersion => GitRepository.Tags.SingleOrDefault(x => x.StartsWith('v'))?[1..];
    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(TagVersion);

    string VersionSuffix;

    [Parameter]
    string Version;

    string PublicNuGetSource => "https://api.nuget.org/v3/index.json";
    string FeedzNuGetSource => "https://f.feedz.io/acornima/acornima/nuget/index.json";

    [Parameter] [Secret] readonly string PublicNuGetApiKey;
    [Parameter] [Secret] readonly string FeedzNuGetApiKey;

    bool IsPublicRelease => GitRepository.IsOnMainOrMasterBranch() && IsTaggedBuild;
    string IPublish.NuGetSource => IsPublicRelease ? PublicNuGetSource : FeedzNuGetSource;
    string IPublish.NuGetApiKey => IsPublicRelease ? PublicNuGetApiKey : FeedzNuGetApiKey;

    protected override void OnBuildInitialized()
    {
        VersionSuffix = !IsTaggedBuild
            ? $"preview-{DateTime.UtcNow:yyyyMMdd-HHmm}"
            : "";

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }
        else
        {
            if (IsTaggedBuild)
            {
                Version = TagVersion;
            }
        }

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", ((IHazConfiguration) this).Configuration);
        Log.Information("Version suffix:\t{VersionSuffix}", VersionSuffix);
        Log.Information("Version:\t\t{Version}", Version);
        Log.Information("Tagged build:\t{IsTaggedBuild}", IsTaggedBuild);
    }

    Target Clean => _ => _
        .Before<IRestore>(x => x.Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ((IHazArtifacts) this).ArtifactsDirectory.CreateOrCleanDirectory();
        });

    public IEnumerable<Project> TestProjects => ((IHazSolution) this).Solution.AllProjects.Where(x => x.Name.Contains("Tests"));


    public Configure<DotNetBuildSettings> CompileSettings => _ => _
        .SetVersion(Version)
        .SetVersionSuffix(VersionSuffix);

    public Configure<DotNetPublishSettings> PublishSettings => _ => _
        .SetVersion(Version)
        .SetVersionSuffix(VersionSuffix);

    public Configure<DotNetPackSettings> PackSettings => _ => _
        .SetVersion(Version)
        .SetVersionSuffix(VersionSuffix);
}
