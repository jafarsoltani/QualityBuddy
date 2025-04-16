using System.Collections.Generic;
using YamlDotNet.Serialization;

public class GitHubWorkflow
{
    public string name { get; set; }
    public Trigger on { get; set; }
    public Dictionary<string, Job> jobs { get; set; }
}

public class Trigger
{
    public Branches push { get; set; }

    [YamlMember(Alias = "pull_request")]
    public Branches pull_request { get; set; }
}

public class Branches
{
    public List<string> branches { get; set; }
}

public class Job
{
    [YamlMember(Alias = "runs-on")]
    public string runs_on { get; set; }

    [YamlMember(Alias = "needs")]
    public List<string> needs { get; set; } // Add support for job dependencies
    public List<Step> steps { get; set; }
}

public class Step
{
    public string name { get; set; }
    public string uses { get; set; }

    [YamlMember(Alias = "if")]
    public string if_condition { get; set; } // Add support for conditional execution

    //[YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
    public string run { get; set; } // Ensure multiline strings are serialized correctly

    public bool ShouldSerializeName() => !string.IsNullOrEmpty(name);
    public bool ShouldSerializeWith() => with != null && with.Count > 0;
    public bool ShouldSerializeIf_Condition() => !string.IsNullOrEmpty(if_condition); // Serialize only if not null

    public Dictionary<string, string> with { get; set; }
}

public class EnvironmentVariables
{
    [YamlMember(Alias = "UNITY_VERSION")]
    public string UNITY_VERSION { get; set; }

    [YamlMember(Alias = "HASH")]
    public string HASH { get; set; }

    [YamlMember(Alias = "UNITY_EMAIL")]
    public string UNITY_EMAIL { get; set; }

    [YamlMember(Alias = "UNITY_PASSWORD")]
    public string UNITY_PASSWORD { get; set; }

    [YamlMember(Alias = "UNITY_LICENSE")]
    public string UNITY_LICENSE { get; set; }

    [YamlMember(Alias = "CACHE_DIR")]
    public string CACHE_DIR { get; set; }

    [YamlMember(Alias = "INSTALLER_PATH")]
    public string INSTALLER_PATH { get; set; }

    [YamlMember(Alias = "BASE_URL")]
    public string BASE_URL { get; set; }

    [YamlMember(Alias = "UNITY_PATH")]
    public string UNITY_PATH { get; set; }
}

public class JobWithEnv : Job
{
    public EnvironmentVariables env { get; set; }
}
