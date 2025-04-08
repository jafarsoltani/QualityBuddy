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
                    "build", new Job
                    {
                        runs_on = "ubuntu-latest",
                        steps = new List<Step>
                        {
                            new Step
                            {
                                name = "checkout repository",
                                uses = "actions/checkout@v4"
                            },
                            new Step
                            {
                                name = "Set up Unity",
                                uses = "game-ci/unity-actions/setup@v2",
                                with = new Dictionary<string, string>
                                {
                                    { "unityVersion", "6000.0.44f1" }
                                }
                            },
                            new Step
                            {
                                name = "Build project",
                                uses = "game-ci/unity-builder@v4",
                                with = new Dictionary<string, string>
                                {
                                    { "targetPlatform", "StandaloneOSX" }
                                }
                            }
                        }
                    }
                }
            }
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        return serializer.Serialize(workflow);
    }
}
