using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class CIYamlGenerator
{
    public static string GenerateYaml(
        bool buildForWindows,
        bool buildForLinux,
        bool uploadArtifacts,
        string windowsBuildOutputName,
        string windowsArtifactName,
        string windowsBuildOutputPaths,
        string linuxBuildOutputName,
        string linuxArtifactName,
        string linuxBuildOutputPaths)
    {
        var jobs = new Dictionary<string, Job>();

        if (buildForLinux)
        {
            jobs["build-linux"] = new JobWithEnv
            {
                runs_on = "ubuntu-latest",
                env = new EnvironmentVariables
                {
                    UNITY_VERSION = "6000.0.44f1",
                    HASH = "101c91f3a8fb",
                    UNITY_EMAIL = "${{ secrets.UNITY_EMAIL }}",
                    UNITY_PASSWORD = "${{ secrets.UNITY_PASSWORD }}",
                    UNITY_LICENSE = "${{ secrets.UNITY_LICENSE }}",
                    CACHE_DIR = "$HOME/.cache/unity",
                    INSTALLER_PATH = "$HOME/.cache/unity/Unity-6000.0.44f1.tar.xz",
                    BASE_URL = "https://download.unity3d.com/download_unity",
                    UNITY_PATH = "/opt/unity/Editor/Unity"
                },
                steps = new List<Step>
                {
                    new Step { name = "Checkout repo", uses = "actions/checkout@v4" },
                    new Step { name = "Prepare cache directory", run = "mkdir -p ~/.cache/unity" },
                    new Step
                    {
                        name = "Cache Unity installation",
                        uses = "actions/cache@v3",
                        with = new Dictionary<string, string>
                        {
                            { "path", "~/.cache/unity" },
                            { "key", "${{ runner.os }}-unity-${{ env.UNITY_VERSION }}" }
                        }
                    },
                    new Step
                    {
                        name = "Download Unity Installer",
                        run = @"
set -e

echo ""📅 Downloading Unity Editor""

if [ ! -f ""${{ env.INSTALLER_PATH }}"" ]; then
  wget -O ""${{ env.INSTALLER_PATH }}"" ""${{ env.BASE_URL }}/${{ env.HASH }}/LinuxEditorInstaller/Unity-${{ env.UNITY_VERSION }}.tar.xz""
else
  echo ""✅ Found Unity installer in cache.""
fi"
                    },
                    new Step
                    {
                        name = "Install Unity Editor",
                        run = @"
set -e

echo ""📦 Installing Unity Editor""

sudo mkdir -p /opt/unity
sudo tar -xJf ""${{ env.INSTALLER_PATH }}"" -C /opt/unity
echo ""✅ Unity installed to /opt/unity"""
                    },
                    new Step
                    {
                        name = "Activate Unity License (Pro)",
                        run = @"
${{ env.UNITY_PATH }} \
  -batchmode -nographics -quit \
  -logFile - \
  -serial ""${{ secrets.UNITY_LICENSE }}"" \
  -username ""${{ secrets.UNITY_EMAIL }}"" \
  -password ""${{ secrets.UNITY_PASSWORD }}"""
                    },
                    new Step
                    {
                        name = "Build Project",
                        run = $@"
${{{{ env.UNITY_PATH }}}} -batchmode -nographics -quit \
  -projectPath Test/QualityBuddyDev \
  -buildTarget StandaloneLinux64 \
  -buildLinux64Player {linuxBuildOutputName} \
  -logFile /dev/stdout"
                    },
                    new Step
                    {
                        name = "Check Unity Build Output",
                        run = @"
echo ""PWD: $(pwd)""
ls -la ""$(pwd)/Test/QualityBuddyDev""
find ""$(pwd)/Test/QualityBuddyDev"" -type f"
                    },
                    new Step
                    {
                        name = "Upload Linux Build",
                        uses = "actions/upload-artifact@v4",
                        with = uploadArtifacts ? new Dictionary<string, string>
                        {
                            { "name", linuxArtifactName },
                            { "path", linuxBuildOutputPaths }
                        } : null
                    },
                    new Step
                    {
                        name = "Deactivate Unity License",
                        if_condition = "always()",
                        run = @"
${{ env.UNITY_PATH }} -batchmode -nographics -returnlicense -logFile /dev/stdout || true"
                    }
                }
            };
        }

        if (buildForWindows)
        {
            jobs["build-windows"] = new JobWithEnv
            {
                runs_on = "windows-latest",
                needs = buildForLinux ? new List<string> { "build-linux" } : null,
                env = new EnvironmentVariables
                {
                    UNITY_VERSION = "6000.0.44f1",
                    HASH = "101c91f3a8fb",
                    UNITY_EMAIL = "${{ secrets.UNITY_EMAIL }}",
                    UNITY_PASSWORD = "${{ secrets.UNITY_PASSWORD }}",
                    UNITY_LICENSE = "${{ secrets.UNITY_LICENSE }}",
                    BASE_URL = "https://download.unity3d.com/download_unity",
                    INSTALLER_PATH = "C:\\Unity\\cache\\UnitySetup64.exe",
                    UNITY_PATH = "C:\\Unity\\Editor\\Unity.exe",
                    CACHE_DIR = "C:\\Unity\\cache"
                },
                steps = new List<Step>
                {
                    new Step { name = "Checkout repo", uses = "actions/checkout@v4" },
                    new Step
                    {
                        name = "Cache Unity installer",
                        uses = "actions/cache@v3",
                        with = new Dictionary<string, string>
                        {
                            { "path", "${{ env.CACHE_DIR }}" },
                            { "key", "${{ runner.os }}-unity-${{ env.UNITY_VERSION }}" }
                        }
                    },
                    new Step
                    {
                        name = "Create Unity directories",
                        run = @"
$directories = @(
  ""C:\Users\runneradmin\AppData\Local\Unity\Caches"",
  ""C:\Users\runneradmin\AppData\Local\Unity"",
  ""C:\ProgramData\Unity"",
  ""C:\ProgramData\Unity\config"",
  ""C:\UnityTemp"",
  ""${{ env.CACHE_DIR }}""
)
foreach ($dir in $directories) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  Write-Host ""Created directory: $dir""
}"
                    },
                    new Step
                    {
                        name = "Download Unity Installer",
                        run = @"
$ErrorActionPreference = ""Stop""
New-Item -ItemType Directory -Force -Path ""C:\Unity"" | Out-Null
if (-Not (Test-Path ""${env:INSTALLER_PATH}"")) {
  Write-Host ""Downloading ${env:BASE_URL}/${env:HASH}/Windows64EditorInstaller/UnitySetup64.exe""
  Invoke-WebRequest ""${env:BASE_URL}/${env:HASH}/Windows64EditorInstaller/UnitySetup64.exe"" -OutFile ""${env:INSTALLER_PATH}""
} else {
  Write-Host ""✅ Found Unity installer in cache.""
}"
                    },
                    new Step
                    {
                        name = "Install Unity Editor",
                        run = @"
Start-Process -FilePath ""${env:INSTALLER_PATH}"" -ArgumentList ""/S /D=C:\Unity"" -Wait"
                    },
                    new Step
                    {
                        name = "Activate Unity License (Pro)",
                        run = @"
$logFile = ""C:\UnityTemp\unity-license.log""
$unityArgs = @(
  ""-batchmode"",
  ""-nographics"",
  ""-quit"",
  ""-logFile"", ""$logFile"",
  ""-serial"", ""$env:UNITY_LICENSE"",
  ""-username"", ""$env:UNITY_EMAIL"",
  ""-password"", ""$env:UNITY_PASSWORD""
)
$process = Start-Process -FilePath ""$env:UNITY_PATH"" -ArgumentList $unityArgs -Wait -PassThru
$exitCode = $process.ExitCode
Write-Host ""Unity exited with code: $exitCode""
Write-Host ""========== Unity License Log ==========""
Get-Content $logFile | Write-Host
if ($exitCode -ne 0) {
  Write-Host ""❌ License activation failed.""
}"
                    },
                    new Step
                    {
                        name = "Build Project",
                        run = $@"
$logFile = ""C:\UnityTemp\unity-build.log""
$unityArgs = @(
  ""-batchmode"",
  ""-nographics"",
  ""-quit"",
  ""-logFile"", ""$logFile"",
  ""-projectPath"", ""Test/QualityBuddyDev"",
  ""-buildWindows64Player"", ""{windowsBuildOutputName}""
)
$process = Start-Process -FilePath ""$env:UNITY_PATH"" -ArgumentList $unityArgs -Wait -PassThru
$exitCode = $process.ExitCode
Write-Host ""Unity exited with code: $exitCode""
Write-Host ""========== Unity Build Log ==========""
Get-Content $logFile | Write-Host
if ($exitCode -ne 0) {{
  Write-Host ""❌ Build failed.""
}}"
                    },
                    new Step
                    {
                        name = "Check Unity Build Output",
                        run = @"
Write-Host ""PWD: $(Get-Location)""
Get-ChildItem -Path ""$pwd\Test\QualityBuddyDev\build"" -Recurse -Force -ErrorAction SilentlyContinue | Out-String | Write-Host"
                    },
                    new Step
                    {
                        name = "Upload Windows Build",
                        uses = "actions/upload-artifact@v4",
                        with = uploadArtifacts ? new Dictionary<string, string>
                        {
                            { "name", windowsArtifactName },
                            { "path", windowsBuildOutputPaths }
                        } : null
                    },
                    new Step
                    {
                        name = "Deactivate Unity License",
                        if_condition = "always()",
                        run = @"
& $env:UNITY_PATH -batchmode -nographics -returnlicense -logFile - || true"
                    }
                }
            };
        }

        var workflow = new GitHubWorkflow
        {
            name = "Unity Build",
            on = new Trigger
            {
                push = new Branches { branches = new List<string> { "main" } },
                pull_request = new Branches()
            },
            jobs = jobs
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance) // Use CamelCase for other fields
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithEventEmitter(nextEmitter => new MultilineScalarFlowStyleEmitter(nextEmitter)) // Ensure multiline strings are serialized correctly
            .DisableAliases() // Prevent aliasing for repeated values
            .Build();

        return serializer.Serialize(workflow);
    }

    // Ensure multiline strings are formatted correctly for YAML
    // private static string FormatMultilineString(string[] lines)
    // {
    //     return "|\n  " + string.Join("\n  ", lines);
    // }
}
