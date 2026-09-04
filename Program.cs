using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.IO.Pipes;

class Program
{
    private const string PipeName = "QuellGTA_JoystickSim_Pipe";
    private static ViGEmClient? _client;
    private static IDualShock4Controller? _ds4;
    private static IXbox360Controller? _xbox;
    private static string _controllerType = "xbox";
    private static bool _isRunning = false;
    private static bool _isPushing = false;
    private static readonly object _lock = new object();

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "-start":
                // 检查是否有控制器类型参数
                string type = "xbox"; // 默认
                if (args.Length >= 2)
                {
                    string argType = args[1].ToLower();
                    if (argType == "ps4" || argType == "xbox")
                    {
                        type = argType;
                    }
                    else
                    {
                        Console.WriteLine($"无效的控制器类型: {args[1]}，使用默认 xbox");
                        Console.WriteLine("支持的类型: xbox, ps4");
                    }
                }
                StartService(type);
                break;

            case "-push":
                if (args.Length < 3)
                {
                    Console.WriteLine("用法: JoystickSim.exe -push <方向> <强度>");
                    Console.WriteLine("  方向: left, right, up, down, upleft, upright, downleft, downright");
                    Console.WriteLine("  强度: 0-255");
                    return;
                }
                SendCommand($"push|{args[1].ToLower()}|{args[2]}");
                break;

            case "-center":
                SendCommand("center");
                break;

            case "-stop":
                SendCommand("stop");
                break;

            case "-status":
                SendCommand("status");
                break;

            default:
                Console.WriteLine($"未知命令: {command}");
                PrintUsage();
                break;
        }
    }

    // ============ 服务端（主进程） ============
    static void StartService(string controllerType = "xbox")
    {
        _controllerType = controllerType;

        // 检查是否已有实例在运行
        try
        {
            using var testClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            testClient.Connect(100);
            Console.WriteLine("服务已在运行中");
            return;
        }
        catch
        {
            // 没有现有实例，继续启动
        }

        Console.WriteLine("QuellGTA 手柄摇杆服务启动...");
        Console.WriteLine($"控制器类型: {_controllerType.ToUpper()}");
        Console.WriteLine("等待命令...");
        Console.WriteLine("可用命令: -push <方向> <强度>, -center, -stop, -status");

        try
        {
            _client = new ViGEmClient();
            InitializeController();

            _isRunning = true;

            // 主循环 - 每次连接都创建新的管道服务器
            while (_isRunning)
            {
                try
                {
                    // 每次循环创建新的管道服务器实例
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                        PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    Console.WriteLine("等待连接...");
                    server.WaitForConnection();
                    Console.WriteLine("客户端已连接");

                    // 处理命令
                    using (var reader = new StreamReader(server))
                    using (var writer = new StreamWriter(server) { AutoFlush = true })
                    {
                        string? message = reader.ReadLine();
                        if (!string.IsNullOrEmpty(message))
                        {
                            Console.WriteLine($"收到命令: {message}");
                            string response = ProcessCommand(message);
                            writer.WriteLine(response);
                            Console.WriteLine($"响应: {response}");
                        }
                    }

                    // 客户端断开后，管道会被释放，然后循环继续等待新连接
                    Console.WriteLine("客户端已断开，等待新连接...");
                }
                catch (IOException ex)
                {
                    // 管道相关的IO异常，通常是因为客户端断开
                    if (_isRunning)
                    {
                        Console.WriteLine($"管道IO错误: {ex.Message}");
                        // 继续循环，等待新连接
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"错误: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"服务致命错误: {ex.Message}");
        }
        finally
        {
            Cleanup();
            Console.WriteLine("服务已停止");
        }
    }

    static void InitializeController()
    {
        if (_controllerType == "ps4")
        {
            _ds4 = _client!.CreateDualShock4Controller();
            _ds4.Connect();
            Console.WriteLine("PS4手柄已创建");
        }
        else
        {
            _xbox = _client!.CreateXbox360Controller();
            _xbox.Connect();
            Console.WriteLine("Xbox手柄已创建");
        }
        Thread.Sleep(500);
    }

    static string ProcessCommand(string command)
    {
        lock (_lock)
        {
            var parts = command.Split('|');
            string cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "push":
                    if (parts.Length < 3)
                        return "错误: 参数不足";

                    string direction = parts[1];
                    if (!byte.TryParse(parts[2], out byte strength))
                        return "错误: 强度必须是数字 0-255";

                    strength = Math.Min((byte)255, strength);
                    SetStickPosition(direction, strength);
                    _isPushing = true;
                    return $"推杆: {direction}, 强度: {strength}/255";

                case "center":
                    SetStickPosition("center", 0);
                    _isPushing = false;
                    return "摇杆已归中";

                case "stop":
                    _isRunning = false;
                    return "服务停止中...";

                case "status":
                    return _isPushing ? "推杆中" : "空闲";

                default:
                    return $"未知命令: {cmd}";
            }
        }
    }

    static void SetStickPosition(string direction, byte ps4Strength)
    {
        if (_controllerType == "ps4" && _ds4 != null)
        {
            byte x = 128, y = 128;
            byte offset = (byte)(ps4Strength / 2);

            switch (direction)
            {
                case "left": x = (byte)(128 - offset); y = 128; break;
                case "right": x = (byte)(128 + offset); y = 128; break;
                case "up": x = 128; y = (byte)(128 - offset); break;
                case "down": x = 128; y = (byte)(128 + offset); break;
                case "upleft": x = (byte)(128 - offset); y = (byte)(128 - offset); break;
                case "upright": x = (byte)(128 + offset); y = (byte)(128 - offset); break;
                case "downleft": x = (byte)(128 - offset); y = (byte)(128 + offset); break;
                case "downright": x = (byte)(128 + offset); y = (byte)(128 + offset); break;
                case "center":
                default: x = 128; y = 128; break;
            }

            _ds4.SetAxisValue(DualShock4Axis.LeftThumbX, x);
            _ds4.SetAxisValue(DualShock4Axis.LeftThumbY, y);
        }
        else if (_xbox != null)
        {
            short xboxStrength = (short)(ps4Strength / 255.0 * short.MaxValue);
            short x = 0, y = 0;

            switch (direction)
            {
                case "left": x = (short)-xboxStrength; y = 0; break;
                case "right": x = xboxStrength; y = 0; break;
                case "up": x = 0; y = xboxStrength; break;      // 原来是 -xboxStrength，现在取反
                case "down": x = 0; y = (short)-xboxStrength; break;  // 原来是 xboxStrength，现在取反
                case "upleft": x = (short)-xboxStrength; y = xboxStrength; break;      // 原来 y 是负
                case "upright": x = xboxStrength; y = xboxStrength; break;             // 原来 y 是负
                case "downleft": x = (short)-xboxStrength; y = (short)-xboxStrength; break; // 原来 y 是正
                case "downright": x = xboxStrength; y = (short)-xboxStrength; break;       // 原来 y 是正
                case "center":
                default: x = 0; y = 0; break;
            }

            _xbox.SetAxisValue(Xbox360Axis.LeftThumbX, x);
            _xbox.SetAxisValue(Xbox360Axis.LeftThumbY, y);
            _xbox.SubmitReport();
        }
    }

    static void Cleanup()
    {
        // 归中
        SetStickPosition("center", 0);
        Thread.Sleep(200);

        // 清理引用
        _ds4 = null;
        _xbox = null;

        // 释放 ViGEmClient（会自动清理所有控制器）
        _client?.Dispose();
        _client = null;
    }

    // ============ 客户端（发送命令） ============
    static void SendCommand(string command)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            client.Connect(2000); // 等待2秒

            using var writer = new StreamWriter(client) { AutoFlush = true };
            using var reader = new StreamReader(client);

            writer.WriteLine(command);
            string? response = reader.ReadLine();

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine(response);
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("错误: 服务未运行或超时，请先执行 JoystickSim.exe -start");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"通信错误: {ex.Message}");
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("QuellGTA 手柄摇杆控制服务");
        Console.WriteLine("================================");
        Console.WriteLine("启动服务 (首次运行):");
        Console.WriteLine("  JoystickSim.exe -start            # 默认 Xbox 手柄");
        Console.WriteLine("  JoystickSim.exe -start xbox       # Xbox 手柄");
        Console.WriteLine("  JoystickSim.exe -start ps4        # PS4 手柄");
        Console.WriteLine();
        Console.WriteLine("控制命令 (服务运行后):");
        Console.WriteLine("  JoystickSim.exe -push <方向> <强度>   # 推杆");
        Console.WriteLine("  JoystickSim.exe -center               # 归中");
        Console.WriteLine("  JoystickSim.exe -status               # 查看状态");
        Console.WriteLine("  JoystickSim.exe -stop                 # 停止服务");
        Console.WriteLine();
        Console.WriteLine("方向: left, right, up, down, upleft, upright, downleft, downright");
        Console.WriteLine("强度: 0-255");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  JoystickSim.exe -start ps4           # 启动PS4手柄服务");
        Console.WriteLine("  JoystickSim.exe -start xbox          # 启动Xbox手柄服务");
        Console.WriteLine("  JoystickSim.exe -push up 200         # 向上推200强度");
        Console.WriteLine("  JoystickSim.exe -push left 128       # 向左半推");
        Console.WriteLine("  JoystickSim.exe -center              # 归中");
        Console.WriteLine("  JoystickSim.exe -stop                # 停止服务");
    }
}