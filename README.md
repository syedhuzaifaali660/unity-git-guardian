# Unity Git Guardian

Unity Git Guardian prevents one of the most common Unity team disasters:

> “It works on my machine… but after my teammate pulls/clones the repo, scenes/prefabs lose references.”

This usually happens when **`.meta` files** are missing/corrupted or when Unity project settings are not collaboration-friendly.

Unity Git Guardian helps you catch these issues **before you push/merge** — locally in Unity and optionally in GitHub Actions.

---

## What it checks

### ✅ Meta / GUID integrity (the big one)
- **Missing `.meta` files** for assets or folders (can break references on other machines)
- **Orphan `.meta` files** (meta exists but the asset/folder is missing)
- **Duplicate GUIDs** in `.meta` files (rare but can cause chaos)

> Unity stores GUIDs in `.meta` files. Scenes/prefabs reference GUIDs, not file names.
> If a `.meta` is missing and Unity regenerates it, the GUID changes → references can break.

### ✅ Unity collaboration settings
- **Force Text** serialization (recommended for merges)
- Version Control mode hints (Visible Meta Files / PlasticSCM)

### ✅ VCS hints (Git / Plastic)
- Detects **Git `.gitignore`** or **Plastic `ignore.conf`**
- Optional `.gitattributes` suggestion (only shown when it’s actually a Git repo)

### Ignores noisy false positives
Unity commonly ignores:
- dotfolders/dotfiles inside `Assets/` (example: `.github`, `.DS_Store`)
- folders ending with `~` (example: `Legacy~`)

Git Guardian ignores those too to avoid spam.

---

## Install (Unity Package Manager / UPM)

Add this to your Unity project `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.syedhuzaifaali.unity-git-guardian": "https://github.com/syedhuzaifaali660/unity-git-guardian.git?path=Packages/com.syedhuzaifaali.unity-git-guardian#v0.2.2"
  }
}
