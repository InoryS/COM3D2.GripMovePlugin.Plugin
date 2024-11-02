using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200003E RID: 62
	internal class MenuTool : MenuToolBase
	{
		// Token: 0x06000237 RID: 567 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000E708 File Offset: 0x0000C908
		protected override void ChangeToNextModel()
		{
			if (!this.modelReplaceTool.enabled)
			{
				return;
			}
			SteamVR_TrackedObject component = GameMain.Instance.OvrMgr.ovr_obj.left_controller.controller.gameObject.GetComponent<SteamVR_TrackedObject>();
			List<VIVEModelReplaceTool.ModelInfo> list = ((component == null || component.index != this.tracked.index) ? this.modelReplaceTool.models_R : this.modelReplaceTool.models_L);
			int num = this.currentModelIndex + 1;
			if (num >= list.Count<VIVEModelReplaceTool.ModelInfo>())
			{
				num = 0;
			}
			this.currentModelIndex = num;
			this.modelReplaceTool.SetModel(this.tracked, list[this.currentModelIndex].id);
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000E7BC File Offset: 0x0000C9BC
		protected override bool CheckActivateDirectMode()
		{
			if (this.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
			{
				this.menuPressStart = Time.time;
			}
			return (!(this.Gui != null) || this.IsDirectModeActive()) && (this.controller.GetPress(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - this.menuPressStart > this.activateOrResetSec);
		}

		// Token: 0x0600023A RID: 570 RVA: 0x00003A0E File Offset: 0x00001C0E
		protected override bool CheckSwitchCommandTool()
		{
			return this.controller.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu) && this.controller.GetPress(this.yotogiToolButton);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00003A31 File Offset: 0x00001C31
		protected override bool CheckSwitchDirectMode()
		{
			return (this.IsGrabMode() || this.IsDirectModeActive()) && (this.controller.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - this.menuPressStart <= this.activateOrResetSec);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x00003A6A File Offset: 0x00001C6A
		protected override bool CheckSwitchIKTool()
		{
			return this.controller.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu) && this.controller.GetPress(this.IKToolButton);
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000E820 File Offset: 0x0000CA20
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

		// Token: 0x0600023E RID: 574 RVA: 0x0000E890 File Offset: 0x0000CA90
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

		// Token: 0x0600023F RID: 575 RVA: 0x00002579 File Offset: 0x00000779
		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00003A8D File Offset: 0x00001C8D
		public override bool IsDirectModeActive()
		{
			return !(this.Gui == null) && !(this.viveController == null) && !this.viveController.enabled;
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00003ABB File Offset: 0x00001CBB
		public override bool IsDrawMode()
		{
			return !(this.viveControllerBehavior2 == null) && this.viveControllerBehavior2.enabled && (bool)MenuTool.f_m_bMoveDrawMode.GetValue(this.viveControllerBehavior2);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000E8F0 File Offset: 0x0000CAF0
		private bool IsGrabMode()
		{
			if (base.IsInNewMode())
			{
				if (this.viveControllerBehavior2 != null)
				{
					return (int)MenuTool.f_m_eMode2.GetValue(this.viveControllerBehavior2) == 0;
				}
			}
			else if (this.viveControllerBehavior != null)
			{
				return (int)MenuTool.f_m_eMode.GetValue(this.viveControllerBehavior) == 0;
			}
			return false;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000E954 File Offset: 0x0000CB54
		protected override void OnDirectModeEnabledChanged(bool newMode)
		{
			if (this.viveController != null)
			{
				this.viveController.enabled = !newMode;
			}
			if (base.IsInNewMode())
			{
				if (this.viveControllerBehavior2 != null)
				{
					this.viveControllerBehavior2.enabled = !newMode;
					return;
				}
			}
			else if (this.viveControllerBehavior != null)
			{
				this.viveControllerBehavior.enabled = !newMode;
			}
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000E9C4 File Offset: 0x0000CBC4
		protected override void OnFixedUpdate()
		{
			DeviceWrapper controller = this.controller;
			if (controller == null)
			{
				return;
			}
			if (!this.IsDirectModeActive() && (!IMGUIQuad.IsVisble || !IMGUIQuad.Instance.NowAttached))
			{
				return;
			}
			if (this.screenGrabbed || (!base.IsCommandToolActive() && !base.IsIKToolActive() && !this.controller.GetPress(this.grabScreenButton)))
			{
				if (controller.GetPressDown(EVRButtonId.k_EButton_Axis0))
				{
					this.pressLeft = controller.GetAxis(EVRButtonId.k_EButton_Axis0).x < 0.3f;
					Win32API.MouseEvent(this.pressLeft ? 2 : 8);
					this.pressDownTime = Time.time;
				}
				if (controller.GetPressUp(EVRButtonId.k_EButton_Axis0))
				{
					Win32API.MouseEvent(this.pressLeft ? 4 : 16);
					this.pressDownTime = 0f;
				}
			}
		}

		// Token: 0x06000245 RID: 581 RVA: 0x00003AEF File Offset: 0x00001CEF
		protected override void PostStart()
		{
			this.tracked = base.GetComponent<SteamVR_TrackedObject>();
			this.modelReplaceTool = VIVEModelReplaceTool.Instance;
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000EA94 File Offset: 0x0000CC94
		protected override void PostUpdate()
		{
			base.PostUpdate();
			if (!this.screenGrabbed && !base.IsCommandToolActive() && !base.IsIKToolActive() && base.IsOvrTabletCtrlOutOfRange())
			{
				if (this.device.GetTouchDown(EVRButtonId.k_EButton_Axis0))
				{
					this.touchDownPosition = this.device.GetAxis(EVRButtonId.k_EButton_Axis0);
				}
				if (this.device.GetTouch(EVRButtonId.k_EButton_Axis0) && Time.time - this.pressDownTime > 0.3f)
				{
					int num = 1280;
					int num2 = 720;
					this.cursorPosVR = UICameraUtil.GetOvrVirtualMousePos();
					Vector2 axis = this.device.GetAxis(EVRButtonId.k_EButton_Axis0);
					Vector2 vector = axis - this.touchDownPosition;
					Vector3 cursorPosVR = this.cursorPosVR;
					cursorPosVR.x += vector.x * (float)num * this.mouseCursorSpeed;
					cursorPosVR.y += vector.y * (float)num2 * this.mouseCursorSpeed;
					UICameraUtil.SetOvrVirtualMousePos(cursorPosVR);
					this.touchDownPosition = axis;
				}
			}
			if (!this.screenGrabbed && base.IsCommandToolActive())
			{
				if (this.controller.GetTouchDown(EVRButtonId.k_EButton_Axis0))
				{
					this.touchDownPosition = this.controller.GetAxis(EVRButtonId.k_EButton_Axis0);
				}
				if (this.controller.GetTouch(EVRButtonId.k_EButton_Axis0) && Time.time - this.pressDownTime > 0.3f)
				{
					Vector2 axis2 = this.controller.GetAxis(EVRButtonId.k_EButton_Axis0);
					float x = (axis2 - this.touchDownPosition).x;
					this.commandTool.UpdateCommandListLocation(x);
					this.touchDownPosition = axis2;
				}
				if (this.controller.GetPressDown(EVRButtonId.k_EButton_Axis0))
				{
					this.pressDownTime = Time.time;
				}
				if (this.controller.GetPressUp(EVRButtonId.k_EButton_Axis0))
				{
					if (!this.moveToHeadEnabled || this.moveToHeadButton != EVRButtonId.k_EButton_Axis0 || !this.controller.GetPress(this.moveSelfButton))
					{
						this.commandTool.InvokeCurrentCommand();
					}
					this.pressDownTime = 0f;
				}
			}
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0000EC9C File Offset: 0x0000CE9C
		protected override void PreUpdate()
		{
			if (this.viveController == null)
			{
				this.viveController = base.GetComponent<ViveController>();
			}
			if (this.viveControllerBehavior == null)
			{
				this.viveControllerBehavior = base.GetComponent<ViveControllerBehavior>();
			}
			if (this.viveControllerBehavior2 == null)
			{
				this.viveControllerBehavior2 = base.GetComponent<ViveControllerBehavior2>();
			}
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0000ECF8 File Offset: 0x0000CEF8
		protected override void RestoreLastDirectModeStatus()
		{
			OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
			OvrMgr.OvrObject.Controller left_controller = ovr_obj.left_controller;
			OvrMgr.OvrObject.Controller right_controller = ovr_obj.right_controller;
			if (left_controller.controller != null && left_controller.controller.gameObject == base.gameObject)
			{
				this.needActivateDirectMode = Settings.Instance.GetBoolValue("LastDirectMode_L", false);
				return;
			}
			if (right_controller.controller != null && right_controller.controller.gameObject == base.gameObject)
			{
				this.needActivateDirectMode = Settings.Instance.GetBoolValue("LastDirectMode_R", false);
			}
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000ED9C File Offset: 0x0000CF9C
		protected override void SaveLastDirectModeStatus(bool mode)
		{
			OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
			OvrMgr.OvrObject.Controller left_controller = ovr_obj.left_controller;
			OvrMgr.OvrObject.Controller right_controller = ovr_obj.right_controller;
			if (left_controller.controller != null && left_controller.controller.gameObject == base.gameObject)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
				return;
			}
			if (right_controller.controller != null && right_controller.controller.gameObject == base.gameObject)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
			}
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000EE34 File Offset: 0x0000D034
		protected override void TurnOffDirectMode()
		{
			if (base.IsInNewMode())
			{
				if (this.viveControllerBehavior2 != null)
				{
					MenuTool.changeMode2.Invoke(this.viveControllerBehavior2, new object[] { 1 });
					return;
				}
			}
			else if (this.viveControllerBehavior != null)
			{
				MenuTool.changeMode.Invoke(this.viveControllerBehavior, new object[] { 1 });
			}
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000EEA8 File Offset: 0x0000D0A8
		protected override void TurnOnDirectMode()
		{
			if (base.IsInNewMode())
			{
				if (this.viveControllerBehavior2 != null && this.IsGrabMode())
				{
					MenuTool.changeMode2.Invoke(this.viveControllerBehavior2, new object[] { 1 });
					base.DirectModeEnable(true);
					return;
				}
			}
			else if (this.viveControllerBehavior != null && this.IsGrabMode())
			{
				MenuTool.changeMode.Invoke(this.viveControllerBehavior, new object[] { 1 });
				base.DirectModeEnable(true);
			}
		}

		// Token: 0x04000188 RID: 392
		private DeviceViveController device;

		// Token: 0x04000189 RID: 393
		private SteamVR_TrackedObject tracked;

		// Token: 0x0400018A RID: 394
		private ViveController viveController;

		// Token: 0x0400018B RID: 395
		private ViveControllerBehavior viveControllerBehavior;

		// Token: 0x0400018C RID: 396
		private ViveControllerBehavior2 viveControllerBehavior2;

		// Token: 0x0400018D RID: 397
		private VIVEModelReplaceTool modelReplaceTool;

		// Token: 0x0400018E RID: 398
		private int currentModelIndex;

		// Token: 0x0400018F RID: 399
		protected float menuPressStart;

		// Token: 0x04000190 RID: 400
		private static MethodInfo changeMode = typeof(ViveControllerBehavior).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		// Token: 0x04000191 RID: 401
		private static MethodInfo changeMode2 = typeof(ViveControllerBehavior2).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		// Token: 0x04000192 RID: 402
		private static FieldInfo f_m_eMode = typeof(ViveControllerBehavior).GetField("m_eMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x04000193 RID: 403
		private static FieldInfo f_m_eMode2 = typeof(ViveControllerBehavior2).GetField("m_eMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x04000194 RID: 404
		private Vector2 touchDownPosition;

		// Token: 0x04000195 RID: 405
		private static FieldInfo f_m_bMoveDrawMode = typeof(ViveControllerBehavior2).GetField("m_bMoveDrawMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
	}
}
