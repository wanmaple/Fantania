# 此文件用途：记录桌面应用壳层、Avalonia 组合结构以及主要的 MVVM 表现层。

## 范围

本模块覆盖 Fantania 编辑器的启动路径、顶层 Avalonia 应用接线、主窗口，以及驱动工作区相关 UI 的 View/ViewModel 组合结构。

它不负责定义核心工作区逻辑。核心工作区逻辑位于 `FantaniaLib`，并在 `workspace-core.md` 中总结。

## 主要职责

- 启动 Avalonia 桌面应用并初始化崩溃处理。
- 创建并承载主窗口。
- 通过 ViewModel 将 UI 操作绑定到工作区操作。
- 管理面向编辑器的流程，例如打开、保存、导出、最近工作区、对话框和本地化字符串。

## 关键入口

- `FantaniaApp/Program.cs`：桌面应用入口和 Avalonia 启动代码。
- `FantaniaApp/App.axaml` 与 `FantaniaApp/App.axaml.cs`：应用资源和应用级初始化。
- `FantaniaApp/ViewModels/MainWindowViewModel.cs`：工作区打开、保存、导出、编辑等主命令入口。
- `FantaniaApp/Views/MainWindow.axaml` 与 `FantaniaApp/Views/MainWindow.axaml.cs`：主壳窗口和 UI 组合根。
- `FantaniaApp/Models/FantaniaWorkspace.cs`：应用层工作区包装或对核心工作区抽象的具体绑定。
- `FantaniaApp/Localization/LocalizationHelper.cs`：供 UI 和对话框使用的本地化字符串桥接。

## 高层流程

1. `Program.Main` 初始化崩溃日志并启动 Avalonia。
2. 应用创建主窗口，并将其绑定到 `MainWindowViewModel`。
3. `MainWindowViewModel` 负责转发用户操作，例如新建、打开、保存、导出、编辑游戏数据和弹出对话框。
4. 当工作区打开后，ViewModel 会把实际工作委托给 `Workspace`、`LevelModule`、`EditorModule`、`ScriptingModule` 以及其他 FantaniaLib 服务。

## 常见改动热点

- 新增或修改顶层菜单命令：先看 `FantaniaApp/ViewModels/MainWindowViewModel.cs` 以及对应的 XAML 视图。
- 修改启动行为或全局异常处理：先看 `FantaniaApp/Program.cs` 和崩溃处理辅助代码。
- 调整本地化 UI 文案：查看 `FantaniaApp/Localization/Resources.resx` 和相关本地化辅助类。
- 修改当前工作区的打开、关闭或恢复方式：查看 `MainWindowViewModel`、`RecentAccess` 和 `FantaniaWorkspace`。

## 未来查询关键词

`Avalonia`、`App`、`Program`、`MainWindow`、`ViewModel`、`View`、`toolbar`、`menu`、最近工作区、本地化、桌面壳层、命令绑定

## 关联文档

- `workspace-core.md`：实际的工作区状态、持久化和内部模块。
- `controls-and-ui-components.md`：应用壳层使用的可复用 UI 控件和对话框。
- `level-editing.md`：编辑画布和关卡操作体验。