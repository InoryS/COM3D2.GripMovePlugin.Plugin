#if !COM3D25_UNITY_2022
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

internal class GripMoveController : GripMoveControllerBase
{
	private DeviceViveController device;

	private ViveController viveController;

	private Transform vrHeadGameViewTrans;

	protected override bool CheckGripMoveIgnore() =>
		controller.GetTouch((EVRButtonId)32) || base.CheckGripMoveIgnore();

	public override DeviceWrapper GetController()
	{
		if (device == null)
		{
			SteamVR_TrackedObject component = GetComponent<SteamVR_TrackedObject>();
			if (component != null)
			{
				device = gameObject.GetComponent<DeviceViveController>();
				if (device == null)
				{
					device = gameObject.AddComponent<DeviceViveController>();
					device.Init(component);
				}
			}
		}

		return device;
	}

	protected override Transform GetHeadTransform()
	{
		if (vrHeadGameViewTrans == null)
		{
			Component component = CM2COM.FindOvrCamera();
			if (component != null)
			{
				vrHeadGameViewTrans = component.transform;
			}
		}

		return vrHeadGameViewTrans;
	}

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (viveController == null)
		{
			return null;
		}

		if (!viveController.m_bHandL)
		{
			return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
		}

		return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
	}

	protected override Text GetTextForMode() => GetComponentInChildren<Text>();

	protected override void PostStart() => menuTool =
		gameObject.GetComponent<MenuTool>() ?? gameObject.AddComponent<MenuTool>();

	protected override void PreUpdate()
	{
		if (viveController == null)
		{
			viveController = GetComponent<ViveController>();
		}
	}
}
#endif
