# QuellGTA_JoystickAFK
QuellGTA 真后台挂机插件，用ViGEmBus来模拟手柄实现后台挂机。
QuellGTA-PS4手柄摇杆模拟程序
使用方法:(需要安装ViGEmBus驱动)
  PS4StickSim.exe [-d 方向] [-t 毫秒数]

参数:
  -d, --direction  摇杆方向 (默认: left)
      可选值: left, right, up, down, upleft, upright, downleft, downright
  -t, --time       持续时间(毫秒) (默认: 1000)
  -h, --help       显示帮助信息

示例:
  PS4StickSim.exe                  # 下拉1秒
  PS4StickSim.exe -d left -t 500   # 左拉0.5秒
  PS4StickSim.exe -d up -t 2000    # 上拉2秒
