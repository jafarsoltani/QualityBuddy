using UnityEditor;
using UnityEngine;
using System.IO;

public class QualityBuddyWindow : EditorWindow
{
    private const string ApiKeyPrefKey = "QualityBuddy_API_Key";
    private string apiKey = "";

    [MenuItem("Tools/QualityBuddy/CI Setup")]
    public static void ShowWindow()
    {
        GetWindow<QualityBuddyWindow>("QualityBuddy CI");
    }

    private void OnEnable()
    {
        apiKey = EditorPrefs.GetString(ApiKeyPrefKey, "");
    }

    private void OnGUI()
    {
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
        string yamlContent = @"name: Unity CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up Unity
        uses: game-ci/unity-actions/setup@v2
        with:
          unityVersion: 2021.3.16f1
      - name: Run tests
        uses: game-ci/unity-actions/test@v2
        with:
          testMode: PlayMode
";

        string path = "Assets/QualityBuddy-CI.yml";
        File.WriteAllText(path, yamlContent);
        AssetDatabase.Refresh();
        Debug.Log($"✅ GitHub Actions YML written to {path}");
    }
}
