# Changelog

All notable changes to **Unity Git Guardian** will be documented in this file.

This project follows a simple versioning approach during early development (`v0.x.y`).

---

## v0.0.1

Initial public release.

### Added
- **Unity Editor window**
  - Menu: `Tools → Git Guardian → Open`
  - Buttons: **Run Checks**, **Copy Report**
- **Meta / GUID integrity checks**
  - Missing `.meta` files for assets (**Error**)
  - Missing `.meta` files for folders (**Warning**)
  - Orphan `.meta` files (meta exists but asset/folder missing) (**Warning**)
  - Duplicate GUID detection across `.meta` files (**Error**)
- **Project collaboration checks**
  - Asset Serialization mode (recommends **Force Text**, with one-click fix)
  - Version control mode hints (Visible Meta Files / PlasticSCM)
- **Git / Plastic support**
  - Detects Git `.gitignore` and Plastic `ignore.conf`
  - Optional `.gitattributes` suggestion (only when a `.git/` folder exists)
- **GitHub Actions support**
  - Composite action included at: `.github/actions/git-guardian`
  - Unity button to generate workflow file: `.github/workflows/git-guardian.yml`
- **Local CI-style checker**
  - Script: `Tools/guardian_check.py` (prints report + returns PASS/FAIL exit code)

### Notes
- To reduce false positives, Git Guardian ignores common Unity-ignored paths under `Assets/`:
  - dotfiles/dotfolders (e.g., `.github`, `.DS_Store`)
  - folders ending with `~` (e.g., `Legacy~`)
