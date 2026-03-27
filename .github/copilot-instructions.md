# 此文件用途：定义 GitHub Copilot 在此仓库中需要始终遵循的规则。

## 基础规范
- 请尽量用中文回复，除非用户明确要求使用英文。
- 如果你查阅了这些底层逻辑，请在回复的尾巴加一个表情符号'wOw'，以表明你已经理解并会遵守这些规则。
- 如果遇到了我没有提到的新加或者需要改动的地方，先不要写代码，而是先在回复里说明你发现了什么问题，为什么需要改动，以及你打算怎么改动。

## 主要目标

本仓库在 `.github/modules/` 下维护了一套模块知识库。
当用户询问架构、某个子系统、某个目录、某个功能区域，或者提出一个明显属于某个模块的修改请求时，应当先查阅模块知识库，再进入更深入的代码分析。

## 必须遵循的查阅流程

1. 先读取 `.github/modules/index.md`。
2. 根据模块名、关键词、代表性文件以及当前活动文件路径来匹配目标模块。
3. 再读取 `.github/modules/` 下最相关的 1 到 3 份模块文档。
4. 最后再读取实际代码，并用代码验证文档描述是否仍然成立。

## 匹配规则

- 如果用户提到 `Avalonia`、`App`、`MainWindow`、`View`、`ViewModel`、工作区窗口或桌面 UI，优先从 `app-shell-and-mvvm.md` 开始。
- 如果用户提到 `workspace`、`solution`、`database`、`placement`、`editor module`、`undo` 或项目生命周期，优先从 `workspace-core.md` 开始。
- 如果用户提到 `level`、`selection`、`transform`、`canvas`、`gizmo`、`placement mode` 或视口编辑，优先从 `level-editing.md` 开始。
- 如果用户提到 `render`、`pipeline`、`shader`、`material`、`framebuffer`、`light`、`shadow` 或后处理，优先从 `rendering-and-lighting.md` 开始。
- 如果用户提到 `control`、`message box`、`field editor`、`popup`、`toolbar` 或可复用的 Avalonia 组件，优先从 `controls-and-ui-components.md` 开始。
- 如果用户提到 `Lua`、`MoonSharp`、`export`、`pipeline_hook`、`export_settings`、`gamedata` 或生成输出，优先从 `scripting-and-export.md` 开始。
- 如果用户提到 `PixelGameWorkspace`、`scripts/entities`、`levels`、`textures`、`shaders` 或工作区内容，优先从 `workspace-content.md` 开始。

## 冲突处理

- 将模块文档视为整理过的上下文，而不是绝对真相。
- 如果文档与代码不一致，以代码为准。
- 如果发现文档与实现存在有意义的漂移，需要在回复中指出。
- 如果你做了影响模块边界、入口点或数据流的结构性修改，需要同时更新对应的模块文档，必要时也要更新 `.github/modules/index.md`。

## 响应行为

- 在总结代码库时，优先使用模块文档中的术语。
- 当用户请求存在歧义时，先读取 `.github/modules/index.md` 和最可能相关的两份模块文档，再简要说明你的判断假设。
- 对于代码修改，除非用户明确要求，否则避免进行大范围重构。