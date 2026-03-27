# 此文件用途：记录渲染管线、材质与帧缓冲流转，以及光照相关子系统。

## 范围

本模块覆盖 `FantaniaLib` 中的渲染管线，包括 stage、framebuffer、material、uniform、相机相关渲染状态，以及光照和 light culling 流程。

它也覆盖工作区 Lua 脚本如何参与管线配置和全局 uniform 设置。

## 主要职责

- 构建并执行可配置的渲染管线。
- 管理 framebuffer、shader program、material、texture 和 command buffer。
- 将 renderable 路由到有序的 pipeline stage。
- 提供与光照相关的 buffer、light culling 以及由 pipeline hook 驱动的 uniform。

## 关键入口

- `FantaniaLib/Rendering/Pipeline/ConfigurableRenderPipeline.cs`：主渲染上下文和管线执行中心。
- `FantaniaLib/Rendering/Pipeline/BuiltinPipelineStages.cs`：内建 stage 定义与顺序。
- `FantaniaLib/Rendering/Camera2D.cs`：相机空间配置。
- `FantaniaLib/Rendering/RenderInfo.cs`：面向 renderable 的数据结构。
- `FantaniaLib/Rendering/MaterialSet.cs` 及相关渲染支持类型：shader/material 注册中心。
- `PixelGameWorkspace/scripts/pipeline_hook.lua`：脚本定义的 uniform 和管线钩子数据。
- `PixelGameWorkspace/scripts/pipeline_setup.lua`：工作区侧渲染管线配置。
- `PixelGameWorkspace/scripts/entities/point_light.lua`：被编辑器和运行时管线使用的具体点光源实体定义。

## 高层管线流程

1. 管线根据渲染配置和工作区提供的 shader/material 资源进行构建。
2. 注册 framebuffer 和 stage。
3. 将内建 shader 与工作区 shader 载入 material set。
4. 按 stage 对 renderable 分组，并按顺序处理。
5. 工作线程负责填充 uniform、执行 stage setup，并输出 command buffer。

## 光照说明

- 管线显式声明了 `Color`、`LightOccluderMask`、`JFA1`、`JFA2` 等 buffer。
- 光照行为与 stage 名称、以及由 pipeline hook 驱动的 uniform 设置紧密耦合。
- `point_light.lua` 声明了一个 `PlacementTypes.LightSource` 类型的光源放置模板，同时提供编辑器可见的指示器和用于 tiled light culling 的 renderable。

## 常见改动热点

- 新增或修改渲染 stage：查看 `ConfigurableRenderPipeline` 和内建 stage 定义。
- 修复 uniform 缺失或 framebuffer 绑定问题：查看 pipeline hook 的 uniform 映射和 framebuffer 初始化。
- 修复光源渲染或 light culling：查看光照 stage、光照相关 renderable 类型，以及 `point_light.lua`。
- 修改 shader/material 注册方式：查看 material set 的构建和工作区 shader 的加载逻辑。

## 未来查询关键词

`render`、`pipeline`、`shader`、`material`、`framebuffer`、`render stage`、`camera`、`light`、`shadow`、`light culling`、`pipeline hook`、`uniform`

## 关联文档

- `workspace-content.md`：为管线提供脚本和资源的具体工作区内容。
- `scripting-and-export.md`：脚本侧配置以及与导出相关的工作区数据使用方式。
- `level-editing.md`：最终会触发 renderable 更新的编辑器交互。