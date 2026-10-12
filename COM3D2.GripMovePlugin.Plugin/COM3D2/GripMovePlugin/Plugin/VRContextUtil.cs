using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class VRContextUtil
{
	private static Transform headTrans;

	public static Transform FindOVRHeadTransform()
	{
		headTrans = headTrans ?? FindOVRHeadTransformDirect();
		return headTrans;
	}

	public static Transform FindOVRHeadTransformDirect()
	{
#if COM3D25_UNITY_2022
		// COM3D2.5 / XR：直接按节点名找头显，不依赖 OVRCameraRig
		if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
		{
			return GameObject.Find("TrackingSpace/CenterEyeAnchor").transform;
		}

		if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
		{
			return GameObject.Find("Main Camera (eye)").transform;
		}

		return null;
#else
		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
		{
			OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject
				.GetComponentInParent<OVRCameraRig>();
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
#endif
	}
}
