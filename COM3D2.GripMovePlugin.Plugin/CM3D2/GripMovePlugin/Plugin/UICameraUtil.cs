using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class UICameraUtil
	{
		private static UICamera UICamera
		{
			get
			{
				if (UICameraUtil._UICamera == null)
				{
					UICameraUtil._UICamera = UnityEngine.Object.FindObjectOfType<UICamera>();
				}
				return UICameraUtil._UICamera;
			}
		}

		public static Vector3 GetOvrVirtualMousePos()
		{
			if (UICameraUtil.UICamera == null)
			{
				return Vector3.zero;
			}
			return UICameraUtil.UICamera.GetOvrVirtualMouseCurrentSidePos();
		}

		public static void SetOvrVirtualMousePos(Vector3 pos)
		{
			if (UICameraUtil.UICamera != null)
			{
				UICameraUtil.UICamera.SetOvrVirtualMousePos(false, pos);
				UICameraUtil.UICamera.SetOvrVirtualMousePos(true, pos);
			}
		}

		public static Vector3 GetOvrVirtualMouseCurrentSidePos()
		{
			if (UICameraUtil.UICamera != null)
			{
				return UICameraUtil.UICamera.GetOvrVirtualMouseCurrentSidePos();
			}
			return default(Vector3);
		}

		private static UICamera _UICamera;
	}
}
