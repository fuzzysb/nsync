// Eugene

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace nsync
{
    /// <summary>
    /// MouseUtilities provides a class to call Win32 API to get the correct mouse position on drag and drop actions.
    /// </summary>
    public static class MouseUtilities
    {
        /// <summary>
        /// Win32 API to get the correct position of the mouse point on a drag event
        /// </summary>
        /// <param name="relativeTo">coordinates relative to an WPF control</param>
        /// <returns>returns the coordinates as a point object</returns>
        public static Point CorrectGetPosition(Visual relativeTo)
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
        }

        /// <summary>
        /// A Win32 point with x,y coordinates
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// imported Win32 API for getting cursor position
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
    }

}
