#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitGuardian
{
    internal static class WorkflowInstaller
    {
        private const string PrefRepo = "GitGuardian.Workflow.Repo";
        private const string PrefRef = "GitGuardian.Workflow.Ref";
        private const string PrefProjectPath = "GitGuardian.Workflow.ProjectPath";

        public static string RepoSlug
        {
            get => EditorPrefs.GetString(PrefRepo, "syedhuzaifaali660/unity-git-guardian");
            set => EditorPrefs.SetString(PrefRepo, value);
        }

        public static string ActionRef
        {
            get => EditorPrefs.GetString(PrefRef, "v0.0.1");
            set => EditorPrefs.SetString(PrefRef, value);
        }

        public static string ProjectPath
        {
            get => EditorPrefs.GetString(PrefProjectPath, ".");
            set => EditorPrefs.SetString(PrefProjectPath, value);
        }

        public static void InstallWorkflow()
        {
            var root = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(root))
            {
                EditorUtility.DisplayDialog("Git Guardian", "Could not find project root.", "OK");
                return;
            }

            var workflowsDir = Path.Combine(root, ".github", "workflows");
            Directory.CreateDirectory(workflowsDir);

            var filePath = Path.Combine(workflowsDir, "git-guardian.yml");

            var yaml =
$@"name: Git Guardian

on:
  pull_request:
  push:
    branches: [ main, master ]

jobs:
  guardian:
    runs-on: ubuntu-latest
    steps:
      - uses: {RepoSlug}/.github/actions/git-guardian@{ActionRef}
        with:
          project_path: {ProjectPath}
";

            File.WriteAllText(filePath, yaml);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Git Guardian",
                "Workflow created:\n" + filePath + "\n\nNext step: commit this file and push to GitHub.",
                "OK"
            );
        }
    }
}
#endif
