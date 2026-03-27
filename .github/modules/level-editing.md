# 此文件用途：记录关卡编辑表面、用户交互、选择流程、变换工具和放置行为。

## 范围

本模块覆盖交互式编辑体验：包括关卡画布、选择覆盖层、变换 gizmo、放置模式，以及支撑这些交互的编辑器状态。

它位于 Avalonia UI 层与工作区核心之间。

## 主要职责

- 在桌面编辑器中渲染并承载关卡编辑画布。
- 将用户输入转换为选择、变换和放置操作。
- 维护放置、选择、实体操作等编辑模式。
- 显示吸附、选择和变换手柄相关的覆盖层。

## 关键入口

- `FantaniaApp/Views/Controls/Level/LevelCanvas.cs`：应用内可见的编辑表面。
- `FantaniaLib/Controls/Level/SelectionOverlay.cs`：选择可视化。
- `FantaniaLib/Controls/Level/TransformGizmoOverlay.cs`：变换 gizmo 编排。
- `FantaniaLib/Controls/Level/TranslateGizmo.cs`、`ScaleGizmo.cs`、`RotateGizmo.cs`：具体的变换工具行为。
- `FantaniaLib/Controls/Level/SnapOverlay.cs`：吸附辅助和引导显示。
- `FantaniaLib/Workspace/Level/EditorModule.cs`：后端编辑器状态和交互数据。
- `FantaniaLib/Workspace/Level/PlacementModule.cs`：由模板驱动的放置行为。

## 高层交互流程

1. 用户首先在应用层的关卡画布中进行操作。
2. 输入与交互状态会流入编辑器层和当前编辑模式。
3. 选择、变换和放置操作会通过 `EditorModule`、`LevelModule` 及相关操作对象修改工作区状态。
4. 覆盖层和渲染对象会刷新，以反映当前交互状态。

## 属于本模块的内容

- 实体拾取与选择行为。
- 变换 gizmo 逻辑和吸附行为。
- 放置预览和放置提交逻辑。
- 面向视口的编辑交互。

## 不属于本模块的内容

- shader、framebuffer 等核心渲染实现细节。
- 持久化和工作区生命周期。
- 工作区脚本中的纯内容定义。

## 常见改动热点

- 修复选择问题：查看 `EditorModule`、`SelectionOverlay` 和关卡画布输入处理。
- 修复变换行为：查看各类 gizmo 以及它们发出的操作对象。
- 修复放置模式或放置预览：查看 `PlacementModule` 以及关卡画布的模式管理。
- 增加新的编辑交互：从关卡画布的输入分发和对应 overlay 或 mode 类开始。

## 未来查询关键词

`level canvas`、`viewport`、`selection`、`transform`、`gizmo`、`snap`、`placement`、`editor mode`、鼠标世界坐标、`overlay`

## 关联文档

- `workspace-core.md`：底层 editor 和 level 模块。
- `rendering-and-lighting.md`：最终负责绘制编辑场景的渲染管线。
- `controls-and-ui-components.md`：编辑相关 UI 使用的可复用控件。