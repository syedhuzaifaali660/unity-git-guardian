# Unity Git Guardian (UPM Package)

Current package version: `0.0.2`

Open in Unity: **Tools → Git Guardian → Open**

## Install

If you want Unity to use the local files from this repo, add the package from disk in Package Manager and select:

`Packages/com.syedhuzaifaali.unity-git-guardian/package.json`

If you are installing from GitHub, make sure the URL points at the current release tag, not the old `v0.0.1` tag.

Example:

```json
"com.syedhuzaifaali.unity-git-guardian": "https://github.com/syedhuzaifaali660/unity-git-guardian.git?path=Packages/com.syedhuzaifaali.unity-git-guardian#v0.0.3"
```

## What it checks

- Missing `.meta` files and orphan `.meta` files
- Duplicate GUIDs
- Project settings for Force Text / Visible Meta Files
- Scene checks for missing scripts and null serialized references
- Git / Plastic ignore-file hints

## Notes

- Scene checks only see the saved scene file on disk. Save the scene before running checks.
- If Unity is still showing `v0.0.1`, remove the old Git package and re-add the package from disk or update the Git tag in the Package Manager URL to `v0.0.2`.
