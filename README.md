# Unity Git Guardian

Unity Git Guardian helps teams avoid one of the most common Unity collaboration issues:

> “It works on my machine… but after my teammate pulls/clones the repo, scenes/prefabs lose references.”

This usually happens when **Unity `.meta` files** are missing or inconsistent (Unity uses GUIDs from `.meta` files to keep references stable).

Git Guardian catches these problems **before you merge** (inside Unity) and can also run in **GitHub Actions**.

---

## What it checks

### ✅ Meta / GUID integrity
- **Missing `.meta` files** for assets or folders (can break references on other machines)
- **Orphan `.meta` files** (meta exists but the asset/folder is missing)
- **Duplicate GUIDs** inside `.meta` files (rare but dangerous)

### ✅ Project collaboration settings
- **Force Text** serialization (recommended for Git merges)
- Version control mode hints (Visible Meta Files / PlasticSCM)

### ✅ VCS hints (Git / Plastic)
- Detects Git `.gitignore` or Plastic `ignore.conf`
- Optional `.gitattributes` hint (only shown when a `.git/` folder exists)

### Noise reduction
Unity commonly ignores:
- dotfolders/dotfiles inside `Assets/` (example: `.github`, `.DS_Store`)
- folders ending with `~` (example: `Legacy~`)

Git Guardian ignores those too to avoid false positives.

---

## Install (Unity Package Manager / UPM)

Add this to your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.syedhuzaifaali.unity-git-guardian": "https://github.com/syedhuzaifaali660/unity-git-guardian.git?path=Packages/com.syedhuzaifaali.unity-git-guardian#v0.0.1"
  }
}
