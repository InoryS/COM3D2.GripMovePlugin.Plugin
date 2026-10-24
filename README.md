# COM3D2.GripMovePlugin.Plugin

Modified from the version distributed by [scarletkom](https://x.com/scarletkom)
at <https://ux.getuploader.com/scarletkom_mod/download/45>, incorporating some modernization and
custom changes.

Some code form [COM3D2.GripMovePluginXR_0.8.9.4](https://ux.getuploader.com/scarletkom_mod/download/45) by [ankokusamochi](https://x.com/ankokusamochi)

## Notes

Original reamde says, `改造、複製、再配布等はかまいませんが、本家及や更新版がリリースされた際はそちらに置き換えるようお願いします。`

So this is a modified version.

## Releasing / 发版

Pushing a `v*` tag builds both targets and publishes the release; nothing is uploaded by hand.

推送一个 `v*` tag 就会自动编译两个目标并发布 release，不需要手工上传任何东西。

```bash
# 1. 改版本号（三处一起改：[PluginVersion] / VERSION 常量 / csproj <Version>）
python3 .github/scripts/version.py --set 0.9.0.2

# 2. 提交
git commit -am "Bump version to v0.9.0.2"
git push

# 3. 打 tag 并推送，触发 Release workflow
git tag v0.9.0.2
git push origin v0.9.0.2
```

发版流程会校验 tag 和版本号一致（不一致直接失败，避免漏改版本号照样发出去），
编译两个目标框架，按白名单打包（只有插件 DLL + 默认 ini + README，游戏程序集绝不入包），
再按提交历史生成 release 正文。

需要重发一个已存在的 tag 时，在 Actions 页面手动运行 `Release`，
填 tag 名即可；勾选 `regenerate_notes` 会覆盖已经在 GitHub 上手动改过的正文。

To re-publish an existing tag, run the `Release` workflow manually with the tag name.
Tick `regenerate_notes` only if the release body should be regenerated.

