#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GitGuardian.Checks
{
    public sealed class Check_GitFiles : GitGuardian.IGuardianCheck
    {
        public string Name => "Repo: ignore files (.gitignore / ignore.conf)";

        public List<GitGuardian.GuardianIssue> Run()
        {
            var issues = new List<GitGuardian.GuardianIssue>();
            var root = Directory.GetParent(Application.dataPath)?.FullName;
            if (root == null) return issues;

            var gitignore = Path.Combine(root, ".gitignore");
            var gitattributes = Path.Combine(root, ".gitattributes");

            var plasticIgnoreRoot = Path.Combine(root, "ignore.conf");
            var plasticIgnoreHidden = Path.Combine(root, ".plastic", "ignore.conf");

            var hasGitignore = File.Exists(gitignore);
            var hasPlasticIgnore = File.Exists(plasticIgnoreRoot) || File.Exists(plasticIgnoreHidden);

            if (!hasGitignore && !hasPlasticIgnore)
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    "GG-VCS-001",
                    GitGuardian.GuardianSeverity.Warning,
                    "No ignore file found for Git or Plastic",
                    "Add either a .gitignore (Git) or ignore.conf (Plastic) to avoid committing Library/, Temp/, Logs/, etc."
                ));
            }
            else if (!hasGitignore && hasPlasticIgnore)
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    "GG-VCS-002",
                    GitGuardian.GuardianSeverity.Info,
                    "Plastic ignore.conf detected",
                    "Looks good (using Plastic). Git .gitignore is optional in this setup."
                ));
            }
            else if (hasGitignore && !hasPlasticIgnore)
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    "GG-VCS-003",
                    GitGuardian.GuardianSeverity.Info,
                    "Git .gitignore detected",
                    "Looks good (using Git). Plastic ignore.conf is optional."
                ));
            }

            var isGitRepo = Directory.Exists(Path.Combine(root, ".git"));
            if (isGitRepo && !File.Exists(gitattributes))
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    "GG-GIT-002",
                    GitGuardian.GuardianSeverity.Info,
                    "Missing .gitattributes",
                    "Consider adding .gitattributes (line endings, optional LFS patterns for large binaries).",
                    relatedPath: gitattributes
                ));
            }

            return issues;
        }
    }
}
#endif
