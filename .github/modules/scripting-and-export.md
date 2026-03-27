# 此文件用途：记录 Lua 集成、工作区脚本加载、导出配置以及生成输出流程。

## 范围

本模块覆盖将 FantaniaLib 与工作区 Lua 脚本连接起来的脚本扩展系统，以及把工作区数据转换为生成产物或运行时产物的导出管线。

## 主要职责

- 通过工作区脚本模块加载并暴露 Lua 脚本。
- 把 C# 工作区和渲染概念绑定进脚本引擎。
- 提供脚本定义的导出设置和 pipeline hook。
- 将工作区实体、placement 和游戏数据转换为生成输出。

## 关键入口

- `FantaniaLib/Workspace/Scripting/ScriptingModule.cs`：工作区侧脚本加载与访问入口。
- `FantaniaLib/Scripting/ScriptEngine.cs`：脚本引擎包装和集成接口。
- `FantaniaLib/Scripting/CustomConversion/`：C# 对象与 Lua 可见对象之间的类型转换。
- `FantaniaLib/Workspace/Export/ExportContext.cs`：导出执行流程。
- `PixelGameWorkspace/scripts/export_settings.lua`：项目导出设置、生成代码输出和路径规则。
- `PixelGameWorkspace/scripts/pipeline_hook.lua`：面向渲染钩子的脚本配置。
- `PixelGameWorkspace/scripts/editor_setup.lua`：工作区侧编辑器脚本初始化。

## 高层流程

1. 工作区初始化脚本模块，并加载内建脚本和工作区脚本。
2. 脚本引擎通过 wrapper/proxy 对象暴露工作区 API。
3. 从 Lua 定义中读取导出设置。
4. 导出过程遍历工作区关卡与实体，重映射 placement 数据，并写出生成文件。
5. 管线脚本还会通过命名 uniform 和 hook 数据影响渲染行为。

## 当前工作区的导出要点

- `export_settings.lua` 会收集全局 uniform 变体，并允许排除指定关卡不参与导出。
- 它会遍历 `Workspace.AllLevelNames` 中的所有关卡，抽取导出实体，将字符串归一化为索引，并重映射 placement 标识。
- `GroupReference` 在导出阶段会被写成稳定的 group 数值 ID 加对象 ID；group 名本身不再进入 `AllStrings`，对应的 group/type 常量由 `export_settings.lua` 生成到 C++ 常量文件中。
- 它会把生成的 C++ 文件写入游戏侧源码目录。

## 常见改动热点

- 修复脚本加载或全局对象缺失：查看 `ScriptingModule`、`ScriptEngine` 和 wrapper 暴露逻辑。
- 修复导出数据结构或生成常量：查看 `ExportContext` 和 `export_settings.lua`。
- 修复脚本到 C# 的类型转换问题：查看 `FantaniaLib/Scripting/CustomConversion/`。
- 修复由 pipeline hook 驱动的渲染行为：查看 `pipeline_hook.lua`、`pipeline_setup.lua` 和渲染管线中的 uniform 映射。

## 未来查询关键词

`Lua`、`MoonSharp`、`script engine`、`export`、`ExportContext`、`export_settings`、`pipeline_hook`、生成代码、`gamedata`、`custom conversion`

## 关联文档

- `workspace-core.md`：承载脚本与导出服务的工作区对象。
- `rendering-and-lighting.md`：渲染侧如何消费 pipeline hook 数据。
- `workspace-content.md`：实际由工作区编写的脚本和资源。