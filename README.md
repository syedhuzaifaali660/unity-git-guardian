# Unity Git Guardian

Unity Git Guardian helps teams prevent **broken Unity projects after pulling/cloning** by catching common repo-integrity issues early (locally and in CI):

- Missing `.meta` files (assets/folders)
- Orphan `.meta` files (meta exists but asset is gone)
- Duplicate GUIDs inside `.meta` files
- Unity collaboration settings checks (e.g., **Force Text**)
- VCS hints for Git (`.gitignore`) and Plastic (`ignore.conf`)
- Optional: installs a GitHub Actions workflow into your project

> Why this matters: Unity uses GUIDs from `.meta` files to keep references stable. If metas are missing or GUIDs collide, scenes/prefabs can lose references on other machines.

## Install (UPM)

```json
{
  "dependencies": {
    "com.syedhuzaifaali.unity-git-guardian": "https://github.com/syedhuzaifaali660/unity-git-guardian.git?path=Packages/com.syedhuzaifaali.unity-git-guardian#v0.2.2"
  }
}
```

## Use
In Unity: **Tools → Git Guardian → Open**

## CI
In Unity window, click **Install GitHub Workflow**, then commit `.github/workflows/git-guardian.yml`.
