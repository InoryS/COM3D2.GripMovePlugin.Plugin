using System;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000045 RID: 69
	public struct RECT
	{
		// Token: 0x06000294 RID: 660 RVA: 0x00003D08 File Offset: 0x00001F08
		public RECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000295 RID: 661 RVA: 0x00003D27 File Offset: 0x00001F27
		public int Width
		{
			get
			{
				return this.right - this.left;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000296 RID: 662 RVA: 0x00003D36 File Offset: 0x00001F36
		public int Height
		{
			get
			{
				return this.bottom - this.top;
			}
		}

		// Token: 0x040001C9 RID: 457
		public int left;

		// Token: 0x040001CA RID: 458
		public int top;

		// Token: 0x040001CB RID: 459
		public int right;

		// Token: 0x040001CC RID: 460
		public int bottom;
	}
}
