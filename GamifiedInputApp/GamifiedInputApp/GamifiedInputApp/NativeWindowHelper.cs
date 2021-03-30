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
        MainWindow m_mainWindow;
        GCHandle m_pinnedWindowsProcedureDelegate;

        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static readonly double ScaleFactor = PInvoke.User32.GetDpiForSystem() / 96.0;

        public NativeWindowHelper(MainWindow mainWindow)
        {
            m_mainWindow = mainWindow;

            string className = "Minigame Window Class";
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

            DipAwareRect rect = new(m_mainWindow.GameBounds);
            IntPtr parent = m_mainWindow.Handle;

            PInvoke.User32.WindowStyles dwStyle = parent != IntPtr.Zero ?
                PInvoke.User32.WindowStyles.WS_CHILD : PInvoke.User32.WindowStyles.WS_OVERLAPPEDWINDOW;

            m_hwnd = PInvoke.User32.CreateWindowEx(
                    0,
                    className,
                    "Minigame Window",
                    dwStyle,
                    rect.x,
                    rect.y,
                    rect.cx,
                    rect.cy,
                    parent,
                    new IntPtr(),
                    new IntPtr(),
                    new IntPtr());
        }

        public void Show()
        {
            UpdateWindowPos(m_mainWindow.GameBounds);
            m_mainWindow.BoundsUpdated += M_mainWindow_BoundsUpdated;
        }

        public void Hide()
        {
            m_mainWindow.BoundsUpdated -= M_mainWindow_BoundsUpdated;
            PInvoke.User32.SetWindowPos(
                m_hwnd,
                HWND_TOP,
                0,
                0,
                0,
                0,
                PInvoke.User32.SetWindowPosFlags.SWP_HIDEWINDOW | PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE | PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE);
        }

        private void M_mainWindow_BoundsUpdated(object sender, BoundsUpdatedEventArgs args) => UpdateWindowPos(args.NewBounds);

        private void UpdateWindowPos(ScalingRect gameBounds)
        {
            DipAwareRect rect = new(gameBounds);
            PInvoke.User32.SetWindowPos(
                m_hwnd,
                HWND_TOP,
                rect.x,
                rect.y,
                rect.cx,
                rect.cy,
                PInvoke.User32.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        public void Destroy()
        {
            PInvoke.User32.DestroyWindow(m_hwnd);
            m_pinnedWindowsProcedureDelegate.Free();
            m_hwnd = IntPtr.Zero;
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
            return PInvoke.User32.DefWindowProc(hWnd, msg, (IntPtr)wParam, (IntPtr)lParam);
        }

        public struct DipAwareRect
        {
            public DipAwareRect(ScalingRect bounds)
            {
                x = (int)(bounds.Scaled.X * ScaleFactor);
                y = (int)(bounds.Scaled.Y * ScaleFactor);
                cx = (int)(bounds.Scaled.Width * ScaleFactor);
                cy = (int)(bounds.Scaled.Height * ScaleFactor);
            }

            public int x;
            public int y;
            public int cx;
            public int cy;
        }
    }
}
