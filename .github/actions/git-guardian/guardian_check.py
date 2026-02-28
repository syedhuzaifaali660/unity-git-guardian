#!/usr/bin/env python3
import os
import sys

ASSETS_DIR = os.path.join(os.getcwd(), "Assets")
IGNORE_DIRS = {".git", "Library", "Temp", "Logs", "obj", "Build", "Builds"}

def is_unity_ignored(name: str) -> bool:
    return name.startswith(".") or name.endswith("~")

def walk_assets():
    for root, dirs, files in os.walk(ASSETS_DIR):
        dirs[:] = [d for d in dirs if d not in IGNORE_DIRS and not is_unity_ignored(d)]
        yield root, dirs, files

def read_guid(meta_path):
    try:
        with open(meta_path, "r", encoding="utf-8", errors="ignore") as f:
            for line in f:
                if line.startswith("guid:"):
                    return line.split("guid:", 1)[1].strip()
    except Exception:
        return None
    return None

def main():
    if not os.path.isdir(ASSETS_DIR):
        print("ERROR: Assets/ folder not found. Set project_path to your Unity project root.")
        return 2

    missing_meta = []
    orphan_meta = []
    guid_map = {}
    duplicate_guids = []

    for root, dirs, files in walk_assets():
        for d in dirs:
            path = os.path.join(root, d)
            if not os.path.exists(path + ".meta"):
                missing_meta.append(path)

        for f in files:
            if f.endswith(".meta"):
                continue
            if is_unity_ignored(f):
                continue
            path = os.path.join(root, f)
            if not os.path.exists(path + ".meta"):
                missing_meta.append(path)

    for root, _, files in walk_assets():
        for f in files:
            if not f.endswith(".meta"):
                continue
            meta_path = os.path.join(root, f)
            target = meta_path[:-5]
            target_name = os.path.basename(target)

            if is_unity_ignored(target_name):
                continue

            if not os.path.exists(target):
                orphan_meta.append(meta_path)

            guid = read_guid(meta_path)
            if guid:
                if guid in guid_map:
                    duplicate_guids.append((guid, guid_map[guid], meta_path))
                else:
                    guid_map[guid] = meta_path

    ok = True

    if missing_meta:
        ok = False
        print("\nERROR: Missing .meta files:")
        for p in missing_meta[:200]:
            print(" -", os.path.relpath(p))
        if len(missing_meta) > 200:
            print(f" ... and {len(missing_meta)-200} more")

    if duplicate_guids:
        ok = False
        print("\nERROR: Duplicate GUIDs:")
        for guid, a, b in duplicate_guids[:100]:
            print(f" - {guid}\n   {os.path.relpath(a)}\n   {os.path.relpath(b)}")
        if len(duplicate_guids) > 100:
            print(f" ... and {len(duplicate_guids)-100} more")

    if orphan_meta:
        print("\nWARNING: Orphan .meta files (not always fatal):")
        for p in orphan_meta[:200]:
            print(" -", os.path.relpath(p))
        if len(orphan_meta) > 200:
            print(f" ... and {len(orphan_meta)-200} more")

    print("\nGit Guardian:", "PASS ✅" if ok else "FAIL ❌")
    return 0 if ok else 1

if __name__ == "__main__":
    sys.exit(main())
