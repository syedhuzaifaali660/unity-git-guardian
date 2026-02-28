#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GitGuardian.Checks
{
    public sealed class Check_MetaPairs : GitGuardian.IGuardianCheck
    {
        public string Name => "Assets: missing .meta / orphan .meta";

        static bool IsIgnoredPath(string fullPath)
        {
            var p = fullPath.Replace('\\', '/');
            return p.Contains("/.git/") || p.Contains("/Library/") || p.Contains("/Temp/") || p.Contains("/Logs/");
        }

        static bool IsUnityIgnoredLeaf(string name) => GitGuardian.UnityIgnored.IsIgnoredName(name);

        static bool IsUnderUnityIgnoredFolder(string assetsDir, string fullPath) =>
            GitGuardian.UnityIgnored.IsUnderIgnoredFolder(assetsDir, fullPath);

        public List<GitGuardian.GuardianIssue> Run()
        {
            var issues = new List<GitGuardian.GuardianIssue>();
            var assetsDir = Application.dataPath;

            foreach (var file in Directory.EnumerateFiles(assetsDir, "*", SearchOption.AllDirectories))
            {
                if (IsIgnoredPath(file)) continue;
                if (file.EndsWith(".meta")) continue;

                if (IsUnderUnityIgnoredFolder(assetsDir, file)) continue;

                var fileName = Path.GetFileName(file);
                if (IsUnityIgnoredLeaf(fileName)) continue;

                var meta = file + ".meta";
                if (!File.Exists(meta))
                {
                    issues.Add(new GitGuardian.GuardianIssue(
                        "GG-META-001",
                        GitGuardian.GuardianSeverity.Error,
                        "Missing .meta file",
                        $"Asset has no .meta next to it.\nFile: {file}",
                        relatedPath: file
                    ));
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(assetsDir, "*", SearchOption.AllDirectories))
            {
                if (IsIgnoredPath(dir)) continue;

                if (IsUnderUnityIgnoredFolder(assetsDir, dir)) continue;

                var dirName = Path.GetFileName(dir);
                if (IsUnityIgnoredLeaf(dirName)) continue;

                var meta = dir + ".meta";
                if (!File.Exists(meta))
                {
                    issues.Add(new GitGuardian.GuardianIssue(
                        "GG-META-002",
                        GitGuardian.GuardianSeverity.Warning,
                        "Missing folder .meta file",
                        $"Folder has no .meta next to it.\nFolder: {dir}",
                        relatedPath: dir
                    ));
                }
            }

            foreach (var meta in Directory.EnumerateFiles(assetsDir, "*.meta", SearchOption.AllDirectories))
            {
                if (IsIgnoredPath(meta)) continue;
                if (IsUnderUnityIgnoredFolder(assetsDir, meta)) continue;

                var target = meta.Substring(0, meta.Length - ".meta".Length);
                var targetName = Path.GetFileName(target);

                if (IsUnityIgnoredLeaf(targetName)) continue;
                if (IsUnderUnityIgnoredFolder(assetsDir, target)) continue;

                if (!File.Exists(target) && !Directory.Exists(target))
                {
                    issues.Add(new GitGuardian.GuardianIssue(
                        "GG-META-003",
                        GitGuardian.GuardianSeverity.Warning,
                        "Orphan .meta file",
                        $"Meta exists but asset/folder is missing.\nMeta: {meta}",
                        relatedPath: meta
                    ));
                }
            }

            return issues;
        }
    }
}
#endif
