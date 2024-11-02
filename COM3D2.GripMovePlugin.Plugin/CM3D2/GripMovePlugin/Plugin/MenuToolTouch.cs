using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000029 RID: 41
	internal class MenuToolTouch : MenuToolBase
	{
		// Token: 0x06000148 RID: 328 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x06000149 RID: 329 RVA: 0x000024BB File Offset: 0x000006BB
		protected override void ChangeToNextModel()
		{
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00008A3C File Offset: 0x00006C3C
		protected override bool CheckActivateDirectMode()
		{
			if (this.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
			{
				this.resetTimeStart = Time.time;
			}
			return (!(this.Gui != null) || this.IsDirectModeActive()) && (this.controller.GetPress(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - this.resetTimeStart > this.activateOrResetSec);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00002FBF File Offset: 0x000011BF
		protected override bool CheckSwitchCommandTool()
		{
			return !this.device.GetPress(EVRButtonId.k_EButton_Axis1) && this.device.GetPress(EVRButtonId.k_EButton_Grip) && this.device.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00002FEF File Offset: 0x000011EF
		protected override bool CheckSwitchDirectMode()
		{
			return (this.IsDirectModeActive() || this.IsGrabMode()) && (this.controller.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - this.resetTimeStart <= this.activateOrResetSec);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00002FBF File Offset: 0x000011BF
		protected override bool CheckSwitchIKTool()
		{
			return !this.device.GetPress(EVRButtonId.k_EButton_Axis1) && this.device.GetPress(EVRButtonId.k_EButton_Grip) && this.device.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00008AA0 File Offset: 0x00006CA0
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

		// Token: 0x0600014F RID: 335 RVA: 0x00008B04 File Offset: 0x00006D04
		protected override OvrGripCollider GetOvrGripCollider()
		{
			if (this.ovr_controller == null)
			{
				return null;
			}
			if (this.ovr_controller.m_bHandL)
			{
				return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
			}
			return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00002579 File Offset: 0x00000779
		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00003028 File Offset: 0x00001228
		public override bool IsDirectModeActive()
		{
			return this.Gui != null && this.ovr_controller != null && !this.ovr_controller.enabled;
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00003056 File Offset: 0x00001256
		public override bool IsDrawMode()
		{
			return this.ovrControllerBehavior2 != null && this.ovrControllerBehavior2.enabled && (bool)MenuToolTouch.f_m_bMoveDrawMode.GetValue(this.ovrControllerBehavior2);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00008B64 File Offset: 0x00006D64
		private bool IsGrabMode()
		{
			if (base.IsInNewMode())
			{
				if (this.ovrControllerBehavior2 != null)
				{
					return (int)MenuToolTouch.f_m_eMode2.GetValue(this.ovrControllerBehavior2) == 0;
				}
			}
			else if (this.ovrControllerBehavior != null)
			{
				return (int)MenuToolTouch.f_m_eMode.GetValue(this.ovrControllerBehavior) == 0;
			}
			return false;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00008BC8 File Offset: 0x00006DC8
		protected override void OnDirectModeEnabledChanged(bool newMode)
		{
			if (this.ovr_controller != null)
			{
				this.ovr_controller.enabled = !newMode;
			}
			if (base.IsInNewMode())
			{
				if (this.ovrControllerBehavior2 != null)
				{
					this.ovrControllerBehavior2.enabled = !newMode;
					return;
				}
			}
			else if (this.ovrControllerBehavior != null)
			{
				this.ovrControllerBehavior.enabled = !newMode;
			}
		}

		// Token: 0x06000155 RID: 341 RVA: 0x000024BB File Offset: 0x000006BB
		protected override void OnFixedUpdate()
		{
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000024BB File Offset: 0x000006BB
		protected override void PostStart()
		{
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00008C38 File Offset: 0x00006E38
		protected override void PostUpdate()
		{
			DeviceWrapper controller = this.controller;
			if (controller == null)
			{
				return;
			}
			if (controller.GetPressUp(EVRButtonId.k_EButton_Axis0) && this.pressDownTime != 0f)
			{
				Win32API.MouseEvent(4);
				this.pressDownTime = 0f;
			}
			if (this.ovrController == OVRInput.Controller.RTouch && OVRInput.GetUp(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch))
			{
				Win32API.MouseEvent(16);
				this.pressDownTime = Time.time;
			}
			if (this.ovrController == OVRInput.Controller.LTouch && OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch))
			{
				Win32API.MouseEvent(16);
				this.pressDownTime = Time.time;
			}
			if (!this.IsDirectModeActive())
			{
				if (!IMGUIQuad.IsVisble || !IMGUIQuad.Instance.NowAttached)
				{
					return;
				}
			}
			else
			{
				if (!controller.GetPress(EVRButtonId.k_EButton_ApplicationMenu))
				{
					this.resetTimeStart = float.MaxValue;
				}
				if (!this.screenGrabbed && !base.IsCommandToolActive() && !base.IsIKToolActive() && base.IsOvrTabletCtrlOutOfRange())
				{
					Vector2 axis = controller.GetAxis(EVRButtonId.k_EButton_Axis0);
					if (axis != Vector2.zero)
					{
						int num = 1280;
						int num2 = 720;
						this.cursorPosVR = UICameraUtil.GetOvrVirtualMousePos();
						Vector2 vector = axis;
						Vector3 cursorPosVR = this.cursorPosVR;
						cursorPosVR.x += vector.x * (float)num * this.mouseCursorSpeed;
						cursorPosVR.y += vector.y * (float)num2 * this.mouseCursorSpeed;
						UICameraUtil.SetOvrVirtualMousePos(cursorPosVR);
					}
				}
				if (this.commandTool != null && this.commandTool.headTransform == null)
				{
					this.commandTool.headTransform = VRContextUtil.FindOVRHeadTransform();
				}
				if (!this.screenGrabbed && base.IsCommandToolActive())
				{
					Vector2 axis2 = this.controller.GetAxis(EVRButtonId.k_EButton_Axis0);
					if (axis2 != Vector2.zero)
					{
						this.commandTool.UpdateCommandListLocation(axis2.y * 0.2f);
					}
					if (this.controller.GetPressUp(EVRButtonId.k_EButton_Axis0) && (!this.moveToHeadEnabled || this.moveToHeadButton != EVRButtonId.k_EButton_Axis0 || !this.controller.GetPress(this.moveSelfButton)))
					{
						this.commandTool.InvokeCurrentCommand();
					}
				}
			}
			if (this.screenGrabbed || (!base.IsCommandToolActive() && !base.IsIKToolActive() && !this.controller.GetPress(this.grabScreenButton)))
			{
				if (controller.GetPressDown(EVRButtonId.k_EButton_Axis0))
				{
					this.pressLeft = true;
					Win32API.MouseEvent(2);
					this.pressDownTime = Time.time;
				}
				if (this.ovrController == OVRInput.Controller.RTouch && OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch))
				{
					this.pressLeft = false;
					Win32API.MouseEvent(8);
					this.pressDownTime = Time.time;
					return;
				}
				if (this.ovrController == OVRInput.Controller.LTouch && OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch))
				{
					this.pressLeft = false;
					Win32API.MouseEvent(8);
					this.pressDownTime = Time.time;
				}
			}
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00008F10 File Offset: 0x00007110
		protected override void PreUpdate()
		{
			if (this.ovr_controller == null)
			{
				this.ovr_controller = base.GetComponent<OvrController>();
			}
			if (this.ovrControllerBehavior == null)
			{
				this.ovrControllerBehavior = base.GetComponent<OvrControllerBehavior>();
			}
			if (this.ovrControllerBehavior2 == null)
			{
				this.ovrControllerBehavior2 = base.GetComponent<OvrControllerBehavior2>();
			}
			if (this.commandTool != null && this.commandTool.headTransform == null)
			{
				this.commandTool.headTransform = VRContextUtil.FindOVRHeadTransform();
			}
			if (this.IsDirectModeActive() && this.pointer == null)
			{
				base.CreatePointer();
			}
			if (this.pointer != null)
			{
				this.pointer.transform.localPosition = Vector3.zero;
			}
		}

		// Token: 0x06000159 RID: 345 RVA: 0x000024BB File Offset: 0x000006BB
		protected override void RestoreLastDirectModeStatus()
		{
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000308A File Offset: 0x0000128A
		protected override void SaveLastDirectModeStatus(bool mode)
		{
			if (this.ovrController == OVRInput.Controller.LTouch)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
				return;
			}
			if (this.ovrController == OVRInput.Controller.RTouch)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
			}
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00008FDC File Offset: 0x000071DC
		protected override void TurnOffDirectMode()
		{
			if (base.IsInNewMode())
			{
				if (this.ovrControllerBehavior2 != null)
				{
					MenuToolTouch.changeMode2.Invoke(this.ovrControllerBehavior2, new object[] { 1 });
					return;
				}
			}
			else if (this.ovrControllerBehavior != null)
			{
				MenuToolTouch.changeMode.Invoke(this.ovrControllerBehavior, new object[] { 1 });
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00009050 File Offset: 0x00007250
		protected override void TurnOnDirectMode()
		{
			if (base.IsInNewMode())
			{
				if (this.ovrControllerBehavior2 != null && this.IsGrabMode())
				{
					MenuToolTouch.changeMode2.Invoke(this.ovrControllerBehavior2, new object[] { 1 });
					base.DirectModeEnable(true);
					return;
				}
			}
			else if (this.ovrControllerBehavior != null && this.IsGrabMode())
			{
				MenuToolTouch.changeMode.Invoke(this.ovrControllerBehavior, new object[] { 1 });
				base.DirectModeEnable(true);
			}
		}

		// Token: 0x040000F4 RID: 244
		private DeviceOculusTouchController device;

		// Token: 0x040000F5 RID: 245
		public OVRInput.Controller ovrController;

		// Token: 0x040000F6 RID: 246
		public OvrController ovr_controller;

		// Token: 0x040000F7 RID: 247
		private OvrControllerBehavior ovrControllerBehavior;

		// Token: 0x040000F8 RID: 248
		private OvrControllerBehavior2 ovrControllerBehavior2;

		// Token: 0x040000F9 RID: 249
		private Vector3 pointerOffset = Vector3.zero;

		// Token: 0x040000FA RID: 250
		private static MethodInfo changeMode = typeof(OvrControllerBehavior).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		// Token: 0x040000FB RID: 251
		private static MethodInfo changeMode2 = typeof(OvrControllerBehavior2).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		// Token: 0x040000FC RID: 252
		private static FieldInfo f_m_eMode = typeof(OvrControllerBehavior).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000FD RID: 253
		private static FieldInfo f_m_eMode2 = typeof(OvrControllerBehavior2).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000FE RID: 254
		private float resetTimeStart;

		// Token: 0x040000FF RID: 255
		private static FieldInfo f_m_bMoveDrawMode = typeof(OvrControllerBehavior2).GetField("m_bMoveDrawMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
	}
}
