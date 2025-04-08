using UnityEditor;
using UnityEngine;
using System.IO;

public class QualityBuddyWindow : EditorWindow
{
  private const string ApiKeyPrefKey = "QualityBuddy_API_Key";
  private string apiKey = "";

  private bool showVersionWarning = false;
  private string unityVersion = "";

  private string yamlPath = "Assets/QualityBuddy-CI.yml";

  [MenuItem("Tools/QualityBuddy/CI Setup")]
  public static void ShowWindow()
  {
    var window = GetWindow<QualityBuddyWindow>("QualityBuddy CI");
    window.CheckUnityVersion();
  }

  private void OnEnable()
  {
    apiKey = EditorPrefs.GetString(ApiKeyPrefKey, "");
  }

  private void OnGUI()
  {
    if (showVersionWarning)
    {
      EditorGUILayout.HelpBox(
          $"Quality Buddy is designed for Unity 6 (e.g., 6000.x.x).\nYou're using {unityVersion}. Some features may not work correctly.",
          MessageType.Warning
      );

      EditorGUILayout.Space(10);
    }

    GUILayout.Label("QualityBuddy – CI Setup", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    DrawApiKeySection();
    EditorGUILayout.Space(10);

    DrawCISection();
    EditorGUILayout.Space(10);

    DrawTestToolsSection();
  }

  private void DrawApiKeySection()
  {
    GUILayout.Label("🔑 API Key (optional – for cloud sync):", EditorStyles.label);
    EditorGUI.BeginChangeCheck();
    apiKey = EditorGUILayout.TextField(apiKey);
    if (EditorGUI.EndChangeCheck())
    {
      EditorPrefs.SetString(ApiKeyPrefKey, apiKey);
    }
  }

  private void DrawCISection()
  {
    GUILayout.Label("⚙️ GitHub Actions CI Setup", EditorStyles.label);
    GUILayout.Label("Path to the generated YAML file:", EditorStyles.label);
    EditorGUI.BeginChangeCheck();

    yamlPath = EditorGUILayout.TextField(yamlPath);
    EditorGUILayout.Space();


    if (GUILayout.Button("Generate GitHub Actions YML"))
    {
      GenerateGitHubCIYAML();
    }
  }

  private void DrawTestToolsSection()
  {
    GUILayout.Label("🧪 Test Tools (Coming Soon)", EditorStyles.label);
    GUILayout.Label("- Test tagging", EditorStyles.miniLabel);
    GUILayout.Label("- Local test runner", EditorStyles.miniLabel);
    GUILayout.Label("- Push to dashboard", EditorStyles.miniLabel);
  }

  private void GenerateGitHubCIYAML()
  {
    var yamlContent = CIYamlGenerator.GenerateYaml();

    //string path = "Assets/QualityBuddy-CI.yml";

    File.WriteAllText(yamlPath, yamlContent);
    AssetDatabase.Refresh();
    Debug.Log($"✅ GitHub Actions YML written to {yamlPath}");
  }

  private void CheckUnityVersion()
  {
    unityVersion = Application.unityVersion;
    if (unityVersion.StartsWith("6000"))
    {
      Debug.Log("Unity version is compatible.");
    }
    else
    {
      showVersionWarning = true;
      Debug.LogWarning("Unity version is not compatible. Please use Unity 2021.3.x.");
    }
  }
}
