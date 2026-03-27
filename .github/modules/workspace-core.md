# 此文件用途：记录核心工作区抽象、内部模块、持久化流程以及共享编辑器状态。

## 范围

本模块描述 `FantaniaLib` 中的系统中心：`Workspace` 抽象以及它聚合的各个模块。

当请求涉及保存/打开行为、工作区有效性、数据库同步、模块边界或撤销重做时，应优先阅读本文件。

## 主要职责

- 定义根工作区生命周期：`validate`、`create`、`open`、`save`、`tick`。
- 聚合关卡管理、数据库、放置模板、编辑器状态、日志和脚本等核心模块。
- 统一维护工作区相对路径约定，例如 `textures`、`scripts`、`gamedata`、`levels` 和生成文件目录。
- 跟踪脏状态和撤销栈。

## 关键入口

- `FantaniaLib/Workspace/Workspace.cs`：根抽象和生命周期中心。
- `FantaniaLib/Workspace/WorkspaceSolution.cs`：工作区级 solution 持久化数据。
- `FantaniaLib/Workspace/Database/DatabaseModule.cs`：结构化数据持久化与同步。
- `FantaniaLib/Workspace/Level/LevelModule.cs`：关卡加载、保存、切换和实体级操作。
- `FantaniaLib/Workspace/Level/PlacementModule.cs`：放置模板和相关工作区数据。
- `FantaniaLib/Workspace/Level/EditorModule.cs`：选择集、鼠标世界坐标和编辑模式等编辑器状态。
- `FantaniaLib/Workspace/Log/LogModule.cs`：供编辑器和脚本使用的日志接口。
- `FantaniaLib/Workspace/Scripting/ScriptingModule.cs`：脚本加载、工作区脚本引擎接入和导出设置访问。

## 内部模块结构

工作区对象拥有并暴露一组固定模块。
根据 `Workspace.cs`，当前主要内建模块有：

- `LevelModule`
- `DatabaseModule`
- `PlacementModule`
- `EditorModule`
- `LogModule`
- `ScriptingModule`

这些模块共同构成了 Avalonia UI 层和 Lua/导出管线所依赖的核心运行面。

## 生命周期流程

1. 构造阶段先校验目录，再实例化内建模块。
2. `Validate()` 会检查 `workspace.json`、可选的用户临时数据、数据库可访问性，并初始化脚本全局对象。
3. `CreateNew()` 会创建必要目录并初始化 solution 状态。
4. `Open()` 会加载脚本、同步数据库数据、同步放置模板，并在可用时恢复最近编辑的关卡。
5. `Save()` 会同步数据库、当前关卡、solution 元数据和用户临时状态。
6. `Tick()` 会更新帧数、时间和脏标记，并把这些值暴露给编辑器层。

## 重要数据边界

- `workspace.json` 是工作区根级 solution 文件。
- `.fantania/` 存放生成数据或用户临时编辑器数据。
- `scripts/`、`textures/`、`gamedata/` 和 `levels/` 是规范工作区内容目录。
- `WorkspaceProxy` 负责把工作区桥接进脚本环境。

## 常见改动热点

- 修复保存/打开回归：先看 `FantaniaLib/Workspace/Workspace.cs`、`DatabaseModule` 和 `LevelModule`。
- 修复脏状态或撤销问题：查看 `Workspace.cs`、撤销栈代码，以及真正修改数据的模块。
- 增加新的工作区级生成状态：查看 `.fantania` 约定以及 `WriteUserTemp()` / `WriteSolution()`。
- 新增内建工作区服务：查看 `Workspace` 构造函数和模块暴露模式。

## 未来查询关键词

`workspace`、`solution`、`workspace.json`、`database`、`placement module`、`editor module`、`log module`、`scripting module`、`undo`、脏状态、`save`、`open`、`validate`

## 关联文档

- `level-editing.md`：建立在 editor 和 level 模块之上的用户侧编辑行为。
- `scripting-and-export.md`：构建在工作区核心之上的脚本与导出体系。
- `workspace-content.md`：工作区实际加载的内容目录结构。