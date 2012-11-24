using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace SimpleWPFChart
{
  public static class Utilities
  {

    public static Point PointToWindow(UIElement element, Point pointOnElement) 
    { 
        Window wnd = Window.GetWindow(element);
        if (wnd == null) 
        { 
            throw new InvalidOperationException("target element is not yet connected to the Window drawing surface"); 
        } 
        return element.TransformToAncestor(wnd).Transform(pointOnElement); }

    public static Point CorrectGetPosition(Visual relativeTo)
    {
      Win32Point w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);
      return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);
  }
}
