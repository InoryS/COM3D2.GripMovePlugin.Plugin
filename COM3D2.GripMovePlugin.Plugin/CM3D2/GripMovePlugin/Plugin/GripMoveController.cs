using System;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000027 RID: 39
	internal class GripMoveController : GripMoveControllerBase
	{
		// Token: 0x06000138 RID: 312 RVA: 0x00002F55 File Offset: 0x00001155
		protected override bool CheckGripMoveIgnore()
		{
			return this.controller.GetTouch(EVRButtonId.k_EButton_Axis0) || base.CheckGripMoveIgnore();
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00008460 File Offset: 0x00006660
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

		// Token: 0x0600013A RID: 314 RVA: 0x000084D0 File Offset: 0x000066D0
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

		// Token: 0x0600013B RID: 315 RVA: 0x0000850C File Offset: 0x0000670C
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

		// Token: 0x0600013C RID: 316 RVA: 0x00002579 File Offset: 0x00000779
		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00002F6E File Offset: 0x0000116E
		protected override void PostStart()
		{
			this.menuTool = base.gameObject.GetComponent<MenuTool>() ?? base.gameObject.AddComponent<MenuTool>();
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00002F90 File Offset: 0x00001190
		protected override void PreUpdate()
		{
			if (this.viveController == null)
			{
				this.viveController = base.GetComponent<ViveController>();
			}
		}

		// Token: 0x040000ED RID: 237
		private DeviceViveController device;

		// Token: 0x040000EE RID: 238
		private Transform vrHeadGameViewTrans;

		// Token: 0x040000EF RID: 239
		private ViveController viveController;
	}
}
