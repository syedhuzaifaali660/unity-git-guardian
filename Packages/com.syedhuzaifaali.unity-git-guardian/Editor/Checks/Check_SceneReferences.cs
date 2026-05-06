#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GitGuardian.Checks
{
    public sealed class Check_SceneReferences : GitGuardian.IGuardianCheck
    {
        public string Name => "Scenes: missing scripts & null references";

        public List<GitGuardian.GuardianIssue> Run()
        {
            var issues = new List<GitGuardian.GuardianIssue>();

            // Collect scenes: prefer Build Settings scenes first (likely project scenes),
            // then add any remaining .unity files found under Assets.
            var scenesToScan = new List<string>();
            try
            {
                foreach (var bs in EditorBuildSettings.scenes)
                {
                    if (bs == null) continue;
                    if (string.IsNullOrEmpty(bs.path)) continue;
                    if (!scenesToScan.Contains(bs.path))
                        scenesToScan.Add(bs.path);
                }
            }
            catch { /* ignore if BuildSettings unavailable */ }

            var assetsDir = Application.dataPath.Replace("\\", "/");
            foreach (var sceneFile in Directory.EnumerateFiles(assetsDir, "*.unity", SearchOption.AllDirectories))
            {
                var sceneFileNormalized = sceneFile.Replace("\\", "/");
                var relative = "Assets" + sceneFileNormalized.Substring(assetsDir.Length);
                if (!scenesToScan.Contains(relative))
                    scenesToScan.Add(relative);
            }

            foreach (var relative in scenesToScan)
            {
                try
                {
                    Debug.Log($"[GitGuardian] Scanning scene: {relative}");
                    var scene = EditorSceneManager.GetSceneByPath(relative);
                    var openedByUs = false;
                    if (!scene.isLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(relative, OpenSceneMode.Additive);
                        openedByUs = true;
                    }

                    var roots = scene.GetRootGameObjects();
                    Debug.Log($"[GitGuardian] Scene {relative} loaded; root objects: {roots.Length}");

                    foreach (var root in roots)
                    {
                        var transforms = root.GetComponentsInChildren<Transform>(true);
                        foreach (var t in transforms)
                        {
                            var go = t.gameObject;
                            var components = go.GetComponents<Component>();
                            for (int i = 0; i < components.Length; i++)
                            {
                                var comp = components[i];
                                if (comp == null)
                                {
                                    var msg = $"[GitGuardian] Missing script in scene {relative} on GameObject {GetGameObjectPath(go)}";
                                    Debug.LogError(msg);
                                    issues.Add(new GitGuardian.GuardianIssue(
                                        "GG-SCN-001",
                                        GitGuardian.GuardianSeverity.Error,
                                        "Missing script on GameObject",
                                        msg,
                                        relatedPath: relative
                                    ));
                                    continue;
                                }

                                try
                                {
                                    var so = new SerializedObject(comp);
                                    so.Update();
                                    var sp = so.GetIterator();
                                    if (sp.NextVisible(true))
                                    {
                                        do
                                        {
                                            if (sp.propertyType == SerializedPropertyType.ObjectReference)
                                            {
                                                if (sp.name == "m_Script") continue;
                                                if (sp.objectReferenceValue == null)
                                                {
                                                    var msg = $"[GitGuardian] Null reference in scene {relative}\nGameObject: {GetGameObjectPath(go)}\nComponent: {comp.GetType().Name}\nField: {sp.name}";
                                                    Debug.LogWarning(msg);
                                                    issues.Add(new GitGuardian.GuardianIssue(
                                                        "GG-SCN-002",
                                                        GitGuardian.GuardianSeverity.Warning,
                                                        "Null serialized reference in scene",
                                                        msg,
                                                        relatedPath: relative
                                                    ));
                                                }
                                            }
                                        } while (sp.NextVisible(false));
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogWarning($"[GitGuardian] Failed inspecting component {comp?.GetType().Name} on {GetGameObjectPath(go)}: {e.Message}");
                                }
                            }
                        }
                    }

                    if (openedByUs)
                        EditorSceneManager.CloseScene(scene, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GitGuardian] Failed to scan scene {relative}: {e.Message}");
                }
            }

            return issues;
        }

        private static string GetGameObjectPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
    }
}
#endif
