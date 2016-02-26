using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Com.Fairjm.Tony
{
    /// <summary>
    /// by fairjm.
    /// 2016/02/19
    /// </summary>
    class LockScreen
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        delegate bool ConsoleEventDelegate(CtrlType CtrlType);

        private static readonly string CONFIG_FILE = "config";

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        static void HideAllWindows()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
            Thread.Sleep(5000);
        }

        private static bool ConsoleCtrlCheck(CtrlType ctrlType)
        {
            bool isclosing = false;
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlType.CTRL_C_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+C Received!");
                    break;

                case CtrlType.CTRL_BREAK_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+BREAK Received!");
                    break;

                case CtrlType.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    Console.WriteLine("Program Being Closed!");
                    break;

                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    Console.WriteLine("User Is Logging Off!");
                    break;

            }
            if (isclosing)
            {
                LockWorkStation();
            }
            return true;
        }

        static List<string> readAllowFile()
        {
            var configExists = File.Exists(CONFIG_FILE);
            if (!configExists)
            {
                // 默认用酷狗
                return new List<string>() {
                    "kugou.exe"
                };
            }
            else
            {
                var files = File.ReadAllLines(CONFIG_FILE);
                if (files.Length == 0)
                {
                    return new List<string>() {
                    "kugou.exe"
                };
                }
                else
                {
                    return new List<string>(files.Select(e => e.ToLower()));
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("starting....");
            var allowFiles = readAllowFile();
            Console.WriteLine("allow files:");
            allowFiles.ForEach(Console.WriteLine);
            try
            {
                //Console.CancelKeyPress += delegate
                //{
                //    LockWorkStation();
                //};

                SetConsoleCtrlHandler(new ConsoleEventDelegate(ConsoleCtrlCheck), true);

                HideAllWindows();
                while (true)
                {
                    Thread.Sleep(250);
                    IntPtr handler = GetForegroundWindow();
                    if (handler == null)
                    {
                        Console.WriteLine("No Process");
                        continue;
                    }
                    uint pid;
                    GetWindowThreadProcessId(handler, out pid);
                    var process = Process.GetProcessById((int)pid);
                    String windowTitle = process?.MainWindowTitle;
                    String fileName = process?.MainModule?.FileName?.ToLower();
                    Console.WriteLine($"Current Execution File:{fileName}");
                    if (fileName != null)
                    {
                        bool isAllowFile = allowFiles.Exists(e => fileName.Contains(e));
                        if (!isAllowFile) { break; }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            finally
            {
                LockWorkStation();
            }
        }
    }
}
