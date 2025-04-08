using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class CIYamlGenerator
{
    // [MenuItem("Tools/Generate Unity CI YAML")]
    public static string GenerateYaml()
    {
        var workflow = new GitHubWorkflow
        {
            name = "Unity CI",
            on = new Trigger
            {
                push = new Branches { branches = new List<string> { "main" } },
                pull_request = new Branches { branches = new List<string> { "main" } }
            },
            jobs = new Dictionary<string, Job>
            {
                {
                    "test", new Job
                    {
                        runs_on = "ubuntu-latest",
                        steps = new List<Step>
                        {
                            new Step { uses = "actions/checkout@v3" },
                            new Step
                            {
                                name = "Set up Unity",
                                uses = "game-ci/unity-actions/setup@v2",
                                with = new Dictionary<string, string>
                                {
                                    { "unityVersion", "2021.3.16f1" }
                                }
                            },
                            new Step
                            {
                                name = "Run tests",
                                uses = "game-ci/unity-actions/test@v2",
                                with = new Dictionary<string, string>
                                {
                                    { "testMode", "PlayMode" }
                                }
                            }
                        }
                    }
                }
            }
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(workflow);

        // var outputPath = "Assets/unity-ci.yml";
        // File.WriteAllText(outputPath, yaml);

        // Debug.Log($"✅ YAML generated at: {outputPath}");
    }
}
