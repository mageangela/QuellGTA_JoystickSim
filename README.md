# QuellGTA_JoystickSim

一个基于 ViGEm 的虚拟手柄摇杆控制工具，支持 Xbox 360 和 PS4 虚拟手柄，通过命令行控制摇杆方向与力度。

## 🎮 功能特性

- 支持 **Xbox 360** 和 **PS4** 虚拟手柄
- 通过命令行控制左摇杆的 **8 个方向**（上、下、左、右及四个对角线）
- 摇杆力度 **0-255** 可调
- 采用命名管道通信，支持单实例服务模式
- 使用 **ViGEm** 模拟硬件级手柄输入

## 📦 依赖

- [ViGEm Client](https://github.com/ViGEm/ViGEmClient) - 虚拟游戏手柄驱动
- .NET 6.0 或更高版本
- Windows 10/11

## 🚀 快速开始

### 1. 安装 ViGEm 驱动

首先需要安装 ViGEm 总线驱动：

```powershell
# 使用 winget 安装
winget install ViGEm.ViGEmBus

# 或从官方 GitHub 下载安装包
# https://github.com/ViGEm/ViGEmBus/releases
```

### 2. 启动服务

```bash
# 默认使用 Xbox 手柄
StickSim.exe -start

# 或指定 PS4 手柄
StickSim.exe -start ps4

# 指定 Xbox 手柄
StickSim.exe -start xbox
```

### 3. 控制摇杆

```bash
# 向上推杆，强度 200
StickSim.exe -push up 200

# 向左半推
StickSim.exe -push left 128

# 推右上角，最大力度
StickSim.exe -push upright 255

# 摇杆归中（释放）
StickSim.exe -center

# 查看当前状态
StickSim.exe -status

# 停止服务
StickSim.exe -stop
```

## 📖 命令参考

### `-start [type]`

启动虚拟手柄服务。

| 参数 | 说明 |
|------|------|
| `xbox` | Xbox 360 手柄（默认） |
| `ps4` | PS4 手柄 |

### `-push <direction> <strength>`

推摇杆到指定方向和力度。

**方向 (direction):**
- `left` / `right` / `up` / `down`
- `upleft` / `upright` / `downleft` / `downright`

**力度 (strength):** `0` ~ `255`

### `-center`

将摇杆归中（释放摇杆）。

### `-status`

查询当前摇杆状态（推杆中 / 空闲）。

### `-stop`

停止服务并清理虚拟手柄。

## 💡 使用场景

- **游戏自动化** - 配合脚本实现游戏内自动移动
- **按键映射** - 将键盘按键映射为摇杆操作
- **辅助功能** - 为残障人士提供替代输入方式
- **测试工具** - 测试游戏对摇杆输入的响应

## ⚠️ 注意事项

1. **首次运行需要管理员权限**（ViGEm 驱动需要）
2. 服务启动后会在后台持续运行，直到收到 `-stop` 命令
3. 同一时间只能有一个服务实例运行
4. 如果游戏无法识别手柄，请检查 ViGEm 驱动是否正确安装

## 🔧 故障排查

### 服务无法启动
```bash
# 检查是否已有服务在运行
StickSim.exe -status

# 如果卡住了，强制结束进程
taskkill /f /im StickSim.exe
```

### 游戏检测不到手柄
1. 确认 ViGEm 驱动已安装
2. 尝试重启服务
3. 在 Windows 游戏控制器中检查虚拟手柄是否出现

---

**Happy Simulating!** 🎮
