using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class VRContextUtil
	{
		public static Transform FindOVRHeadTransform()
		{
			VRContextUtil.headTrans = VRContextUtil.headTrans ?? VRContextUtil.FindOVRHeadTransformDirect();
			return VRContextUtil.headTrans;
		}

		public static Transform FindOVRHeadTransformDirect()
		{
			if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
			{
				OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInParent<OVRCameraRig>();
				if (componentInParent != null)
				{
					return componentInParent.centerEyeAnchor;
				}
			}
			else if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
			{
				Component component = CM2COM.FindOvrCamera();
				if (component != null)
				{
					return component.transform;
				}
			}
			return null;
		}

		private static Transform headTrans;
	}
}
