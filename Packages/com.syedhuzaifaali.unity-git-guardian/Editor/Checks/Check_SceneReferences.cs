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
            var assetsDir = Application.dataPath.Replace("\\", "/");

            foreach (var sceneFile in Directory.EnumerateFiles(assetsDir, "*.unity", SearchOption.AllDirectories))
            {
                try
                {
                    var sceneFileNormalized = sceneFile.Replace("\\", "/");
                    var relative = "Assets" + sceneFileNormalized.Substring(assetsDir.Length);

                    var scene = EditorSceneManager.GetSceneByPath(relative);
                    var openedByUs = false;
                    if (!scene.isLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(relative, OpenSceneMode.Additive);
                        openedByUs = true;
                    }

                    foreach (var root in scene.GetRootGameObjects())
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
                                    issues.Add(new GitGuardian.GuardianIssue(
                                        "GG-SCN-001",
                                        GitGuardian.GuardianSeverity.Error,
                                        "Missing script on GameObject",
                                        $"Scene: {relative}\nGameObject: {GetGameObjectPath(go)} has a missing script component.",
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
                                                    issues.Add(new GitGuardian.GuardianIssue(
                                                        "GG-SCN-002",
                                                        GitGuardian.GuardianSeverity.Warning,
                                                        "Null serialized reference in scene",
                                                        $"Scene: {relative}\nGameObject: {GetGameObjectPath(go)}\nComponent: {comp.GetType().Name}\nField: {sp.name} is null",
                                                        relatedPath: relative
                                                    ));
                                                }
                                            }
                                        } while (sp.NextVisible(false));
                                    }
                                }
                                catch { /* ignore per-component errors */ }
                            }
                        }
                    }

                    if (openedByUs)
                        EditorSceneManager.CloseScene(scene, true);
                }
                catch { /* ignore scene scan errors */ }
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
