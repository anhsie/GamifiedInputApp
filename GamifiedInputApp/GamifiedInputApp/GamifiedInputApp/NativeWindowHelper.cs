using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamifiedInputApp
{
    public class NativeWindowHelper
    {
        PInvoke.User32.WNDCLASSEX windowClass;
        IntPtr m_hwnd;

        public NativeWindowHelper()
        {
            windowClass = PInvoke.User32.WNDCLASSEX.Create();
            windowClass.style = PInvoke.User32.ClassStyles.CS_HREDRAW | PInvoke.User32.ClassStyles.CS_VREDRAW;
            
            string className = "Minigame Window Class";
            unsafe
            {
                windowClass.lpfnWndProc += WindowProcedure;
                fixed (char* c = className)
                {
                    windowClass.lpszClassName = c;
                }
            }

            PInvoke.User32.RegisterClassEx(ref windowClass);

            m_hwnd = PInvoke.User32.CreateWindowEx(
                    PInvoke.User32.WindowStylesEx.WS_EX_OVERLAPPEDWINDOW,
                    className,
                    "Minigame Window",
                    PInvoke.User32.WindowStyles.WS_OVERLAPPEDWINDOW,
                    0,
                    0,
                    400,
                    400,
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr());
        }

        public void Show()
        {
            PInvoke.User32.ShowWindow(m_hwnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
        }

        public void Destroy()
        {
            PInvoke.User32.DestroyWindow(m_hwnd);
        }

        public Microsoft.UI.WindowId WindowId
        { 
            get 
            {
                Microsoft.UI.WindowId windowId;
                windowId.Value = (ulong)m_hwnd.ToInt64();
                return windowId; 
            } 
        }

        static unsafe IntPtr WindowProcedure(IntPtr hWnd, PInvoke.User32.WindowMessage msg, void* wParam, void* lParam)
        {
            return PInvoke.User32.DefWindowProc(hWnd, msg, new IntPtr(wParam), new IntPtr(lParam));
        }
    }
}
