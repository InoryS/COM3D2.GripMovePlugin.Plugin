using System;
using UnityEngine;
using UnityEngine.UI;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class GripMoveController : GripMoveControllerBase
	{
		protected override bool CheckGripMoveIgnore()
		{
			return this.controller.GetTouch(32) || base.CheckGripMoveIgnore();
		}

		public override DeviceWrapper GetController()
		{
			if (this.device == null)
			{
				SteamVR_TrackedObject component = base.GetComponent<SteamVR_TrackedObject>();
				if (component != null)
				{
					this.device = base.gameObject.GetComponent<DeviceViveController>();
					if (this.device == null)
					{
						this.device = base.gameObject.AddComponent<DeviceViveController>();
						this.device.Init(component);
					}
				}
			}
			return this.device;
		}

		protected override Transform GetHeadTransform()
		{
			if (this.vrHeadGameViewTrans == null)
			{
				Component component = CM2COM.FindOvrCamera();
				if (component != null)
				{
					this.vrHeadGameViewTrans = component.transform;
				}
			}
			return this.vrHeadGameViewTrans;
		}

		protected override OvrGripCollider GetOvrGripCollider()
		{
			if (this.viveController == null)
			{
				return null;
			}
			if (!this.viveController.m_bHandL)
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
			this.menuTool = base.gameObject.GetComponent<MenuTool>() ?? base.gameObject.AddComponent<MenuTool>();
		}

		protected override void PreUpdate()
		{
			if (this.viveController == null)
			{
				this.viveController = base.GetComponent<ViveController>();
			}
		}

		private DeviceViveController device;

		private Transform vrHeadGameViewTrans;

		private ViveController viveController;
	}
}
