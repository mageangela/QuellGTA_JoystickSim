# QuellGTA_JoystickAFK
QuellGTA 真后台挂机插件，用ViGEmBus来模拟手柄实现后台挂机。<p></p>
QuellGTA-手柄摇杆模拟程序<p></p>
使用方法:(需要安装ViGEmBus驱动)<p></p>
  StickSim.exe [-c 手柄类型] [-d 方向] [-t 毫秒数]<p></p>
参数:<p></p>
  -c, --controller  手柄类型 (默认: xbox)<p></p>
      可选值: ps4, xbox<p></p>
  -d, --direction   摇杆方向 (默认: left)<p></p>
      可选值: left, right, up, down, upleft, upright, downleft, downright<p></p>
  -t, --time        持续时间(毫秒) (默认: 1000)<p></p>
  -h, --help        显示帮助信息<p></p>
示例:<p></p>
  StickSim.exe                          # Xbox手柄下拉1秒<p></p>
  StickSim.exe -c ps4 -d left -t 500    # PS4手柄左拉0.5秒<p></p>
  StickSim.exe -c xbox -d up -t 2000    # Xbox手柄上拉2秒<p></p>
注意:<p></p>
  PS4手柄: 兼容性较好，无设备插入提示音<p></p>
  Xbox手柄: 每次运行会有设备插入提示音，但游戏兼容性更好<p></p>
