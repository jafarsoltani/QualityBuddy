using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class QualityBuddyWindow : EditorWindow
{
  private enum Platform { Windows, Linux, macOS, Switch, Xbox }
  private enum TriggerOption { OnPush, OnPullRequest, Manual, Scheduled }

  private List<Platform> availablePlatforms = new() {
        Platform.Windows, Platform.Linux, Platform.macOS, Platform.Switch, Platform.Xbox
    };

  private Dictionary<Platform, bool> platformToggles = new();
  private Dictionary<Platform, bool> platformFoldouts = new();
  private Dictionary<Platform, bool> buildEnabled = new();
  private Dictionary<Platform, string> outputNames = new();
  private Dictionary<Platform, bool> uploadEnabled = new();
  private Dictionary<Platform, string> uploadPaths = new();
  private Dictionary<Platform, string> artifactNames = new();
  private Dictionary<Platform, TriggerOption> triggerOptions = new();

  private Dictionary<Platform, string> platformEmojis = new()
  {
      { Platform.Windows, "🪟" },
      { Platform.Linux, "🐧" },
      { Platform.macOS, "🍎" },
      { Platform.Switch, "🎮" },
      { Platform.Xbox, "🎮" }
  };

  private Dictionary<string, string> ciProviderEmojis = new()
  {
      { "GitHub Actions", "🐙" },
      { "TeamCity", "🏙️" },
      { "Jenkins", "🤖" }
  };

  private string[] unityVersions = new[] { "2022.3.61f1", "2023.2.20f1", "6000.0.44f1" };
  private int selectedUnityVersion = 1;
  private bool isUnityVersionSupported = true;

  private string yamlOutputPath = "Assets/QualityBuddy-CI.yml";
  private string projectPath = "Test/QualityBuddyDev"; // Default project path, relative to the root of the repository

  [MenuItem("Tools/QualityBuddy - Beta")]
  public static void ShowWindow()
  {
    GetWindow<QualityBuddyWindow>("QualityBuddy - Beta");
  }

  private void OnEnable()
  {
    foreach (var platform in availablePlatforms)
    {
      platformToggles.TryAdd(platform, false);
      platformFoldouts.TryAdd(platform, false);
      buildEnabled.TryAdd(platform, true);
      outputNames.TryAdd(platform, "");
      uploadEnabled.TryAdd(platform, false);
      uploadPaths.TryAdd(platform, "");
      artifactNames.TryAdd(platform, "");
      triggerOptions.TryAdd(platform, TriggerOption.OnPush);
    }

    string currentVersion = Application.unityVersion;
    isUnityVersionSupported = false;
    for (int i = 0; i < unityVersions.Length; i++)
    {
      if (currentVersion.StartsWith(unityVersions[i].Substring(0, 6))) // Match first 6 characters
      {
        selectedUnityVersion = i;
        isUnityVersionSupported = true;
        break;
      }
    }
  }

  private bool IsInputValid()
  {
    bool anyPlatformToggled = false;

    foreach (var platform in availablePlatforms)
    {
      if (platformToggles[platform])
      {
        anyPlatformToggled = true;

        if (buildEnabled[platform])
        {
          if (string.IsNullOrWhiteSpace(outputNames[platform]))
          {
            return false;
          }
        }

        if (uploadEnabled[platform])
        {
          if (string.IsNullOrWhiteSpace(uploadPaths[platform]))
          {
            return false;
          }
        }
      }
    }

    if (!anyPlatformToggled)
    {
      return false;
    }

    return true;
  }

  private void OnGUI()
  {
    DrawHeader();
    if (!isUnityVersionSupported)
    {
      DrawVersionWarning();
      return;
    }

    DrawProjectPathInput(); // Add project path input field
    DrawPlatformSelection();
    DrawPlatformConfigurations();
    DrawUnityVersionSelection();
    DrawCIProviderSelection();
    DrawYamlOutputPath();
    DrawGenerateButton();
  }

  private void DrawHeader()
  {
    GUILayout.Label("👉 QualityBuddy – CI Job Generator", EditorStyles.boldLabel);
    EditorGUILayout.Space();
  }

  private void DrawVersionWarning()
  {
    EditorGUILayout.HelpBox($"Current Unity version ({Application.unityVersion}) is not supported.", MessageType.Warning);
    GUI.enabled = false; // Disable the rest of the UI
  }

  private void DrawProjectPathInput()
  {
    GUILayout.Label("Project Path (Relative to Repository Root):");
    projectPath = EditorGUILayout.TextField(projectPath);
    EditorGUILayout.Space();
  }

  private void DrawPlatformSelection()
  {
    GUILayout.Label("Select Platforms:", EditorStyles.label);
    EditorGUILayout.BeginHorizontal();
    foreach (var platform in availablePlatforms)
    {
      bool enabled = platform == Platform.Windows || platform == Platform.Linux;
      GUI.enabled = enabled && isUnityVersionSupported;

      if (enabled)
      {
        platformToggles[platform] = GUILayout.Toggle(platformToggles[platform], $"{platformEmojis[platform]} {platform}");
      }
      else
      {
        GUILayout.Toggle(false, new GUIContent($"{platformEmojis[platform]} {platform}", "Coming soon"));
      }
    }
    GUI.enabled = isUnityVersionSupported;
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.Space();
  }

  private void DrawPlatformConfigurations()
  {
    foreach (var platform in availablePlatforms)
    {
      if (!platformToggles[platform]) continue;

      EditorGUILayout.BeginVertical("box");
      platformFoldouts[platform] = EditorGUILayout.Foldout(platformFoldouts[platform], $"{platformEmojis[platform]} {platform} Configuration", true);

      if (platformFoldouts[platform])
      {
        EditorGUI.indentLevel++;
        DrawBuildConfiguration(platform);
        DrawUploadConfiguration(platform);
        DrawTriggerOption(platform);
        EditorGUI.indentLevel--;
      }

      EditorGUILayout.EndVertical();
    }
    EditorGUILayout.Space();
  }

  private void DrawBuildConfiguration(Platform platform)
  {
    buildEnabled[platform] = EditorGUILayout.Toggle("Build Project", buildEnabled[platform]);
    if (buildEnabled[platform])
    {
      outputNames[platform] = EditorGUILayout.TextField("Output Name", outputNames[platform]);
      if (string.IsNullOrWhiteSpace(outputNames[platform]))
      {
        EditorGUILayout.HelpBox("Output Name is required when Build Project is enabled.", MessageType.Error);
      }
    }
  }

  private void DrawUploadConfiguration(Platform platform)
  {
    EditorGUI.BeginDisabledGroup(!buildEnabled[platform] || string.IsNullOrWhiteSpace(outputNames[platform]));
    uploadEnabled[platform] = EditorGUILayout.Toggle("Upload Artifacts", uploadEnabled[platform]);
    EditorGUI.EndDisabledGroup();

    if (uploadEnabled[platform])
    {
      GUILayout.Label("Paths to File(s):");
      uploadPaths[platform] = EditorGUILayout.TextArea(uploadPaths[platform], GUILayout.MinHeight(50));
      artifactNames[platform] = EditorGUILayout.TextField("Artifact Name", artifactNames[platform]);

      if (string.IsNullOrWhiteSpace(uploadPaths[platform]))
      {
        EditorGUILayout.HelpBox("Paths to File(s) are required when Upload Artifacts is enabled.", MessageType.Error);
      }
    }
  }

  private void DrawTriggerOption(Platform platform)
  {
    triggerOptions[platform] = (TriggerOption)EditorGUILayout.EnumPopup("Trigger", triggerOptions[platform]);
  }

  private void DrawUnityVersionSelection()
  {
    GUILayout.Label("Unity Version:");
    selectedUnityVersion = EditorGUILayout.Popup(selectedUnityVersion, unityVersions);
    EditorGUILayout.Space();
  }

  private void DrawCIProviderSelection()
  {
    GUILayout.Label("CI Provider:");
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.ToggleLeft($"{ciProviderEmojis["GitHub Actions"]} GitHub Actions", true);
    GUI.enabled = false;
    EditorGUILayout.ToggleLeft(new GUIContent($"{ciProviderEmojis["TeamCity"]} TeamCity", "Coming soon"), false);
    EditorGUILayout.ToggleLeft(new GUIContent($"{ciProviderEmojis["Jenkins"]} Jenkins", "Coming soon"), false);
    GUI.enabled = isUnityVersionSupported;
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.Space();
  }

  private void DrawYamlOutputPath()
  {
    GUILayout.Label("YAML Output Path:");
    yamlOutputPath = EditorGUILayout.TextField(yamlOutputPath);
    EditorGUILayout.Space();
  }

  private void DrawGenerateButton()
  {
    var isInputValid = IsInputValid();
    GUI.enabled = isInputValid;
    if (GUILayout.Button("🛠️  Generate Job YAML File"))
    {
      GenerateGitHubCIYAML();
    }
    GUI.enabled = true;
  }

  private void GenerateGitHubCIYAML()
  {
    bool buildForWindows = platformToggles.ContainsKey(Platform.Windows) && platformToggles[Platform.Windows] && buildEnabled[Platform.Windows];
    bool buildForLinux = platformToggles.ContainsKey(Platform.Linux) && platformToggles[Platform.Linux] && buildEnabled[Platform.Linux];
    bool uploadArtifacts = false;

    string windowsBuildOutputName = outputNames.ContainsKey(Platform.Windows) ? outputNames[Platform.Windows] : "";
    string windowsArtifactName = artifactNames.ContainsKey(Platform.Windows) ? artifactNames[Platform.Windows] : "";
    string windowsBuildOutputPaths = uploadPaths.ContainsKey(Platform.Windows) ? uploadPaths[Platform.Windows] : "";
    if (platformToggles.ContainsKey(Platform.Windows) && uploadEnabled[Platform.Windows])
      uploadArtifacts = true;

    string linuxBuildOutputName = outputNames.ContainsKey(Platform.Linux) ? outputNames[Platform.Linux] : "";
    string linuxArtifactName = artifactNames.ContainsKey(Platform.Linux) ? artifactNames[Platform.Linux] : "";
    string linuxBuildOutputPaths = uploadPaths.ContainsKey(Platform.Linux) ? uploadPaths[Platform.Linux] : "";
    if (platformToggles.ContainsKey(Platform.Linux) && uploadEnabled[Platform.Linux])
      uploadArtifacts = true;

    if (string.IsNullOrEmpty(windowsBuildOutputPaths))
      windowsBuildOutputPaths = GetDefaultBuildOutputPath("Windows");
    if (string.IsNullOrEmpty(linuxBuildOutputPaths))
      linuxBuildOutputPaths = GetDefaultBuildOutputPath("Linux");

    var yamlContent = CIYamlGenerator.GenerateYaml(
        projectPath, // Use the user-specified project path
        buildForWindows,
        buildForLinux,
        uploadArtifacts,
        windowsBuildOutputName,
        windowsArtifactName,
        windowsBuildOutputPaths,
        linuxBuildOutputName,
        linuxArtifactName,
        linuxBuildOutputPaths
    );

    File.WriteAllText(yamlOutputPath, yamlContent);
    AssetDatabase.Refresh();
    Debug.Log("✅ GitHub Actions YML written to " + yamlOutputPath);
  }

  private string GetDefaultBuildOutputPath(string platform)
  {
    return $"Builds/{platform}/";
  }
}
