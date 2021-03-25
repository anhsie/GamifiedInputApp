using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamifiedInputApp
{
    public class NativeWindowHelper
    {
        IntPtr m_hwnd;

        public NativeWindowHelper()
        {
            string className = "Minigame Window Class";

            unsafe
            {
                PInvoke.User32.WNDCLASSEX windowClass = PInvoke.User32.WNDCLASSEX.Create();
                windowClass.style = PInvoke.User32.ClassStyles.CS_HREDRAW | PInvoke.User32.ClassStyles.CS_VREDRAW;
                windowClass.lpfnWndProc = WindowProcedure;
                fixed (char* c = className)
                {
                    windowClass.lpszClassName = c;
                }
                PInvoke.User32.RegisterClassEx(ref windowClass);
            }

            m_hwnd = PInvoke.User32.CreateWindowEx(
                    PInvoke.User32.WindowStylesEx.WS_EX_OVERLAPPEDWINDOW,
                    className,
                    "Minigame Window",
                    PInvoke.User32.WindowStyles.WS_OVERLAPPEDWINDOW,
                    0,
                    0,
                    1280,
                    720,
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr());
        }

        public void Show()
        {
            PInvoke.User32.ShowWindow(m_hwnd, PInvoke.User32.WindowShowStyle.SW_SHOWDEFAULT);
        }

        public void Destroy()
        {
            PInvoke.User32.DestroyWindow(m_hwnd);
        }

        public Microsoft.UI.WindowId WindowId
        { 
            get { return GetWindowIdFromHwnd(m_hwnd); } 
        }

        public static IntPtr GetHwndFromWindowId(Microsoft.UI.WindowId windowId)
        {
            return new IntPtr((long)windowId.Value);
        }

        public static Microsoft.UI.WindowId GetWindowIdFromHwnd(IntPtr hwnd)
        {
            Microsoft.UI.WindowId windowId;
            windowId.Value = (ulong)hwnd.ToInt64();
            return windowId;
        }

        static unsafe IntPtr WindowProcedure(IntPtr hWnd, PInvoke.User32.WindowMessage msg, void* wParam, void* lParam)
        {
            Debugger.Log(1, "Windows Message", msg.ToString());
            return PInvoke.User32.DefWindowProc(hWnd, msg, (IntPtr)wParam, (IntPtr)lParam);
        }
    }
}
