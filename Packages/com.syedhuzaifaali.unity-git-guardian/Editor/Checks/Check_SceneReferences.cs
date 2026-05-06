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
            var scannedScenePaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // Scan currently loaded scenes first so the open demo scene is covered even
            // when it is unsaved or not part of Build Settings.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (!loadedScene.IsValid() || !loadedScene.isLoaded) continue;

                var sceneLabel = string.IsNullOrEmpty(loadedScene.path)
                    ? $"Open Scene {i}"
                    : loadedScene.path;

                if (!string.IsNullOrEmpty(loadedScene.path))
                    scannedScenePaths.Add(loadedScene.path);

                ScanScene(loadedScene, sceneLabel, issues, false);
            }

            // Collect scenes: prefer Build Settings scenes first (likely project scenes),
            // then add any remaining .unity files found under Assets.
            var scenesToScan = new List<string>();
            try
            {
                foreach (var bs in EditorBuildSettings.scenes)
                {
                    if (bs == null) continue;
                    if (string.IsNullOrEmpty(bs.path)) continue;
                    if (!scannedScenePaths.Contains(bs.path) && !scenesToScan.Contains(bs.path))
                        scenesToScan.Add(bs.path);
                }
            }
            catch { /* ignore if BuildSettings unavailable */ }

            var assetsDir = Application.dataPath.Replace("\\", "/");
            foreach (var sceneFile in Directory.EnumerateFiles(assetsDir, "*.unity", SearchOption.AllDirectories))
            {
                var sceneFileNormalized = sceneFile.Replace("\\", "/");
                var relative = "Assets" + sceneFileNormalized.Substring(assetsDir.Length);
                if (!scannedScenePaths.Contains(relative) && !scenesToScan.Contains(relative))
                    scenesToScan.Add(relative);
            }

            foreach (var relative in scenesToScan)
            {
                try
                {
                    var scene = EditorSceneManager.GetSceneByPath(relative);
                    var openedByUs = false;
                    if (!scene.isLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(relative, OpenSceneMode.Additive);
                        openedByUs = true;
                    }

                    ScanScene(scene, relative, issues, true);

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

        private static void ScanScene(Scene scene, string sceneLabel, List<GitGuardian.GuardianIssue> issues, bool logScan)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;

            if (logScan)
                Debug.Log($"[GitGuardian] Scanning scene: {sceneLabel}");

            var roots = scene.GetRootGameObjects();
            if (logScan)
                Debug.Log($"[GitGuardian] Scene {sceneLabel} loaded; root objects: {roots.Length}");

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
                            var msg = $"[GitGuardian] Missing script in scene {sceneLabel} on GameObject {GetGameObjectPath(go)}";
                            Debug.LogError(msg);
                            issues.Add(new GitGuardian.GuardianIssue(
                                "GG-SCN-001",
                                GitGuardian.GuardianSeverity.Error,
                                "Missing script on GameObject",
                                msg,
                                relatedPath: sceneLabel,
                                goTo: CreateGoToAction(scene, sceneLabel, GetGameObjectPath(go), null, null)
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
                                            var msg = $"[GitGuardian] Null reference in scene {sceneLabel}\nGameObject: {GetGameObjectPath(go)}\nComponent: {comp.GetType().Name}\nField: {sp.name}";
                                            Debug.LogWarning(msg);
                                            issues.Add(new GitGuardian.GuardianIssue(
                                                "GG-SCN-002",
                                                GitGuardian.GuardianSeverity.Warning,
                                                "Null serialized reference in scene",
                                                msg,
                                                relatedPath: sceneLabel,
                                                goTo: CreateGoToAction(scene, sceneLabel, GetGameObjectPath(go), comp.GetType().FullName, sp.name)
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
        }

        private static System.Func<bool> CreateGoToAction(Scene scene, string sceneLabel, string gameObjectPath, string componentTypeName, string fieldName)
        {
            return () =>
            {
                var targetScene = scene;

                if (!targetScene.IsValid() || !targetScene.isLoaded)
                {
                    if (!string.IsNullOrEmpty(targetScene.path))
                    {
                        targetScene = EditorSceneManager.GetSceneByPath(targetScene.path);
                        if (!targetScene.isLoaded)
                        {
                            try
                            {
                                targetScene = EditorSceneManager.OpenScene(targetScene.path, OpenSceneMode.Additive);
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogWarning($"[GitGuardian] Could not open scene {sceneLabel}: {e.Message}");
                                return false;
                            }
                        }
                    }
                }

                if (!targetScene.IsValid() || !targetScene.isLoaded)
                    targetScene = FindLoadedSceneByLabel(sceneLabel);

                if (!targetScene.IsValid() || !targetScene.isLoaded)
                {
                    Debug.LogWarning($"[GitGuardian] Could not locate scene {sceneLabel} for navigation.");
                    return false;
                }

                var go = FindGameObjectByPath(targetScene, gameObjectPath);
                if (go == null)
                {
                    Debug.LogWarning($"[GitGuardian] Could not locate GameObject '{gameObjectPath}' in scene {sceneLabel}.");
                    return false;
                }

                GameObject selected = go;
                if (!string.IsNullOrEmpty(componentTypeName))
                {
                    var comps = go.GetComponents<Component>();
                    foreach (var comp in comps)
                    {
                        if (comp == null) continue;
                        var type = comp.GetType();
                        if (type.FullName == componentTypeName || type.Name == componentTypeName)
                        {
                            selected = go;
                            Selection.activeObject = comp;
                            EditorGUIUtility.PingObject(comp);
                            EditorGUIUtility.PingObject(go);
                            return true;
                        }
                    }
                }

                Selection.activeGameObject = selected;
                EditorGUIUtility.PingObject(selected);
                return true;
            };
        }

        private static Scene FindLoadedSceneByLabel(string sceneLabel)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var candidate = SceneManager.GetSceneAt(i);
                if (!candidate.IsValid() || !candidate.isLoaded) continue;
                var candidateLabel = string.IsNullOrEmpty(candidate.path) ? $"Open Scene {i}" : candidate.path;
                if (candidateLabel == sceneLabel)
                    return candidate;
            }

            return default;
        }

        private static GameObject FindGameObjectByPath(Scene scene, string gameObjectPath)
        {
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(gameObjectPath)) return null;

            var segments = gameObjectPath.Split('/');
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root.name != segments[0]) continue;

                var current = root;
                for (var i = 1; i < segments.Length; i++)
                {
                    current = FindChild(current.transform, segments[i]);
                    if (current == null) break;
                }

                if (current != null)
                    return current;
            }

            return null;
        }

        private static GameObject FindChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == childName)
                    return child.gameObject;
            }

            return null;
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
