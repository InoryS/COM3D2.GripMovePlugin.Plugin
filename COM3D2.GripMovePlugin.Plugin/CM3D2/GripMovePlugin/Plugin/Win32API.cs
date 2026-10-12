using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal static class Win32API
	{
		[DllImport("USER32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("USER32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("USER32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("USER32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		[DllImport("USER32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[DllImport("USER32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

		[DllImport("USER32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int length);

		[DllImport("USER32.dll")]
		private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("USER32.dll")]
		public static extern void SetCursorPos(int x, int y);

		[DllImport("USER32.dll")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		public static IntPtr WindowH
		{
			get
			{
				if (Win32API.m_windowH == IntPtr.Zero)
				{
					Win32API.m_windowH = Win32API.GetActiveWindow();
				}
				return Win32API.m_windowH;
			}
		}

		public static void SetForeground()
		{
			IntPtr foregroundWindow = Win32API.GetForegroundWindow();
			if (Win32API.m_windowH != foregroundWindow)
			{
				uint windowThreadProcessId = Win32API.GetWindowThreadProcessId(Win32API.m_windowH, IntPtr.Zero);
				uint windowThreadProcessId2 = Win32API.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
				if (windowThreadProcessId2 != windowThreadProcessId)
				{
					Win32API.AttachThreadInput(windowThreadProcessId, windowThreadProcessId2, true);
				}
				Win32API.SetForegroundWindow(Win32API.m_windowH);
				if (windowThreadProcessId2 != windowThreadProcessId)
				{
					Win32API.AttachThreadInput(windowThreadProcessId, windowThreadProcessId2, false);
				}
			}
		}

		public static RECT GetWindowRect()
		{
			RECT rect = default(RECT);
			Win32API.GetWindowRect(Win32API.WindowH, ref rect);
			return rect;
		}

		public static POINT ClientToScreen(ref POINT pt)
		{
			Win32API.ClientToScreen(Win32API.WindowH, ref pt);
			return pt;
		}

		public static void MouseDown()
		{
			Win32API.mouse_event(2, 0, 0, 0, 0);
		}

		public static void MouseUp()
		{
			Win32API.mouse_event(4, 0, 0, 0, 0);
		}

		public static void MouseEvent(int eventFlg)
		{
			Win32API.mouse_event(eventFlg, 0, 0, 0, 0);
		}

		public const int MOUSEEVENTF_MOVE = 1;

		public const int MOUSEEVENTF_LEFTDOWN = 2;

		public const int MOUSEEVENTF_LEFTUP = 4;

		public const int MOUSEEVENTF_RIGHTDOWN = 8;

		public const int MOUSEEVENTF_RIGHTUP = 16;

		public const int MOUSEEVENTF_MIDDLEDOWN = 32;

		public const int MOUSEEVENTF_MIDDLEUP = 64;

		public const int MOUSEEVENTF_ABSOLUTE = 32768;

		private static IntPtr m_windowH = IntPtr.Zero;
	}
}
