#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GitGuardian.Checks
{
    public sealed class Check_DuplicateGuids : GitGuardian.IGuardianCheck
    {
        public string Name => "Assets: duplicate GUIDs (meta)";

        public List<GitGuardian.GuardianIssue> Run()
        {
            var issues = new List<GitGuardian.GuardianIssue>();
            var assetsDir = Application.dataPath;

            var guidToMeta = new Dictionary<string, string>();

            foreach (var metaPath in Directory.EnumerateFiles(assetsDir, "*.meta", SearchOption.AllDirectories))
            {
                var guid = ReadGuid(metaPath);
                if (string.IsNullOrEmpty(guid)) continue;

                if (guidToMeta.TryGetValue(guid, out var firstMeta))
                {
                    issues.Add(new GitGuardian.GuardianIssue(
                        "GG-GUID-001",
                        GitGuardian.GuardianSeverity.Error,
                        "Duplicate GUID found",
                        $"Two meta files share the same GUID:\n- {firstMeta}\n- {metaPath}\nThis can cause confusing reference issues.",
                        relatedPath: metaPath
                    ));
                }
                else
                {
                    guidToMeta[guid] = metaPath;
                }
            }

            return issues;
        }

        static string ReadGuid(string metaPath)
        {
            try
            {
                foreach (var line in File.ReadLines(metaPath))
                {
                    if (line.StartsWith("guid:"))
                        return line.Substring("guid:".Length).Trim();
                }
            }
            catch { }
            return null;
        }
    }
}
#endif
