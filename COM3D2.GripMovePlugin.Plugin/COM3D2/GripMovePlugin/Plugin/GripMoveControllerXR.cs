#if COM3D25_UNITY_2022
using System;
using UnityEngine;
using UnityEngine.UI;

namespace COM3D2.GripMovePlugin.Plugin;

internal class GripMoveControllerXR : GripMoveControllerBase
{
	public OvrMgr.OvrObject.Controller ovrController;

	public AVRController ovr_controller;

	private DeviceXRController device;

	public override DeviceWrapper GetController()
	{
		if (this.device == null)
		{
			this.device = base.gameObject.GetComponent<DeviceXRController>();
		}
		if (this.device == null)
		{
			this.device = base.gameObject.AddComponent<DeviceXRController>();
			this.device.Init(this.ovrController);
		}
		return this.device;
	}

	protected override Transform GetHeadTransform()
	{
		return VRContextUtil.FindOVRHeadTransform();
	}

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (this.ovrController == null) return null;
		return this.ovrController.grip_collider;
	}

	protected override Text GetTextForMode()
	{
		return base.GetComponentInChildren<Text>();
	}

	protected override void PostStart()
	{
		this.menuTool = base.gameObject.GetComponent<MenuToolXR>();
		if (this.menuTool == null)
		{
			this.menuTool = base.gameObject.AddComponent<MenuToolXR>();
			((MenuToolXR)this.menuTool).ovrController = this.ovrController;
		}
	}

	protected override void PreUpdate()
	{
		if (this.ovr_controller == null)
		{
			this.ovr_controller = GameMain.Instance.OvrMgr.ovr_obj.left_controller.controller;
		}
	}
}
#endif
