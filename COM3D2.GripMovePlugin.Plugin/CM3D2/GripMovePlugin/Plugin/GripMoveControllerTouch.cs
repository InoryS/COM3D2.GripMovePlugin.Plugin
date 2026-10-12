using System;
using UnityEngine;
using UnityEngine.UI;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class GripMoveControllerTouch : GripMoveControllerBase
	{
		public override DeviceWrapper GetController()
		{
			if (this.device == null)
			{
				this.device = base.gameObject.GetComponent<DeviceOculusTouchController>();
			}
			if (this.device == null)
			{
				this.device = base.gameObject.AddComponent<DeviceOculusTouchController>();
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
			if (this.ovr_controller == null)
			{
				return null;
			}
			if (!this.ovr_controller.m_bHandL)
			{
				return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
			}
			return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
		}

		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		protected override void PostStart()
		{
			this.menuTool = base.gameObject.GetComponent<MenuToolTouch>();
			if (this.menuTool == null)
			{
				this.menuTool = base.gameObject.AddComponent<MenuToolTouch>();
				((MenuToolTouch)this.menuTool).ovrController = this.ovrController;
			}
		}

		protected override void PreUpdate()
		{
			if (this.ovr_controller == null)
			{
				this.ovr_controller = base.GetComponent<OvrController>();
			}
		}

		private DeviceOculusTouchController device;

		public OVRInput.Controller ovrController;

		public OvrController ovr_controller;
	}
}
