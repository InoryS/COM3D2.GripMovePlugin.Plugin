using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

public class UICameraUtil
{
	private static UICamera _UICamera;

	private static UICamera UICamera
	{
		get
		{
			if (_UICamera == null)
			{
				_UICamera = Object.FindObjectOfType<UICamera>();
			}

			return _UICamera;
		}
	}

	public static Vector3 GetOvrVirtualMousePos()
	{
		if (UICamera == null)
		{
			return Vector3.zero;
		}

		return UICamera.GetOvrVirtualMouseCurrentSidePos();
	}

	public static void SetOvrVirtualMousePos(Vector3 pos)
	{
		if (UICamera != null)
		{
			UICamera.SetOvrVirtualMousePos(false, pos);
			UICamera.SetOvrVirtualMousePos(true, pos);
		}
	}

	public static Vector3 GetOvrVirtualMouseCurrentSidePos()
	{
		if (UICamera != null)
		{
			return UICamera.GetOvrVirtualMouseCurrentSidePos();
		}

		return default;
	}
}
