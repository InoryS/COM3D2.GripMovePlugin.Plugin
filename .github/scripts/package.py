#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# Function: 组装单个目标（COM3D2 / COM3D25）的发布 zip / Assemble the release zip for one target
# License: BSD-3
"""组装发布 zip。

解压到游戏根目录后的布局，和此前手工发布的包保持一致：

    COM3D2
      UnityInjector/COM3D2.GripMovePlugin.Plugin.dll
      UnityInjector/Config/gripmoveplugin.ini

    COM3D25
      UnityInjector/COM3D25.GripMovePlugin.Plugin.dll
      Config/gripmoveplugin.ini

    README.md（包根目录，方便解压前先看清楚该用哪个包）

两个要点 / Two things worth knowing:

* **只打包插件 DLL 本体。** dotnet build 的输出目录里还有从 NuGet GameLibs 复制过来的
  Assembly-CSharp.dll、UnityEngine.UI.dll 以及 UnityInjector.dll、ExIni.dll，
  这些是游戏/加载器自带的，打进包里会覆盖玩家游戏目录里的文件，直接把游戏搞坏。
  脚本按白名单打包，并且事后自检包里没有这些东西。

* **COM3D25 的 DLL 要改名。** csproj 两个平台共用一个 AssemblyName，输出都是
  COM3D2.GripMovePlugin.Plugin.dll；而 COM3D2.5 用户此前装的包是
  COM3D25.GripMovePlugin.Plugin.dll。不改名的话升级后 UnityInjector 目录里会同时存在
  两个 DLL，插件被加载两次，界面和按键全部翻倍。

Only the plugin dll itself is packaged: the build output also contains game assemblies
copied from NuGet GameLibs, and shipping those would overwrite the player's game files.
The COM3D25 dll is renamed because that is the file name existing users already have.
"""

import argparse
import os
import sys
import time
import zipfile

SOURCE_DLL = "COM3D2.GripMovePlugin.Plugin.dll"

# 目标定义 / Target definitions
TARGETS = {
    "COM3D2": {
        "build_subdir": os.path.join("COM3D2", "net35"),
        "dll_name": "COM3D2.GripMovePlugin.Plugin.dll",
        "ini_path": "UnityInjector/Config/gripmoveplugin.ini",
        "zip_name": "COM3D2.GripMovePlugin.Plugin",
    },
    "COM3D25": {
        "build_subdir": os.path.join("COM3D25", "net48"),
        "dll_name": "COM3D25.GripMovePlugin.Plugin.dll",
        "ini_path": "Config/gripmoveplugin.ini",
        "zip_name": "COM3D25.GripMovePlugin.Plugin",
    },
}

# 绝对不允许出现在包里的东西 / Never allowed inside the archive
FORBIDDEN_NAMES = {
    "assembly-csharp.dll",
    "assembly-csharp-firstpass.dll",
    "unityengine.dll",
    "unityengine.ui.dll",
    "unityinjector.dll",
    "exini.dll",
    "pluginext.dll",
}

INI_SOURCE = os.path.join("Config", "gripmoveplugin.ini")
README_SOURCE = "README.md"


def validate_ini(data, path):
    """拦住 `key = value` 这种写法。

    ExIni **不会 trim**：`MoveSelfButton = Trigger` 解析出来的键是 `"MoveSelfButton "`、
    值是 `" Trigger"`，于是 `GetKey("MoveSelfButton")` 拿不到，整份配置静默失效、插件一直
    用代码里的默认值，而且保存设置时还会多写一条同名键出来。等号两边不许有空格。

    ExIni does not trim: `key = value` yields the key "key " and the value " value", so every
    lookup misses and the whole config silently falls back to the built-in defaults.
    """
    bad = []
    for lineno, raw_line in enumerate(data.decode("utf-8-sig").splitlines(), 1):
        stripped = raw_line.strip()
        if not stripped or stripped[0] in "#[" or "=" not in stripped:
            continue
        key, _, value = stripped.partition("=")
        if key != key.strip() or value != value.strip():
            bad.append(f"{path}:{lineno}: {raw_line!r}")
    if bad:
        sys.exit("::error::ExIni does not trim, write key=value with no spaces around '=':\n"
                 + "\n".join(bad))


def zip_date_time(path=None):
    """zip 条目时间戳：文件用自己的修改时间，目录用打包时刻。
    Timestamp for a zip entry: a file keeps its own modification time, a directory gets
    the packaging time."""
    stamp = os.path.getmtime(path) if path else time.time()
    local = time.localtime(stamp)
    # zip 的时间字段最早只能表示 1980 年 / the zip timestamp cannot represent years before 1980
    if local.tm_year < 1980:
        return (1980, 1, 1, 0, 0, 0)
    return local[:6]


def add_dir(zf, name, added):
    """补一个目录条目（含各级父目录），已加过的跳过。
    Add a directory entry (and its parents), skipping the ones already written."""
    parts = [p for p in name.split("/") if p]
    for i in range(len(parts)):
        path = "/".join(parts[: i + 1]) + "/"
        if path in added:
            continue
        added.add(path)
        info = zipfile.ZipInfo(path, date_time=zip_date_time())
        info.external_attr = (0o40755 << 16) | 0x10
        info.compress_type = zipfile.ZIP_STORED
        zf.writestr(info, b"")


def add_bytes(zf, arcname, data):
    info = zipfile.ZipInfo(arcname, date_time=zip_date_time())
    info.external_attr = 0o100644 << 16
    info.compress_type = zipfile.ZIP_DEFLATED
    zf.writestr(info, data, compresslevel=9)


def add_file(zf, src, arcname):
    with open(src, "rb") as f:
        add_bytes(zf, arcname, f.read())


def main():
    parser = argparse.ArgumentParser(description="Assemble a release zip")
    parser.add_argument("--target", required=True, choices=sorted(TARGETS))
    parser.add_argument("--version", required=True, help="例如 0.9.0.1 / e.g. 0.9.0.1")
    parser.add_argument("--build-root", required=True,
                        help="dotnet build 的输出根目录（bin/Release）/ dotnet build output root")
    parser.add_argument("--repo-root", default=".", help="仓库根目录 / repository root")
    parser.add_argument("--output-dir", default="dist", help="zip 输出目录 / output directory")
    parser.add_argument("--no-config", action="store_true",
                        help="不打包默认 ini（它会覆盖玩家已改过的设置）/ skip the default ini, "
                             "which overwrites the settings a player already changed")
    args = parser.parse_args()

    target = TARGETS[args.target]
    repo_root = os.path.abspath(args.repo_root)
    build_dir = os.path.join(os.path.abspath(args.build_root), target["build_subdir"])

    if not os.path.isdir(build_dir):
        sys.exit(f"::error::build output not found: {build_dir}")

    plugin_dll = os.path.join(build_dir, SOURCE_DLL)
    if not os.path.isfile(plugin_dll):
        sys.exit(f"::error::{SOURCE_DLL} not found in {build_dir}, "
                 f"got {sorted(os.listdir(build_dir))}")

    ini_source = os.path.join(repo_root, INI_SOURCE)
    if not args.no_config and not os.path.isfile(ini_source):
        sys.exit(f"::error::missing file: {ini_source}")

    readme = os.path.join(repo_root, README_SOURCE)
    if not os.path.isfile(readme):
        sys.exit(f"::error::missing file: {readme}")

    os.makedirs(os.path.join(repo_root, args.output_dir), exist_ok=True)
    zip_path = os.path.join(
        repo_root, args.output_dir, f"{target['zip_name']}-{args.version}.zip")

    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        dirs = set()
        add_dir(zf, "UnityInjector", dirs)

        # 插件本体，必要时改名 / the plugin itself, renamed when the target needs it
        add_file(zf, plugin_dll, f"UnityInjector/{target['dll_name']}")

        if not args.no_config:
            # 仓库里的 ini 是 LF（.gitattributes 会把文本规范化），游戏在 Windows 上，
            # 这里转成 CRLF 再打包，避免玩家用记事本打开时看到连成一片的内容
            # The repo copy is LF because of .gitattributes normalization; the game runs on
            # Windows, so it is converted to CRLF here to stay readable in notepad
            add_dir(zf, os.path.dirname(target["ini_path"]), dirs)
            with open(ini_source, "rb") as f:
                ini_bytes = f.read()
            validate_ini(ini_bytes, INI_SOURCE)
            ini_bytes = ini_bytes.replace(b"\r\n", b"\n").replace(b"\n", b"\r\n")
            add_bytes(zf, target["ini_path"], ini_bytes)

        add_file(zf, readme, "README.md")

    # 自检：包里的条目必须和上面写进去的完全一致，多一个少一个都算失败。
    # 这一层是给「以后有人改成整目录拷贝」准备的兜底，防止游戏程序集混进来
    # Self-check: the archive must contain exactly what was written above. This is the guard
    # for anyone who later switches to copying a whole directory, so no game assembly sneaks in
    with zipfile.ZipFile(zip_path) as zf:
        names = zf.namelist()

    expected = {"UnityInjector/", f"UnityInjector/{target['dll_name']}", "README.md"}
    if not args.no_config:
        ini_dir = os.path.dirname(target["ini_path"])
        parts = [p for p in ini_dir.split("/") if p]
        for i in range(len(parts)):
            expected.add("/".join(parts[: i + 1]) + "/")
        expected.add(target["ini_path"])

    bad = [f"{n} (unexpected)" for n in names if n not in expected]
    bad += [f"{n} (missing)" for n in expected if n not in names]
    bad += [f"{n} (game/loader assembly or debug symbol)"
            for n in names if os.path.basename(n).lower() in FORBIDDEN_NAMES
            or n.lower().endswith(".pdb")]
    if bad:
        sys.exit(f"::error::unexpected archive contents in {zip_path}: {bad}")

    size = os.path.getsize(zip_path)
    print(f"built {zip_path} ({len(names)} entries, {size} bytes)")
    for name in names:
        print(f"  {name}")


if __name__ == "__main__":
    main()
