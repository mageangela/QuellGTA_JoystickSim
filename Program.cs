using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

class Program
{
    static void Main(string[] args)
    {
        // 默认参数
        string direction = "left";  // 默认向左拉
        int duration = 1000;        // 默认1秒

        // 解析命令行参数
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLower();

            if (arg == "-d" || arg == "--direction")
            {
                if (i + 1 < args.Length)
                {
                    direction = args[i + 1].ToLower();
                    i++; // 跳过下一个参数（值）
                }
                else
                {
                    Console.WriteLine("错误: -d 参数需要指定方向");
                    PrintUsage();
                    return;
                }
            }
            else if (arg == "-t" || arg == "--time")
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int time))
                {
                    if (time > 0 && time <= 60000) // 限制在1分钟以内
                    {
                        duration = time;
                    }
                    else
                    {
                        Console.WriteLine("警告: 时间参数应在1-60000毫秒之间，使用默认值1000ms");
                    }
                    i++; // 跳过下一个参数（值）
                }
                else
                {
                    Console.WriteLine("错误: -t 参数需要有效的毫秒数");
                    PrintUsage();
                    return;
                }
            }
            else if (arg == "-h" || arg == "--help")
            {
                PrintUsage();
                return;
            }
            else
            {
                Console.WriteLine($"未知参数: {arg}");
                PrintUsage();
                return;
            }
        }

        Console.WriteLine($"方向: {direction}, 持续时间: {duration}ms");

        // 执行手柄操作
        try
        {
            ExecuteControllerAction(direction, duration);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"操作失败: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("QuellGTA-PS4手柄摇杆模拟程序");
        Console.WriteLine("使用方法:(需要安装ViGEmBus驱动)");
        Console.WriteLine("  PS4StickSim.exe [-d 方向] [-t 毫秒数]");
        Console.WriteLine();
        Console.WriteLine("参数:");
        Console.WriteLine("  -d, --direction  摇杆方向 (默认: left)");
        Console.WriteLine("      可选值: left, right, up, down, upleft, upright, downleft, downright");
        Console.WriteLine("  -t, --time       持续时间(毫秒) (默认: 1000)");
        Console.WriteLine("  -h, --help       显示帮助信息");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  PS4StickSim.exe                  # 下拉1秒");
        Console.WriteLine("  PS4StickSim.exe -d left -t 500   # 左拉0.5秒");
        Console.WriteLine("  PS4StickSim.exe -d up -t 2000    # 上拉2秒");
    }

    static void ExecuteControllerAction(string direction, int duration)
    {
        using var client = new ViGEmClient();
        using var ds4 = client.CreateDualShock4Controller();

        ds4.Connect();
        Console.WriteLine("虚拟PS4手柄已连接");
        Thread.Sleep(1000);

        // 根据方向设置摇杆位置
        SetStickPosition(ds4, direction);
        Console.WriteLine($"摇杆{direction}保持 {duration}ms...");

        // 保持指定时间
        Thread.Sleep(duration);

        // 归中
        SetStickPosition(ds4, "center");





        Console.WriteLine("操作完成，摇杆已归中");

        // 确保信号发送完毕
        Thread.Sleep(100);
    }

    static void SetStickPosition(IDualShock4Controller controller, string direction)
    {
        // 摇杆坐标范围: 0-255，128为居中
        byte x = 128;
        byte y = 128;

        switch (direction.ToLower())
        {
            case "left":
                x = 0; y = 128;      // 最左（默认）
                break;
            case "right":
                x = 255; y = 128;    // 最右
                break;
            case "up":
                x = 128; y = 0;      // 最上
                break;
            case "down":
                x = 128; y = 255;    // 最下
                break;
            case "upleft":
                x = 0; y = 0;        // 左上
                break;
            case "upright":
                x = 255; y = 0;      // 右上
                break;
            case "downleft":
                x = 0; y = 255;      // 左下
                break;
            case "downright":
                x = 255; y = 255;    // 右下
                break;
            case "center":
            default:
                x = 128; y = 128;    // 居中
                break;
        }

        controller.SetAxisValue(DualShock4Axis.LeftThumbX, x);
        controller.SetAxisValue(DualShock4Axis.LeftThumbY, y);
    }
}