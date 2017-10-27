using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadCPUID
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Mutex mutex = new Mutex(true, "{fe98c43e-0a42-496e-95e0-3f8bcc725756}");

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmMain());
                mutex.ReleaseMutex();
            }
            else
            {
                Process bProcess = Process.GetProcessesByName("Read Hardware Monitor").FirstOrDefault();

                if (bProcess != null)
                {
                    IntPtr hwnd = bProcess.MainWindowHandle;
                    if (hwnd != null)
                    {
                        ShowWindow(bProcess.MainWindowHandle, ShowWindowEnum.ShowNormalNoActivate);
                    }

                    SetForegroundWindow(bProcess.MainWindowHandle);
                }
            }
                
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };
    }
}
