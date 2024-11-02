using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200002A RID: 42
	public class UICameraUtil
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600015D RID: 349 RVA: 0x000030BF File Offset: 0x000012BF
		private static UICamera UICamera
		{
			get
			{
				if (UICameraUtil._UICamera == null)
				{
					UICameraUtil._UICamera = global::UnityEngine.Object.FindObjectOfType<UICamera>();
				}
				return UICameraUtil._UICamera;
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x000030DD File Offset: 0x000012DD
		public static Vector3 GetOvrVirtualMousePos()
		{
			if (UICameraUtil.UICamera == null)
			{
				return Vector3.zero;
			}
			return UICameraUtil.UICamera.GetOvrVirtualMouseCurrentSidePos();
		}

		// Token: 0x06000161 RID: 353 RVA: 0x000030FC File Offset: 0x000012FC
		public static void SetOvrVirtualMousePos(Vector3 pos)
		{
			if (UICameraUtil.UICamera != null)
			{
				UICameraUtil.UICamera.SetOvrVirtualMousePos(false, pos);
				UICameraUtil.UICamera.SetOvrVirtualMousePos(true, pos);
			}
		}

		// Token: 0x06000162 RID: 354 RVA: 0x000090E0 File Offset: 0x000072E0
		public static Vector3 GetOvrVirtualMouseCurrentSidePos()
		{
			if (UICameraUtil.UICamera != null)
			{
				return UICameraUtil.UICamera.GetOvrVirtualMouseCurrentSidePos();
			}
			return default(Vector3);
		}

		// Token: 0x04000100 RID: 256
		private static UICamera _UICamera;
	}
}
