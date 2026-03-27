# 此文件用途：在进入更深入的代码分析之前，把后续请求路由到正确的模块文档。

## GitHub Copilot 应如何使用这份索引

1. 先根据功能名、目录名、文件路径和关键词识别用户要问的目标区域。
2. 打开下表中对应的模块文档。
3. 如果请求跨越多个区域，先读主模块文档，再读一份辅助模块文档。
4. 在对实现细节下结论之前，先用当前代码验证文档是否仍然成立。

## 模块路由表

| 模块 | 当用户提到这些内容时使用 | 代表性入口文件 | 对应文档 |
|---|---|---|---|
| 应用壳层与 MVVM | 应用启动、Avalonia、主窗口、ViewModel、View、菜单、工具栏、本地化、最近工作区、桌面 UI | `FantaniaApp/Program.cs`、`FantaniaApp/App.axaml.cs`、`FantaniaApp/ViewModels/MainWindowViewModel.cs`、`FantaniaApp/Views/MainWindow.axaml` | `app-shell-and-mvvm.md` |
| 工作区核心 | 工作区生命周期、solution、database、placement module、editor module、log module、scripting module、保存打开、undo | `FantaniaLib/Workspace/Workspace.cs`、`FantaniaLib/Workspace/WorkspaceSolution.cs`、`FantaniaLib/Workspace/Database/DatabaseModule.cs`、`FantaniaLib/Workspace/Level/LevelModule.cs`、`FantaniaLib/Workspace/Scripting/ScriptingModule.cs` | `workspace-core.md` |
| 关卡编辑 | level canvas、viewport、selection、transform、gizmo、placement mode、snapping、编辑覆盖层、关卡交互 | `FantaniaApp/Views/Controls/Level/LevelCanvas.cs`、`FantaniaLib/Controls/Level/SelectionOverlay.cs`、`FantaniaLib/Controls/Level/TransformGizmoOverlay.cs`、`FantaniaLib/Workspace/Level/EditorModule.cs` | `level-editing.md` |
| 渲染与光照 | render pipeline、framebuffer、shader、material、camera、draw stages、light culling、shadow、post process | `FantaniaLib/Rendering/Pipeline/ConfigurableRenderPipeline.cs`、`FantaniaLib/Rendering/Camera2D.cs`、`FantaniaLib/Rendering/RenderInfo.cs`、`PixelGameWorkspace/scripts/pipeline_hook.lua` | `rendering-and-lighting.md` |
| 控件与 UI 组件 | 可复用控件、message box、文件选择、option box、field editor、overlay、toolbar control | `FantaniaLib/Controls/FileSelectBox.axaml.cs`、`FantaniaLib/Controls/OptionBox.axaml.cs`、`FantaniaLib/Controls/Editing/EditFieldsView.axaml.cs`、`FantaniaLib/Controls/MessageBox/MessageBoxHelper.cs` | `controls-and-ui-components.md` |
| 脚本与导出 | Lua 集成、MoonSharp、导出设置、导出管线、生成文件、游戏数据映射、pipeline setup | `FantaniaLib/Workspace/Scripting/ScriptingModule.cs`、`FantaniaLib/Workspace/Export/ExportContext.cs`、`PixelGameWorkspace/scripts/export_settings.lua`、`PixelGameWorkspace/scripts/pipeline_setup.lua` | `scripting-and-export.md` |
| 工作区内容 | PixelGameWorkspace 结构、实体脚本、关卡、资源、示例数据、textures、shaders、workspace.json | `PixelGameWorkspace/workspace.json`、`PixelGameWorkspace/scripts/entities/point_light.lua`、`PixelGameWorkspace/scripts/gamedata/global_parameter.lua`、`PixelGameWorkspace/levels/` | `workspace-content.md` |

## 跨模块说明

- `FantaniaApp` 是桌面编辑器壳层和表现层。
- `FantaniaLib` 是核心引擎/编辑器库，包含大部分可复用实现。
- `PixelGameWorkspace` 是实际内容工程或示例工作区，为编辑器和运行时管线提供脚本、资源、关卡和导出配置。

## 查询示例

- 如果用户问 `点光源`、`light source` 或 `pipeline_hook`，先打开 `rendering-and-lighting.md` 和 `workspace-content.md`。
- 如果用户问 `Workspace.Save`、打开工作区或持久化问题，先打开 `workspace-core.md`。
- 如果用户问 `MainWindow`、工具栏或命令绑定，先打开 `app-shell-and-mvvm.md`，必要时再看 `controls-and-ui-components.md`。
- 如果用户问实体放置、选择或画布编辑行为，先打开 `level-editing.md` 和 `workspace-core.md`。