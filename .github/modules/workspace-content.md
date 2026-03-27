# 此文件用途：记录 Fantania 所使用的具体内容工程 PixelGameWorkspace 的结构和定位。

## 范围

本模块聚焦 `PixelGameWorkspace/`，它是本仓库中面向内容的一侧。
其中包含工作区元数据、关卡、Lua 脚本、shader、texture 以及其他会被编辑器和导出管线加载的资源。

## 主要职责

- 定义工作区级内容文件和目录约定。
- 提供被编辑器和运行时管线消费的实体脚本与游戏数据脚本。
- 提供工作区自定义的渲染管线脚本和导出设置。
- 作为 Fantania 工作区数据组织方式的具体示例。

## 重要目录

- `PixelGameWorkspace/workspace.json`：工作区根 solution 文件。
- `PixelGameWorkspace/levels/`：关卡数据。
- `PixelGameWorkspace/scripts/entities/`：实体定义，例如 sprite、light、spawn 和 tiled-platform 等对象。
- `PixelGameWorkspace/scripts/gamedata/`：游戏数据声明和全局配置。
- `PixelGameWorkspace/scripts/export_settings.lua`：导出行为定义。
- `PixelGameWorkspace/scripts/pipeline_hook.lua` 与 `pipeline_setup.lua`：渲染设置。
- `PixelGameWorkspace/textures/`：工作区纹理资源。
- `PixelGameWorkspace/shaders/`：工作区 shader 资源。
- `PixelGameWorkspace/lang/`：本地化或工作区文本资源。

## 示例内容说明

- `scripts/entities/point_light.lua` 定义了一个点光源 placement，包含 lighting layer、color、radius 和 intensity 等可编辑字段。
- 该脚本同时返回一个编辑器可见的指示器 renderable，以及一个用于 tiled light culling stage 的光照 renderable。
- 这些工作区脚本不只是静态数据，它们还主动定义了编辑器行为、渲染行为和导出行为。

## 常见改动热点

- 新增实体类型：从 `scripts/entities/` 开始，然后继续验证 placement、render 和 export 的联动。
- 修改项目级导出数据：查看 `scripts/export_settings.lua` 和 `scripts/gamedata/`。
- 修改工作区渲染行为：查看 `scripts/pipeline_hook.lua`、`scripts/pipeline_setup.lua` 以及相关 shader。
- 排查资源路径问题：查看工作区目录约定以及解析工作区相对路径的代码。

## 未来查询关键词

`PixelGameWorkspace`、`entity script`、点光源、`gamedata`、`levels`、`textures`、`shaders`、`workspace.json`、`assets`、工作区内容

## 关联文档

- `scripting-and-export.md`：消费这些内容的脚本引擎和导出机制。
- `rendering-and-lighting.md`：部分由工作区脚本驱动的渲染管线。
- `workspace-core.md`：工作区目录约定和生命周期。