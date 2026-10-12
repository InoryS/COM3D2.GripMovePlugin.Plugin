using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class MenuToolTouch : MenuToolBase
	{
		private void Awake()
		{
		}

		protected override void ChangeToNextModel()
		{
		}

		protected override bool CheckActivateDirectMode()
		{
			if (this.controller.GetPressDown(1))
			{
				this.resetTimeStart = Time.time;
			}
			return (this.Gui == null || this.IsDirectModeActive()) && (this.controller.GetPress(1) && Time.time - this.resetTimeStart > this.activateOrResetSec);
		}

		protected override bool CheckSwitchCommandTool()
		{
			return !this.device.GetPress(33) && this.device.GetPress(2) && this.device.GetPressUp(1);
		}

		protected override bool CheckSwitchDirectMode()
		{
			return (this.IsDirectModeActive() || this.IsGrabMode()) && (this.controller.GetPressUp(1) && Time.time - this.resetTimeStart <= this.activateOrResetSec);
		}

		protected override bool CheckSwitchIKTool()
		{
			return !this.device.GetPress(33) && this.device.GetPress(2) && this.device.GetPressUp(1);
		}

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

		protected override Text GetTextForMode()
		{
			return base.GetComponentInChildren<Text>();
		}

		public override bool IsDirectModeActive()
		{
			return this.Gui != null && this.ovr_controller != null && !this.ovr_controller.enabled;
		}

		public override bool IsDrawMode()
		{
			return this.ovrControllerBehavior2 != null && this.ovrControllerBehavior2.enabled && (bool)MenuToolTouch.f_m_bMoveDrawMode.GetValue(this.ovrControllerBehavior2);
		}

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

		protected override void OnFixedUpdate()
		{
		}

		protected override void PostStart()
		{
		}

		protected override void PostUpdate()
		{
			DeviceWrapper controller = this.controller;
			if (controller == null)
			{
				return;
			}
			if (controller.GetPressUp(32) && this.pressDownTime != 0f)
			{
				Win32API.MouseEvent(4);
				this.pressDownTime = 0f;
			}
			if (this.ovrController == (OVRInput.Controller)2 && OVRInput.GetUp((OVRInput.Button)8388608, (OVRInput.Controller)3))
			{
				Win32API.MouseEvent(16);
				this.pressDownTime = Time.time;
			}
			if (this.ovrController == (OVRInput.Controller)1 && OVRInput.GetUp((OVRInput.Button)32768, (OVRInput.Controller)3))
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
				if (!controller.GetPress(1))
				{
					this.resetTimeStart = float.MaxValue;
				}
				if (!this.screenGrabbed && !base.IsCommandToolActive() && !base.IsIKToolActive() && base.IsOvrTabletCtrlOutOfRange())
				{
					Vector2 axis = controller.GetAxis((EVRButtonId)32);
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
					Vector2 axis2 = this.controller.GetAxis((EVRButtonId)32);
					if (axis2 != Vector2.zero)
					{
						this.commandTool.UpdateCommandListLocation(axis2.y * 0.2f);
					}
					if (this.controller.GetPressUp(32) && (!this.moveToHeadEnabled || this.moveToHeadButton != (EVRButtonId)32 || !this.controller.GetPress(this.moveSelfButton)))
					{
						this.commandTool.InvokeCurrentCommand();
					}
				}
			}
			if (this.screenGrabbed || (!base.IsCommandToolActive() && !base.IsIKToolActive() && !this.controller.GetPress(this.grabScreenButton)))
			{
				if (controller.GetPressDown(32))
				{
					this.pressLeft = true;
					Win32API.MouseEvent(2);
					this.pressDownTime = Time.time;
				}
				if (this.ovrController == (OVRInput.Controller)2 && OVRInput.GetDown((OVRInput.Button)8388608, (OVRInput.Controller)3))
				{
					this.pressLeft = false;
					Win32API.MouseEvent(8);
					this.pressDownTime = Time.time;
					return;
				}
				if (this.ovrController == (OVRInput.Controller)1 && OVRInput.GetDown((OVRInput.Button)32768, (OVRInput.Controller)3))
				{
					this.pressLeft = false;
					Win32API.MouseEvent(8);
					this.pressDownTime = Time.time;
				}
			}
		}

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

		protected override void RestoreLastDirectModeStatus()
		{
		}

		protected override void SaveLastDirectModeStatus(bool mode)
		{
			if (this.ovrController == (OVRInput.Controller)1)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
				return;
			}
			if (this.ovrController == (OVRInput.Controller)2)
			{
				Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
			}
		}

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

		private DeviceOculusTouchController device;

		public OVRInput.Controller ovrController;

		public OvrController ovr_controller;

		private OvrControllerBehavior ovrControllerBehavior;

		private OvrControllerBehavior2 ovrControllerBehavior2;

		private Vector3 pointerOffset = Vector3.zero;

		private static MethodInfo changeMode = typeof(OvrControllerBehavior).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		private static MethodInfo changeMode2 = typeof(OvrControllerBehavior2).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		private static FieldInfo f_m_eMode = typeof(OvrControllerBehavior).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		private static FieldInfo f_m_eMode2 = typeof(OvrControllerBehavior2).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		private float resetTimeStart;

		private static FieldInfo f_m_bMoveDrawMode = typeof(OvrControllerBehavior2).GetField("m_bMoveDrawMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
	}
}
