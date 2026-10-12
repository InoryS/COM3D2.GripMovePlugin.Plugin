using UnityEngine;
using UnityEngine.UI;

namespace COM3D2.GripMovePlugin.Plugin;

internal class GripMoveControllerTouch : GripMoveControllerBase
{
	public OVRInput.Controller ovrController;

	public OvrController ovr_controller;

	private DeviceOculusTouchController device;

	public override DeviceWrapper GetController()
	{
		if (device == null)
		{
			device = gameObject.GetComponent<DeviceOculusTouchController>();
		}

		if (device == null)
		{
			device = gameObject.AddComponent<DeviceOculusTouchController>();
			device.Init(ovrController);
		}

		return device;
	}

	protected override Transform GetHeadTransform() => VRContextUtil.FindOVRHeadTransform();

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (ovr_controller == null)
		{
			return null;
		}

		if (!ovr_controller.m_bHandL)
		{
			return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
		}

		return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
	}

	protected override Text GetTextForMode() => GetComponentInChildren<Text>();

	protected override void PostStart()
	{
		menuTool = gameObject.GetComponent<MenuToolTouch>();
		if (menuTool == null)
		{
			menuTool = gameObject.AddComponent<MenuToolTouch>();
			((MenuToolTouch)menuTool).ovrController = ovrController;
		}
	}

	protected override void PreUpdate()
	{
		if (ovr_controller == null)
		{
			ovr_controller = GetComponent<OvrController>();
		}
	}
}
