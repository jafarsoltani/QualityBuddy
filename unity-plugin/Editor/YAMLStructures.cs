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
    public List<Step> steps { get; set; }
}

public class Step
{
    public string name { get; set; }
    public string uses { get; set; }

    public bool ShouldSerializeName() => !string.IsNullOrEmpty(name);

    public bool ShouldSerializeWith() => with != null && with.Count > 0;

    public Dictionary<string, string> with { get; set; }
}
