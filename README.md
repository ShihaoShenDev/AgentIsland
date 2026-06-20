# AgentIsland

`AgentIsland` 是一个面向 `ClassIsland` 的插件，用于把本地课程表能力以 **MCP Server** 的形式暴露给外部智能体或工具。

安装并启动后，插件会在本机开启一个 MCP 服务，外部客户端可以通过它读取当前课表状态、查询上下课信息，或者直接对指定日期的课表执行换课操作。

## 功能

- 提供本机 MCP 服务
- 查询当前正在上的课
- 查询下一节课
- 查询当前时间状态
- 获取当天课表
- 查询所有学科
- 交换指定日期课表中的两节课
- 自动创建或复用 ClassIsland 的临时换课层

## MCP 地址

插件启动后会监听以下地址：

- `http://localhost:5943/mcp`
- `http://localhost:5943/sse`

## 已暴露工具

### `get_current_class`

获取当前正在上的课程信息。

返回内容包含：

- 课程名
- 任课教师
- 上课开始时间
- 下课结束时间
- 剩余秒数
- 是否正在上课

### `get_next_class`

获取下一节课的信息。

返回内容包含：

- 课程名
- 任课教师
- 上课开始时间
- 下课结束时间
- 距离开始还有多少秒
- 是否存在下一节课

### `get_time_status`

获取当前时间状态。

返回内容包含：

- 当前状态
- 剩余秒数
- 当前本地时间

### `get_today_schedule`

获取今天的课表。

返回内容包含：

- 课表名称
- 日期
- 课程列表

课程列表中的每一项包含：

- 序号
- 课程名
- 任课教师
- 开始时间
- 结束时间
- 是否为换课
- 是否启用

### `list_subjects`

获取当前配置中的所有学科。

返回内容包含：

- 学科 ID
- 学科名称
- 任课教师
- 缩写

### `swap_classes`

交换指定日期课表中的两节课。

参数说明：

- `classIndex1`：第一节课索引，从 `0` 开始
- `classIndex2`：第二节课索引，从 `0` 开始
- `date`：换课日期，格式为 `yyyy-MM-dd`
- 如果 `date` 为空字符串，则表示今天

说明：

- 会优先复用已有的临时换课层
- 如果没有对应日期的课表，操作会失败
- 交换后会自动保存到配置中

## 使用场景

- 让大模型读取 ClassIsland 的实时课表状态
- 为校园助手、学习助手、自动排课工具提供本地数据接口
- 在不直接修改原始课表的前提下完成临时换课

## 运行要求

- `Windows`
- `ClassIsland`
- `.NET 8`

## 构建说明

> [!IMPORTANT]
> 请参考 https://docs.classisland.tech/dev/get-started/development-plugins.html 提前搭建 ClassIsland 的开发环境，否则无论是`dotnet run`还是使用编译脚本（见下文）都会报错。

### 使用构建脚本构建并运行（推荐）

```bash
# 构建调试版本（推荐用于开发）
.\build-debug.ps1

# 或者构建发行版本，可使用 ClassIsland 的打包功能直接发布
.\build-release.ps1
```

### 使用 `dotnet run` 构建并运行

> [!NOTE]
> 我也不知道`Properties/launchSettings.json`能不能跑通，反正我的电脑上不行（bushi）

```bash
dotnet run
```

### 打包

```bash
.\create-cipx.ps1
```

或参考：https://docs.classisland.tech/dev/plugins/publishing.html

## 项目结构

- `Plugin.cs`：插件入口，负责启动和停止 MCP 服务
- `Mcp/`：MCP 服务与工具实现
- `Helpers/`：UI 线程辅助工具
- `Models/`：MCP 返回结果模型
- `manifest.yml`：插件清单

## 正在开发的功能
- [ ] ACP 支持
- [ ] 可扩展性
- [ ] 小希语音原生支持
- ......

## 许可证

本项目在 GPL-3.0 license 下发布。

_本 ReadMe 由 Codex 生成。_
