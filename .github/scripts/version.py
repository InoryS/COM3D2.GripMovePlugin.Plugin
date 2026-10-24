#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# Function: 读取 / 同步插件版本号 / Read and sync the plugin version
# License: BSD-3
"""插件版本号的读取与同步。

版本号在这个仓库里重复出现在三处，任何一处漏改都会让包名和游戏里显示的版本对不上：

  1. GripMovePlugin.cs 的 `[PluginVersion("x.y.z")]`  ← 权威来源，游戏内显示的就是它
  2. GripMovePlugin.cs 的 `public const string VERSION = "x.y.z";`
  3. csproj 的 `<Version>x.y.z</Version>`（决定程序集版本）

用法 / Usage:

    python3 .github/scripts/version.py            # 打印版本，并校验三处一致
    python3 .github/scripts/version.py --check-tag v0.9.0.1   # 额外校验与 tag 一致
    python3 .github/scripts/version.py --set 0.9.0.2          # 三处一起改成 0.9.0.2

发版流程只依赖这里：tag 必须等于 `v` + 版本号，否则 release workflow 直接失败，
避免漏改版本号却照样把包发出去。

The version appears in three places; the PluginVersion attribute is authoritative
(it is what the game shows). A release compares it with the tag so a forgotten
bump fails the build instead of shipping a package under the wrong version.
"""

import argparse
import os
import re
import sys

SOURCE = os.path.join(
    "COM3D2.GripMovePlugin.Plugin", "COM3D2", "GripMovePlugin", "Plugin", "GripMovePlugin.cs")
CSPROJ = os.path.join(
    "COM3D2.GripMovePlugin.Plugin", "COM3D2.GripMovePlugin.Plugin.csproj")

VERSION_RE = r"[0-9][0-9A-Za-z.\-]*"

# 三处版本号各自的匹配式 / One pattern per place the version is written down
PATTERNS = {
    "attribute": re.compile(r'(\[PluginVersion\(")(' + VERSION_RE + r')("\)\])'),
    "constant": re.compile(r'(public\s+const\s+string\s+VERSION\s*=\s*")(' + VERSION_RE + r')(")'),
    "csproj": re.compile(r"(<Version>)(" + VERSION_RE + r")(</Version>)"),
}


def read_text(path):
    """按原编码读入，保留 BOM 与换行符 / Read preserving BOM and line endings."""
    with open(path, "rb") as f:
        raw = f.read()
    has_bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig" if has_bom else "utf-8")
    return text, has_bom


def write_text(path, text, has_bom):
    """写回时保持原来的 BOM 状态，不制造无意义的 diff / Keep the original BOM state."""
    with open(path, "wb") as f:
        f.write(b"\xef\xbb\xbf" if has_bom else b"")
        f.write(text.encode("utf-8"))


def collect(repo_root):
    """返回 {来源: 版本号} / Return {source: version}."""
    found = {}
    for key, path in (("attribute", SOURCE), ("constant", SOURCE), ("csproj", CSPROJ)):
        full = os.path.join(repo_root, path)
        if not os.path.isfile(full):
            sys.exit(f"::error::file not found: {full}")
        text, _ = read_text(full)
        match = PATTERNS[key].search(text)
        if not match:
            sys.exit(f"::error::cannot find the version in {path} ({key})")
        found[key] = match.group(2)
    return found


def set_version(repo_root, version):
    """把三处一起改成 version / Write the same version into all three places."""
    if not re.fullmatch(VERSION_RE, version):
        sys.exit(f"::error::not a valid version: {version}")
    for key, path in (("attribute", SOURCE), ("constant", SOURCE), ("csproj", CSPROJ)):
        full = os.path.join(repo_root, path)
        text, has_bom = read_text(full)
        new_text, count = PATTERNS[key].subn(
            lambda m: m.group(1) + version + m.group(3), text)
        if count != 1:
            sys.exit(f"::error::expected exactly one {key} version in {path}, found {count}")
        write_text(full, new_text, has_bom)
    print(f"set version to {version} in {SOURCE} and {CSPROJ}")


def main():
    parser = argparse.ArgumentParser(description="Read or sync the plugin version")
    parser.add_argument("--repo-root", default=".", help="仓库根目录 / repository root")
    parser.add_argument("--check-tag", help="校验版本号与这个 tag 一致 / verify against this tag")
    parser.add_argument("--set", dest="new_version", help="写入新版本号 / write a new version")
    args = parser.parse_args()

    repo_root = os.path.abspath(args.repo_root)

    if args.new_version:
        set_version(repo_root, args.new_version)

    found = collect(repo_root)
    versions = sorted(set(found.values()))
    if len(versions) != 1:
        detail = ", ".join(f"{k}={v}" for k, v in found.items())
        sys.exit(f"::error::version mismatch between the three places: {detail}")
    version = versions[0]

    if args.check_tag:
        tag = args.check_tag
        if tag.lower().startswith("v"):
            tag_version = tag[1:]
        else:
            sys.exit(f"::error::tag {tag} does not start with 'v'")
        if tag_version != version:
            sys.exit(f"::error::version is {version} but tag is {tag}, "
                     f"run: python3 .github/scripts/version.py --set {tag_version}")

    print(version)


if __name__ == "__main__":
    main()
