#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GitGuardian.Checks;

namespace GitGuardian
{
    public sealed class GitGuardianWindow : EditorWindow
    {
        private readonly List<IGuardianCheck> _checks = new()
        {
            new Check_ProjectSettings(),
            new Check_MetaPairs(),
            new Check_DuplicateGuids(),
            new Check_GitFiles()
        };

        private List<GuardianIssue> _issues = new();
        private Vector2 _scroll;

        [MenuItem("Tools/Git Guardian/Open")]
        public static void Open()
        {
            GetWindow<GitGuardianWindow>("Git Guardian");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Checks", GUILayout.Height(28)))
                    RunChecks();

                if (GUILayout.Button("Copy Report", GUILayout.Height(28)))
                    EditorGUIUtility.systemCopyBuffer = BuildReport(_issues);
            }

            EditorGUILayout.Space(10);
            DrawCISection();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Checks: {_checks.Count}   Issues: {_issues.Count}", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var issue in _issues)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    var color = issue.Severity switch
                    {
                        GuardianSeverity.Error => new Color(1f, 0.85f, 0.85f),
                        GuardianSeverity.Warning => new Color(1f, 0.95f, 0.85f),
                        _ => new Color(0.9f, 0.95f, 1f)
                    };

                    var prev = GUI.backgroundColor;
                    GUI.backgroundColor = color;

                    EditorGUILayout.LabelField($"{issue.Severity}: {issue.Title}", EditorStyles.boldLabel);

                    GUI.backgroundColor = prev;

                    EditorGUILayout.LabelField(issue.Details, EditorStyles.wordWrappedLabel);

                    if (!string.IsNullOrEmpty(issue.RelatedPath))
                        EditorGUILayout.LabelField("Path:", issue.RelatedPath);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (issue.Fix != null && GUILayout.Button("Fix"))
                        {
                            var ok = issue.Fix.Invoke();
                            if (ok) RunChecks();
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCISection()
        {
            EditorGUILayout.LabelField("GitHub Actions (optional)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "To run these checks on every push/PR, add a workflow file to your repository. " +
                "Click 'Install GitHub Workflow' to generate .github/workflows/git-guardian.yml, then commit it.",
                MessageType.Info
            );

            WorkflowInstaller.RepoSlug = EditorGUILayout.TextField("Action Repo (owner/name)", WorkflowInstaller.RepoSlug);
            WorkflowInstaller.ActionRef = EditorGUILayout.TextField("Action Ref (tag/branch)", WorkflowInstaller.ActionRef);
            WorkflowInstaller.ProjectPath = EditorGUILayout.TextField("Project Path", WorkflowInstaller.ProjectPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Install GitHub Workflow", GUILayout.Height(26), GUILayout.Width(220)))
                    WorkflowInstaller.InstallWorkflow();
            }
        }

        private void RunChecks()
        {
            _issues = _checks.SelectMany(c =>
            {
                try { return c.Run(); }
                catch (System.Exception e)
                {
                    return new List<GuardianIssue>
                    {
                        new GuardianIssue("GG-CHK-EX", GuardianSeverity.Error, $"Check crashed: {c.Name}", e.ToString())
                    };
                }
            }).ToList();
        }

        private static string BuildReport(List<GuardianIssue> issues)
        {
            if (issues == null || issues.Count == 0) return "Git Guardian: No issues found.";

            return "Git Guardian Report\n" +
                   string.Join("\n\n", issues.Select(i =>
                       $"- [{i.Severity}] {i.Id} {i.Title}\n  {i.Details}\n  {i.RelatedPath}"
                   ));
        }
    }
}
#endif
