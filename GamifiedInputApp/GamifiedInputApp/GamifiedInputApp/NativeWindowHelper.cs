using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamifiedInputApp
{
    public class NativeWindowHelper
    {
        IntPtr m_hwnd;
        DipAwareRect m_rect;
        IntPtr? m_parent;
        GCHandle m_pinnedWindowsProcedureDelegate;

        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private static readonly double ScaleFactor = PInvoke.User32.GetDpiForSystem() / 96.0;

        public NativeWindowHelper(Windows.Foundation.Rect bounds, IntPtr? hWndParent)
        {
            string className = "Minigame Window Class";
            m_rect = new DipAwareRect(bounds);
            m_parent = hWndParent;

            unsafe
            {
                PInvoke.User32.WNDCLASSEX windowClass = PInvoke.User32.WNDCLASSEX.Create();
                windowClass.style = PInvoke.User32.ClassStyles.CS_HREDRAW | PInvoke.User32.ClassStyles.CS_VREDRAW;
                PInvoke.User32.WndProc windowProcedureDelegate = WindowProcedure;
                windowClass.lpfnWndProc = windowProcedureDelegate;
                m_pinnedWindowsProcedureDelegate = GCHandle.Alloc(windowProcedureDelegate);
                windowClass.hbrBackground = PInvoke.Gdi32.GetStockObject(PInvoke.Gdi32.StockObject.GRAY_BRUSH);
                fixed (char* c = className)
                {
                    windowClass.lpszClassName = c;
                }
                PInvoke.User32.RegisterClassEx(ref windowClass);
            }

            PInvoke.User32.WindowStyles dwStyle = hWndParent.HasValue ?
                PInvoke.User32.WindowStyles.WS_CHILD : PInvoke.User32.WindowStyles.WS_OVERLAPPEDWINDOW;

            m_hwnd = PInvoke.User32.CreateWindowEx(
                    0,
                    className,
                    "Minigame Window",
                    dwStyle,
                    m_rect.x,
                    m_rect.y,
                    m_rect.cx,
                    m_rect.cy,
                    hWndParent.GetValueOrDefault(),
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr());
        }

        public void Show()
        {
            PInvoke.User32.SetWindowPos(
                m_hwnd,
                HWND_TOP,
                m_rect.x,
                m_rect.y,
                m_rect.cx,
                m_rect.cy,
                PInvoke.User32.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        public void Hide()
        {
            PInvoke.User32.SetWindowPos(
                m_hwnd,
                HWND_TOP,
                m_rect.x,
                m_rect.y,
                m_rect.cx,
                m_rect.cy,
                PInvoke.User32.SetWindowPosFlags.SWP_HIDEWINDOW);
        }

        public void Destroy()
        {
            PInvoke.User32.DestroyWindow(m_hwnd);
            m_pinnedWindowsProcedureDelegate.Free();
        }

        public Microsoft.UI.WindowId WindowId
        { 
            get { return GetWindowIdFromHwnd(m_hwnd); } 
        }

        public int Width
        {
            get { return m_rect.cx; }
        }

        public int Height
        {
            get { return m_rect.cy; }
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
            return PInvoke.User32.DefWindowProc(hWnd, msg, (IntPtr)wParam, (IntPtr)lParam);
        }

        public struct DipAwareRect
        {
            public DipAwareRect(Windows.Foundation.Rect bounds)
            {
                x = (int)(bounds.X * ScaleFactor);
                y = (int)(bounds.Y * ScaleFactor);
                cx = (int)(bounds.Width * ScaleFactor);
                cy = (int)(bounds.Height * ScaleFactor);
            }

            public int x;
            public int y;
            public int cx;
            public int cy;
        }
    }
}
