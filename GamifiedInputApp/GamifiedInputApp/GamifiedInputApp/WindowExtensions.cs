using System;
using System.Runtime.InteropServices;
using WinRT;

namespace GamifiedInputApp
{
    public static class WindowExtensions
    {
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }

        public static IntPtr GetWindowHandle(this Microsoft.UI.Xaml.Window window)
            => window is null ? throw new ArgumentNullException(nameof(window)) : window.As<IWindowNative>().WindowHandle;
    }
}
