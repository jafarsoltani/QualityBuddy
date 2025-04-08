using System.Collections.Generic;

public class GitHubWorkflow
{
    public string name { get; set; }
    public Trigger on { get; set; }
    public Dictionary<string, Job> jobs { get; set; }
}

public class Trigger
{
    public Branches push { get; set; }
    public Branches pull_request { get; set; }
}

public class Branches
{
    public List<string> branches { get; set; }
}

public class Job
{
    public string runs_on { get; set; }
    public List<Step> steps { get; set; }
}

public class Step
{
    public string name { get; set; }
    public string uses { get; set; }
    public Dictionary<string, string> with { get; set; }
}
