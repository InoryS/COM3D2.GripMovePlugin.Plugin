using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200002D RID: 45
	internal class VRContextUtil
	{
		// Token: 0x06000176 RID: 374 RVA: 0x0000315F File Offset: 0x0000135F
		public static Transform FindOVRHeadTransform()
		{
			VRContextUtil.headTrans = VRContextUtil.headTrans ?? VRContextUtil.FindOVRHeadTransformDirect();
			return VRContextUtil.headTrans;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000096C4 File Offset: 0x000078C4
		public static Transform FindOVRHeadTransformDirect()
		{
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
			{
				OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInParent<OVRCameraRig>();
				if (componentInParent != null)
				{
					return componentInParent.centerEyeAnchor;
				}
			}
			else if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
			{
				Component component = CM2COM.FindOvrCamera();
				if (component != null)
				{
					return component.transform;
				}
			}
			return null;
		}

		// Token: 0x0400010D RID: 269
		private static Transform headTrans;
	}
}
