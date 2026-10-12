using System;

namespace CM3D2.GripMovePlugin.Plugin
{
	public struct RECT
	{
		public RECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public int Width
		{
			get
			{
				return this.right - this.left;
			}
		}

		public int Height
		{
			get
			{
				return this.bottom - this.top;
			}
		}

		public int left;

		public int top;

		public int right;

		public int bottom;
	}
}
