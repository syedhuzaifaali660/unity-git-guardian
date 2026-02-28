# Unity Git Guardian

Unity Git Guardian helps teams prevent **broken Unity projects after pulling/cloning** by catching common repo-integrity issues early (locally and in CI):

- Missing `.meta` files (assets/folders)
- Orphan `.meta` files (meta exists but asset is gone)
- Duplicate GUIDs inside `.meta` files
- Unity collaboration settings checks (e.g., **Force Text**)
- VCS hints for Git (`.gitignore`) and Plastic (`ignore.conf`)
- Optional: installs a GitHub Actions workflow into your project

> Why this matters: Unity uses GUIDs from `.meta` files to keep references stable. If metas are missing or GUIDs collide, scenes/prefabs can lose references on other machines.

---

## Features

✅ **Unity Editor Window**  
`Tools → Git Guardian → Open`

✅ **GitHub Actions support** (CI gate on pull requests / pushes)  
A workflow file is required in the user’s repo — Git Guardian can generate it.

✅ **Works with Git or Plastic**  
- Git: `.gitignore`
- Plastic: `ignore.conf`

---

## Install (Unity Package Manager / UPM)

In your Unity project, open `Packages/manifest.json` and add:

```json
{
  "dependencies": {
    "com.syedhuzaifaali.unity-git-guardian": "https://github.com/syedhuzaifaali660/unity-git-guardian.git?path=Packages/com.syedhuzaifaali.unity-git-guardian#v0.2.0"
  }
}
