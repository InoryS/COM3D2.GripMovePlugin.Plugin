namespace COM3D2.GripMovePlugin.Plugin;

public struct RECT
{
	public RECT(int left, int top, int right, int bottom)
	{
		this.left = left;
		this.top = top;
		this.right = right;
		this.bottom = bottom;
	}

	public int Width => right - left;

	public int Height => bottom - top;

	public int left;

	public int top;

	public int right;

	public int bottom;
}
