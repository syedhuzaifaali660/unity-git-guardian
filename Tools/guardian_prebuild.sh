// Assets/Editor/GitGuardianCloudBuildHook.cs
#if UNITY_CLOUD_BUILD
using UnityEngine;
using GitGuardian.Checks;
using System.Linq;

public static class GitGuardianCloudBuildHook
{
    public static void PreExport()
    {
        var checks = new GitGuardian.IGuardianCheck[]
        {
            new Check_ProjectSettings(),
            new Check_MetaPairs(),
            new Check_DuplicateGuids(),
            new Check_GitFiles()
        };

        var issues = checks.SelectMany(c => c.Run()).ToList();
        var hasErrors = issues.Any(i => i.Severity == GitGuardian.GuardianSeverity.Error);

        foreach (var issue in issues)
            Debug.Log($"[GitGuardian] [{issue.Severity}] {issue.Id}: {issue.Title}\n{issue.Details}");

        if (hasErrors)
            throw new System.Exception("Git Guardian found errors. Aborting build.");
    }
}
#endif