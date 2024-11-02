using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000043 RID: 67
	internal static class Win32API
	{
		// Token: 0x06000281 RID: 641
		[DllImport("USER32.dll")]
		private static extern IntPtr GetActiveWindow();

		// Token: 0x06000282 RID: 642
		[DllImport("USER32.dll")]
		public static extern IntPtr GetForegroundWindow();

		// Token: 0x06000283 RID: 643
		[DllImport("USER32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		// Token: 0x06000284 RID: 644
		[DllImport("USER32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		// Token: 0x06000285 RID: 645
		[DllImport("USER32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		// Token: 0x06000286 RID: 646
		[DllImport("USER32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

		// Token: 0x06000287 RID: 647
		[DllImport("USER32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int length);

		// Token: 0x06000288 RID: 648
		[DllImport("USER32.dll")]
		private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		// Token: 0x06000289 RID: 649
		[DllImport("USER32.dll")]
		public static extern void SetCursorPos(int x, int y);

		// Token: 0x0600028A RID: 650
		[DllImport("USER32.dll")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600028B RID: 651 RVA: 0x00003C92 File Offset: 0x00001E92
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

		// Token: 0x0600028C RID: 652 RVA: 0x00011C50 File Offset: 0x0000FE50
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

		// Token: 0x0600028D RID: 653 RVA: 0x00011CB4 File Offset: 0x0000FEB4
		public static RECT GetWindowRect()
		{
			RECT rect = default(RECT);
			Win32API.GetWindowRect(Win32API.WindowH, ref rect);
			return rect;
		}

		// Token: 0x0600028E RID: 654 RVA: 0x00003CB4 File Offset: 0x00001EB4
		public static POINT ClientToScreen(ref POINT pt)
		{
			Win32API.ClientToScreen(Win32API.WindowH, ref pt);
			return pt;
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00003CC8 File Offset: 0x00001EC8
		public static void MouseDown()
		{
			Win32API.mouse_event(2, 0, 0, 0, 0);
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00003CD4 File Offset: 0x00001ED4
		public static void MouseUp()
		{
			Win32API.mouse_event(4, 0, 0, 0, 0);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00003CE0 File Offset: 0x00001EE0
		public static void MouseEvent(int eventFlg)
		{
			Win32API.mouse_event(eventFlg, 0, 0, 0, 0);
		}

		// Token: 0x040001BE RID: 446
		public const int MOUSEEVENTF_MOVE = 1;

		// Token: 0x040001BF RID: 447
		public const int MOUSEEVENTF_LEFTDOWN = 2;

		// Token: 0x040001C0 RID: 448
		public const int MOUSEEVENTF_LEFTUP = 4;

		// Token: 0x040001C1 RID: 449
		public const int MOUSEEVENTF_RIGHTDOWN = 8;

		// Token: 0x040001C2 RID: 450
		public const int MOUSEEVENTF_RIGHTUP = 16;

		// Token: 0x040001C3 RID: 451
		public const int MOUSEEVENTF_MIDDLEDOWN = 32;

		// Token: 0x040001C4 RID: 452
		public const int MOUSEEVENTF_MIDDLEUP = 64;

		// Token: 0x040001C5 RID: 453
		public const int MOUSEEVENTF_ABSOLUTE = 32768;

		// Token: 0x040001C6 RID: 454
		private static IntPtr m_windowH = IntPtr.Zero;
	}
}
