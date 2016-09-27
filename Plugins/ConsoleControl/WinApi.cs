﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ConsoleControl
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class INPUT_RECORD_KEY
    {
        public Int16 EventType = 1;//this is only one of several possible cases of the INPUT_RECORD structure

        //the rest of fields are from KEY_EVENT_RECORD :
        public bool bKeyDown;
        public Int16 wRepeatCount;
        public Int16 wVirtualKeyCode;
        public Int16 wVirtualScanCode;
        public char UnicodeChar;
        public Int32 dwControlKeyState;
    }

    class WinApi
    {
        public const int GWL_STYLE = -16;
        public const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hwnd, out WINDOWINFO wi);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool AdjustWindowRect(ref RECT lpRect, int dwStyle, bool bMenu);

        public static Rectangle GetClientRect(IntPtr hWnd)
        {
            RECT rc;
            GetClientRect(hWnd, out rc);

            return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        public static void ResizeClientRectTo(IntPtr hWnd, Rectangle desired)
        {
            RECT size;
            size.Top = desired.Top;
            size.Left = desired.Left;
            size.Bottom = desired.Bottom;
            size.Right = desired.Right;

            var style = GetWindowLong(hWnd, GWL_STYLE);
            AdjustWindowRect(ref size, style, false);
            MoveWindow(hWnd, size.Left, size.Top, size.Right - size.Left, size.Bottom - size.Top, true);
        }
    }
}
