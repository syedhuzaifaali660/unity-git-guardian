#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using GitGuardian.Checks;

namespace GitGuardian
{
    public static class GitGuardianCLI
    {
        public static void Run()
        {
            var checks = new IGuardianCheck[]
            {
                new Check_ProjectSettings(),
                new Check_MetaPairs(),
                new Check_DuplicateGuids(),
                new Check_GitFiles()
            };

            var issues = checks.SelectMany(c => c.Run()).ToList();
            foreach (var i in issues)
                Debug.Log($"[GitGuardian] {i.Severity} {i.Id} {i.Title} :: {i.RelatedPath}\n{i.Details}");

            var hasErrors = issues.Any(i => i.Severity == GuardianSeverity.Error);
            EditorApplication.Exit(hasErrors ? 1 : 0);
        }
    }
}
#endif
