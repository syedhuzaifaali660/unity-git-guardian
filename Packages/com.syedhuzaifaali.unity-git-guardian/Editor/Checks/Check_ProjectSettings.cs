#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace GitGuardian.Checks
{
    public sealed class Check_ProjectSettings : GitGuardian.IGuardianCheck
    {
        public string Name => "Project Settings: Force Text + Meta visibility";

        public List<GitGuardian.GuardianIssue> Run()
        {
            var issues = new List<GitGuardian.GuardianIssue>();

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    id: "GG-PROJ-001",
                    severity: GitGuardian.GuardianSeverity.Error,
                    title: "Asset Serialization is not Force Text",
                    details: $"Current: {EditorSettings.serializationMode}. Recommended: Force Text (helps merges & team workflows).",
                    fix: () => { EditorSettings.serializationMode = SerializationMode.ForceText; return true; }
                ));
            }

            var mode = GetVcsModeString();

            // Hidden meta files is the one setting that almost always breaks collaboration
            if (ContainsIgnoreCase(mode, "Hidden"))
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    id: "GG-PROJ-002",
                    severity: GitGuardian.GuardianSeverity.Error,
                    title: "Version Control Mode is 'Hidden Meta Files'",
                    details: "Recommended: 'Visible Meta Files' (or 'PlasticSCM' if you use Unity Version Control / Plastic).",
                    fix: () => { SetVcsMode("Visible Meta Files"); return true; }
                ));
            }
            else if (!ContainsIgnoreCase(mode, "Visible") && !ContainsIgnoreCase(mode, "Plastic"))
            {
                issues.Add(new GitGuardian.GuardianIssue(
                    id: "GG-PROJ-003",
                    severity: GitGuardian.GuardianSeverity.Info,
                    title: "Version Control Mode is not standard",
                    details: $"Current: '{mode}'. This is fine if intentional. Recommended for most teams: 'Visible Meta Files' or 'PlasticSCM'."
                ));
            }

            return issues;
        }

        // Unity deprecated EditorSettings.externalVersionControl in favor of VersionControlSettings.mode.
        // To stay compatible across Unity versions (and avoid CS0618 warnings), use reflection first, then fallback.
        private static string GetVcsModeString()
        {
            // New API (Unity 2021+): UnityEditor.VersionControlSettings.mode
            var t = Type.GetType("UnityEditor.VersionControlSettings, UnityEditor");
            if (t != null)
            {
                var p = t.GetProperty("mode", BindingFlags.Public | BindingFlags.Static);
                if (p != null)
                {
                    var val = p.GetValue(null, null);
                    return val?.ToString() ?? string.Empty;
                }
            }

#pragma warning disable CS0618
            return EditorSettings.externalVersionControl ?? string.Empty;
#pragma warning restore CS0618
        }

        private static void SetVcsMode(string desired)
        {
            var t = Type.GetType("UnityEditor.VersionControlSettings, UnityEditor");
            if (t != null)
            {
                var p = t.GetProperty("mode", BindingFlags.Public | BindingFlags.Static);
                if (p != null)
                {
                    var pt = p.PropertyType;

                    // If it's a string, set directly.
                    if (pt == typeof(string))
                    {
                        p.SetValue(null, desired, null);
                        return;
                    }

                    // If it's an enum, map "Visible Meta Files" -> VisibleMetaFiles etc.
                    if (pt.IsEnum)
                    {
                        var mapped = MapToEnumValue(pt, desired);
                        if (mapped != null)
                        {
                            p.SetValue(null, mapped, null);
                            return;
                        }
                    }
                }
            }

            // Fallback for older Unity versions.
#pragma warning disable CS0618
            EditorSettings.externalVersionControl = desired;
#pragma warning restore CS0618
        }

        private static object MapToEnumValue(Type enumType, string desired)
        {
            // common desired values coming from UI strings
            // "Visible Meta Files" -> VisibleMetaFiles
            // "Hidden Meta Files"  -> HiddenMetaFiles
            // "PlasticSCM"         -> PlasticSCM
            var desiredNorm = Normalize(desired);

            foreach (var name in Enum.GetNames(enumType))
            {
                if (Normalize(name) == desiredNorm)
                    return Enum.Parse(enumType, name);
            }

            // Try a few hard mappings
            var candidates = new[]
            {
                desired.Replace(" ", ""),                 // VisibleMetaFiles
                desired.Replace(" ", "").Replace("_",""), // VisibleMetaFiles
                desired.Replace(" ", "").Replace("-", "") // VisibleMetaFiles
            };

            foreach (var cand in candidates)
            {
                foreach (var name in Enum.GetNames(enumType))
                {
                    if (Normalize(name) == Normalize(cand))
                        return Enum.Parse(enumType, name);
                }
            }

            return null;
        }

        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Trim();
            s = s.Replace(" ", "").Replace("_", "").Replace("-", "");
            return s.ToLowerInvariant();
        }

        private static bool ContainsIgnoreCase(string haystack, string needle)
        {
            if (haystack == null || needle == null) return false;
            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
#endif
