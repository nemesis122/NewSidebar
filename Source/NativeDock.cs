using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace Aurora;
public sealed class NativeDock
{
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct Data { public uint Size; public IntPtr Window; public uint Callback, Edge; public Rect Bounds; public IntPtr Param; }
    [DllImport("shell32.dll")] private static extern UIntPtr SHAppBarMessage(uint message, ref Data data);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] private static extern uint RegisterWindowMessage(string value);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int cx, int cy, uint flags);
    private readonly Window window;
    private bool registered;
    private Rect? lastBounds;
    private uint callback;
    private Action? changed;
    public NativeDock(Window window) { this.window = window; }
    public void Hook(Action onChanged)
    {
        changed = onChanged;
        callback = RegisterWindowMessage("AuroraSidebar.AppBar");
        HwndSource.FromHwnd(new WindowInteropHelper(window).Handle)?.AddHook((IntPtr h, int m, IntPtr w, IntPtr l, ref bool handled) => {
            if ((uint)m == callback && w.ToInt32() == 1) window.Dispatcher.BeginInvoke(new Action(() => changed?.Invoke()));
            return IntPtr.Zero;
        });
    }
    public void Release()
    {
        if (!registered) return;
        var d = new Data { Size = (uint)Marshal.SizeOf<Data>(), Window = new WindowInteropHelper(window).Handle };
        registered = false; lastBounds = null; SHAppBarMessage(1, ref d);
    }
    public void Place(Preferences p)
    {
        var screen = Array.Find(Forms.Screen.AllScreens, s => s.DeviceName == p.Monitor) ?? Forms.Screen.PrimaryScreen ?? Forms.Screen.AllScreens[0];
        var b = screen.WorkingArea;
        var handle = new WindowInteropHelper(window).Handle;
        double scale = VisualTreeHelperScale();
        int width = (int)Math.Round(p.ColumnWidth * p.Columns * scale);
        width = Math.Min(width, p.ReserveSpace ? screen.Bounds.Width : b.Width);
        uint edge = p.Side == "Links" ? 0u : 2u;
        if (!p.ReserveSpace) Release();
        b = p.ReserveSpace ? screen.Bounds : screen.WorkingArea;
        var rect = new Rect { Left = edge == 0 ? b.Left : b.Right-width, Top = b.Top, Right = edge == 0 ? b.Left+width : b.Right, Bottom = b.Bottom };
        if (p.ReserveSpace && window.IsVisible)
        {
            var d = new Data { Size = (uint)Marshal.SizeOf<Data>(), Window = handle, Callback = callback, Edge = edge, Bounds = rect };
            if (!registered) { SHAppBarMessage(0, ref d); registered = true; }
            SHAppBarMessage(2, ref d);
            if (edge == 0) d.Bounds.Right = d.Bounds.Left + width; else d.Bounds.Left = d.Bounds.Right - width;
            if (lastBounds is not Rect last || last.Left != d.Bounds.Left || last.Top != d.Bounds.Top || last.Right != d.Bounds.Right || last.Bottom != d.Bounds.Bottom)
            { lastBounds = d.Bounds; SHAppBarMessage(3, ref d); }
            rect = d.Bounds;
        }
        SetWindowPos(handle, IntPtr.Zero, rect.Left, rect.Top, rect.Right-rect.Left, rect.Bottom-rect.Top, 0x0014);
    }
    private double VisualTreeHelperScale() => System.Windows.Media.VisualTreeHelper.GetDpi(window).DpiScaleX;
}
