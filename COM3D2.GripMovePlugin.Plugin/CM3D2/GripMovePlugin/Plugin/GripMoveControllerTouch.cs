using System;
using UnityEngine;
using UnityEngine.UI;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200000D RID: 13
	internal class GripMoveControllerTouch : GripMoveControllerBase
	{
		// Token: 0x0600003C RID: 60 RVA: 0x00004EC8 File Offset: 0x000030C8
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

		// Token: 0x0600003D RID: 61 RVA: 0x00002572 File Offset: 0x00000772
		protected override Transform GetHeadTransform()
		{
			return VRContextUtil.FindOVRHeadTransform();
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00004F2C File Offset: 0x0000312C
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

		// Token: 0x0600003F RID: 63 RVA: 0x00002579 File Offset: 0x00000779
		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00004F8C File Offset: 0x0000318C
		protected override void PostStart()
		{
			this.menuTool = base.gameObject.GetComponent<MenuToolTouch>();
			if (this.menuTool == null)
			{
				this.menuTool = base.gameObject.AddComponent<MenuToolTouch>();
				((MenuToolTouch)this.menuTool).ovrController = this.ovrController;
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002581 File Offset: 0x00000781
		protected override void PreUpdate()
		{
			if (this.ovr_controller == null)
			{
				this.ovr_controller = base.GetComponent<OvrController>();
			}
		}

		// Token: 0x04000027 RID: 39
		private DeviceOculusTouchController device;

		// Token: 0x04000028 RID: 40
		public OVRInput.Controller ovrController;

		// Token: 0x04000029 RID: 41
		public OvrController ovr_controller;
	}
}
