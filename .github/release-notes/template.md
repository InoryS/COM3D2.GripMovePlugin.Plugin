# GripMovePlugin {{TAG}}

## Which file to download / 该下载哪个包

| Your game / 你的游戏 | Archive / 压缩包 |
| --- | --- |
| COM3D2 (any version / 所有版本) | `COM3D2.GripMovePlugin.Plugin-{{VERSION}}.zip` |
| COM3D2.5 **below** v3.41.0 / v3.41.0 以下 | `COM3D2.GripMovePlugin.Plugin-{{VERSION}}.zip` |
| COM3D2.5 **v3.41.0 or higher** / v3.41.0 及以上 | `COM3D25.GripMovePlugin.Plugin-{{VERSION}}.zip` |

If you are already running an older GripMovePlugin, check which dll sits in your
`Sybaris/UnityInjector` folder and take the same one — the two archives only differ in the
target framework, mixing them up is the usual cause of "plugin does not load".

已经在用旧版的话，看一眼 `Sybaris/UnityInjector` 目录里那个 dll 的文件名，下载同名的那个包即可。
两个包的区别只是目标框架，装错了插件不会加载。

## Install / 安装

1. Close the game. / 关闭游戏。
2. Extract the archive into the **game root folder** (the one containing `Sybaris`),
   merging with the existing folders. / 解压到**游戏根目录**（就是 `Sybaris` 所在的那一层），与已有目录合并。
3. After extracting you should have:
   `Sybaris/UnityInjector/COM3D2.GripMovePlugin.Plugin.dll` (or `COM3D25.…`) and
   `Sybaris/UnityInjector/Config/gripmoveplugin.ini`.
   / 解压后应该是 `Sybaris/UnityInjector/` 下有插件 dll、`Sybaris/UnityInjector/Config/` 下有 ini。
4. Start the game, hold the configured button to switch into `DIRECT` mode.
   / 启动游戏，按住设定按键切换到 `DIRECT` 模式。

`Sybaris/UnityInjector/Config/gripmoveplugin.ini` in the archive is the default configuration
file. If you have already tuned your settings, **keep your own copy** — extracting the archive
replaces it. / 包里的 `Sybaris/UnityInjector/Config/gripmoveplugin.ini` 是默认配置；
如果你已经改过设置，**请先备份自己的那一份**，解压会把它覆盖掉。

# Changes / 更改

{{CHANGELOG}}

<details>
<summary>中文说明（提交正文里没有 <code>zh:</code> 行时回落到英文标题）</summary>

{{CHANGELOG_ZH}}

</details>

**Full Changelog**: {{FULL_CHANGELOG_URL}}

---

This is a modified version of GripMovePlugin by scarletkom, with some code from
`COM3D2.GripMovePluginXR_0.8.9.4` by ankokusamochi. See the
[repository homepage](https://github.com/{{REPO}}) for details.
