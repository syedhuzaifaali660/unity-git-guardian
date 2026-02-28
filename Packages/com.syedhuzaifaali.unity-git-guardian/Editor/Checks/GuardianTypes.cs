using System;
using System.Collections.Generic;

namespace GitGuardian
{
    public enum GuardianSeverity { Info, Warning, Error }

    public sealed class GuardianIssue
    {
        public string Id;
        public GuardianSeverity Severity;
        public string Title;
        public string Details;
        public string RelatedPath; // optional
        public Func<bool> Fix;     // optional fix action

        public GuardianIssue(string id, GuardianSeverity severity, string title, string details, string relatedPath = null, Func<bool> fix = null)
        {
            Id = id;
            Severity = severity;
            Title = title;
            Details = details;
            RelatedPath = relatedPath;
            Fix = fix;
        }
    }

    public interface IGuardianCheck
    {
        string Name { get; }
        List<GuardianIssue> Run();
    }

    internal static class UnityIgnored
    {
        public static bool IsIgnoredName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.StartsWith(".") || name.EndsWith("~");
        }

        public static bool IsUnderIgnoredFolder(string assetsDir, string fullPath)
        {
            var a = assetsDir.Replace('\\', '/').TrimEnd('/');
            var p = fullPath.Replace('\\', '/');

            if (!p.StartsWith(a)) return false;

            var rel = p.Substring(a.Length).TrimStart('/');
            if (string.IsNullOrEmpty(rel)) return false;

            var parts = rel.Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (IsIgnoredName(parts[i]))
                    return true;
            }
            return false;
        }
    }
}
