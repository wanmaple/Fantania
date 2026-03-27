# 此文件用途：记录可复用控件、编辑器组件、覆盖层以及应用内共享 UI 组件。

## 范围

本模块主要描述由 `FantaniaLib/Controls` 提供的可复用控件层，以及应用中配套使用的视图控件。

当请求讨论的是共享控件、字段编辑器、对话框、消息框或编辑覆盖层，而不是某个页面的业务逻辑时，应优先查看本文件。

## 主要职责

- 提供可复用的输入与编辑组件。
- 提供共享对话框和消息框辅助接口。
- 为类型化数据定义提供字段编辑控件。
- 提供关卡编辑所需的覆盖层和控件侧输入辅助。

## 关键入口

- `FantaniaLib/Controls/FileSelectBox.axaml.cs`：可复用的文件/文件夹选择控件。
- `FantaniaLib/Controls/OptionBox.axaml.cs`：基于选项的选择控件。
- `FantaniaLib/Controls/ToggleButtonGroup.axaml.cs`：成组切换按钮 UI。
- `FantaniaLib/Controls/ControlInputTracker.cs`：控件交互中的输入跟踪辅助。
- `FantaniaLib/Controls/Editing/EditFieldsView.axaml.cs`：动态字段编辑表面。
- `FantaniaLib/Controls/Editing/ObjectEditView.axaml.cs`：对象编辑容器。
- `FantaniaLib/Controls/Editing/FieldEditing/*.axaml.cs`：整数、浮点、颜色、向量、枚举、纹理、目录等类型字段编辑器。
- `FantaniaLib/Controls/MessageBox/MessageBoxHelper.cs`：可复用消息框 API。
- `FantaniaLib/Controls/Level/*.cs`：关卡编辑器使用的覆盖层和 gizmo。

## 设计意图

- `FantaniaApp` 中的应用级视图应尽量复用这些控件原语，而不是重复实现底层编辑器。
- 脚本定义或 schema 定义的数据，优先通过共享字段编辑器进行编辑。
- 跨模块对话框应统一经过消息框辅助接口，以保持交互体验一致。

## 常见改动热点

- 新增一种类型字段编辑器：查看 `FantaniaLib/Controls/Editing/FieldEditing/`。
- 调整公共对话框行为：查看 `MessageBoxHelper` 和相关视图/设置。
- 修复可复用的选项框或文件选择控件：查看 `OptionBox`、`FileSelectBox` 或 toggle group。
- 修改 gizmo 外观或控件侧覆盖层：查看 `FantaniaLib/Controls/Level/`。

## 未来查询关键词

`control`、`field editor`、`popup`、`message box`、`dialog`、`option box`、`file select`、`toggle group`、`overlay`、`input tracker`

## 关联文档

- `app-shell-and-mvvm.md`：承载这些控件的应用级页面。
- `level-editing.md`：建立在覆盖层和 gizmo 之上的编辑交互。