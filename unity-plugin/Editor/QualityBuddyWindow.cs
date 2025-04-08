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

  private enum LicenseType { Free, Professional }
  private LicenseType selectedLicenseType = LicenseType.Free;

  private string unityEmail = "";
  private string unityPassword = "";
  private string unitySerialKey = "";
  private string licenseFilePath = "";


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

    DrawLicenseSection();
    EditorGUILayout.Space(10);

    EditorGUI.BeginDisabledGroup(!IsInputValid());

    if (GUILayout.Button("Add GitHub Secrets"))
    {
      AddGitHubSecrets();
      EditorGUILayout.Space(10);
    }

    GUILayout.Label("QualityBuddy – CI Setup", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    DrawApiKeySection();
    EditorGUILayout.Space(10);

    DrawCISection();
    EditorGUILayout.Space(10);

    DrawTestToolsSection();

    EditorGUI.EndDisabledGroup();
  }

  private bool IsInputValid()
  {
    if (string.IsNullOrEmpty(unityEmail) || !unityEmail.Contains("@") || !unityEmail.Contains("."))
      return false;

    if (string.IsNullOrEmpty(unityPassword) || unityPassword.Length < 6)
      return false;

    if (selectedLicenseType == LicenseType.Professional)
    {
      if (string.IsNullOrEmpty(unitySerialKey) || unitySerialKey.Length < 10)
        return false;
    }
    else
    {
      if (string.IsNullOrEmpty(licenseFilePath) || !File.Exists(licenseFilePath) || Path.GetExtension(licenseFilePath) != ".ulf")
        return false;
    }

    return true;
  }

  private void DrawLicenseSection()
  {
    EditorGUILayout.LabelField("🧾 Unity License Setup", EditorStyles.boldLabel);

    selectedLicenseType = (LicenseType)EditorGUILayout.EnumPopup("License Type", selectedLicenseType);

    unityEmail = EditorGUILayout.TextField("Unity Email", unityEmail);
    unityPassword = EditorGUILayout.PasswordField("Unity Password", unityPassword);

    if (selectedLicenseType == LicenseType.Professional)
    {
      unitySerialKey = EditorGUILayout.TextField("Unity Serial Key", unitySerialKey);
    }
    else
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("License File (.ulf)", GUILayout.Width(150));
      if (GUILayout.Button("Browse", GUILayout.Width(100)))
      {
        string selectedPath = EditorUtility.OpenFilePanel("Select Unity License File", "", "ulf");
        if (!string.IsNullOrEmpty(selectedPath))
        {
          licenseFilePath = selectedPath;
        }
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.LabelField(licenseFilePath);
    }
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

  private void AddGitHubSecrets()
  {
    Debug.Log("🔐 Adding GitHub Secrets...");

    Debug.Log($"UNITY_EMAIL = {unityEmail}");
    Debug.Log($"UNITY_PASSWORD = {new string('*', unityPassword.Length)}");

    if (selectedLicenseType == LicenseType.Professional)
    {
      Debug.Log($"UNITY_SERIAL = {unitySerialKey}");
    }
    else
    {
      Debug.Log($"UNITY_LICENSE_FILE = {licenseFilePath}");
    }

    // In a real setup, you'd write to a secrets manager file or call GitHub API.
    // You can serialize these into a .env or a JSON and include it in the CI setup step.
  }

}
