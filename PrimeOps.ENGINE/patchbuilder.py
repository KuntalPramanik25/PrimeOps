"""
build.py
--------
Compiles primeops_python_engine to .pyc files in ./dist/

Commands:
    python build.py               # compile to dist/
    python build.py --clean       # wipe dist/ then compile
    python build.py --output /custom/path
"""

import argparse
import compileall
import os
import shutil
import sys

SRC_PACKAGE = "primeops_python_engine"
DEFAULT_DIST = "dist"


def clean (dist_dir: str) -> None:
    if os.path.exists(dist_dir):
        shutil.rmtree(dist_dir)
        print(f"Cleaned: {dist_dir}")


def build (dist_dir: str) -> None:
    os.makedirs(dist_dir, exist_ok=True)

    print(f"Compiling {SRC_PACKAGE}/...")
    ok = compileall.compile_dir(SRC_PACKAGE, force=True, quiet=0, legacy=False)
    if not ok:
        print("Compilation failed.", file=sys.stderr)
        sys.exit(1)

    _copy_pyc_tree(SRC_PACKAGE, os.path.join(dist_dir, SRC_PACKAGE))
    print(f"\nBuild complete → {dist_dir}/")


def _copy_pyc_tree (src_root: str, dst_root: str) -> None:
    for dirpath, _, _ in os.walk(src_root):
        cache_dir = os.path.join(dirpath, "__pycache__")
        if not os.path.isdir(cache_dir):
            continue

        rel = os.path.relpath(dirpath, src_root)
        dst_pkg = os.path.join(dst_root, rel) if rel != "." else dst_root
        os.makedirs(dst_pkg, exist_ok=True)

        for fname in os.listdir(cache_dir):
            if not fname.endswith(".pyc"):
                continue
            src_pyc = os.path.join(cache_dir, fname)
            base = fname.split(".")[0] + ".pyc"
            dst_pyc = os.path.join(dst_pkg, base)
            shutil.copy2(src_pyc, dst_pyc)
            print(f"  → {dst_pyc}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Build PrimeOps Python Engine to .pyc")
    parser.add_argument("--output", default=DEFAULT_DIST)
    parser.add_argument("--clean", action="store_true")
    args = parser.parse_args()

    os.chdir(os.path.dirname(os.path.abspath(__file__)))

    if args.clean:
        clean(args.output)

    build(args.output)
