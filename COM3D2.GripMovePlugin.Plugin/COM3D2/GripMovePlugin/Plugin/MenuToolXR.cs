#if COM3D25_UNITY_2022
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace COM3D2.GripMovePlugin.Plugin;

internal class MenuToolXR : MenuToolBase
{
	private DeviceXRController device;

	public OvrMgr.OvrObject.Controller ovrController;
	public AVRController ovr_controller;

	private OvrControllerBehavior ovrControllerBehavior;
	private OvrControllerBehavior2 ovrControllerBehavior2;

	private Vector3 pointerOffset = Vector3.zero;
	private float resetTimeStart;

	private static MethodInfo changeMode = typeof(OvrControllerBehavior).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
	private static MethodInfo changeMode2 = typeof(OvrControllerBehavior2).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
	private static FieldInfo f_m_eMode = typeof(OvrControllerBehavior).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
	private static FieldInfo f_m_eMode2 = typeof(OvrControllerBehavior2).GetField("m_eMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
	private static FieldInfo f_m_bMoveDrawMode = typeof(OvrControllerBehavior2).GetField("m_bMoveDrawMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

	private void Awake()
	{
	}

	protected override void ChangeToNextModel()
	{
	}

	protected override bool CheckActivateDirectMode()
	{
		if (this.device.GetPressDown(AVRControllerButtons.BTN.VIRTUAL_MENU))
		{
			resetTimeStart = Time.time;
		}
		if (this.Gui != null && !IsDirectModeActive())
		{
			return false;
		}
		if (this.device.GetPress(AVRControllerButtons.BTN.VIRTUAL_MENU) && Time.time - resetTimeStart > activateOrResetSec) {
			return true;
		}
		return false;
	}

	protected override bool CheckSwitchCommandTool()
	{
		return !this.device.GetPress(AVRControllerButtons.BTN.TRIGGER) && this.device.GetPress(AVRControllerButtons.BTN.GRIP) && this.device.GetPressUp(AVRControllerButtons.BTN.VIRTUAL_MENU);
	}

	protected override bool CheckSwitchDirectMode()
	{
		return (this.IsDirectModeActive() || this.IsGrabMode()) && (this.device.GetPressUp(AVRControllerButtons.BTN.VIRTUAL_MENU) && Time.time - this.resetTimeStart <= this.activateOrResetSec);
	}

	protected override bool CheckSwitchIKTool()
	{
		return !this.device.GetPress(AVRControllerButtons.BTN.TRIGGER) && this.device.GetPress(AVRControllerButtons.BTN.GRIP) && this.device.GetPressUp(AVRControllerButtons.BTN.VIRTUAL_MENU);
	}

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

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (this.ovrController == null) return null;
		return this.ovrController.grip_collider;
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
		return this.ovrControllerBehavior2 != null && this.ovrControllerBehavior2.enabled && (bool)MenuToolXR.f_m_bMoveDrawMode.GetValue(this.ovrControllerBehavior2);
	}

	private bool IsGrabMode()
	{
		if (base.IsInNewMode())
		{
			if (this.ovrControllerBehavior2 != null)
			{
				return (int)MenuToolXR.f_m_eMode2.GetValue(this.ovrControllerBehavior2) == 0;
			}
		}
		else if (this.ovrControllerBehavior != null)
		{
			return (int)MenuToolXR.f_m_eMode.GetValue(this.ovrControllerBehavior) == 0;
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
		if (device == null)
		{
			return;
		}
		if (device.GetPressUp(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) && this.pressDownTime != 0f)
		{
			Win32API.MouseEvent(4);
			this.pressDownTime = 0f;
		}
		//右クリック
		if (this.device.GetAxisClick()) {
			Win32API.MouseEvent(16);
			this.pressDownTime = Time.time;
		}
		if (!this.IsDirectModeActive())
		{
			if (!IMGUIQuad.IsVisble || !IMGUIQuad.Instance.NowAttached) {
				return;
			}
		}
		else
		{
			if (!device.GetPress(AVRControllerButtons.BTN.VIRTUAL_MENU))
			{
				this.resetTimeStart = float.MaxValue;
			}
			if (!this.screenGrabbed && !base.IsCommandToolActive() && !base.IsIKToolActive() && base.IsOvrTabletCtrlOutOfRange())
			{
				Vector2 axis = device.GetAxis();
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
				Vector2 axis2 = this.device.GetAxis();
				if (axis2 != Vector2.zero)
				{
					this.commandTool.UpdateCommandListLocation(axis2.y * 0.2f);
				}
				if (this.device.GetPressUp(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) && (!this.moveToHeadEnabled || this.moveToHeadButton != AVRControllerButtons.BTN.VIRTUAL_L_CLICK || !this.device.GetPress(this.moveSelfButton)))
				{
					this.commandTool.InvokeCurrentCommand();
				}
			}
		}
		if (this.screenGrabbed || (!base.IsCommandToolActive() && !base.IsIKToolActive() && !this.device.GetPress(this.grabScreenButton)))
		{
			if (device.GetPressDown(AVRControllerButtons.BTN.VIRTUAL_L_CLICK))
			{
				this.pressLeft = true;
				Win32API.MouseEvent(2);
				this.pressDownTime = Time.time;
			}
			//右クリック
			if (this.device.GetAxisClick()) {
				this.pressLeft = false;
				Win32API.MouseEvent(8);
				this.pressDownTime = Time.time;
				return;
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
		if (this.ovrController.controller.m_bHandL)
		{
			Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
			return;
		}
		Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
	}

	protected override void TurnOffDirectMode()
	{
		if (base.IsInNewMode())
		{
			if (this.ovrControllerBehavior2 != null)
			{
				MenuToolXR.changeMode2.Invoke(this.ovrControllerBehavior2, new object[] { Settings.Instance.GetIntValue("DefaultMode", 1) });
				return;
			}
		}
		else if (this.ovrControllerBehavior != null)
		{
			MenuToolXR.changeMode.Invoke(this.ovrControllerBehavior, new object[] { Settings.Instance.GetIntValue("DefaultMode", 1) });
		}
	}

	protected override void TurnOnDirectMode()
	{
		if (base.IsInNewMode())
		{
			if (this.ovrControllerBehavior2 != null && this.IsGrabMode())
			{
				MenuToolXR.changeMode2.Invoke(this.ovrControllerBehavior2, new object[] { Settings.Instance.GetIntValue("DefaultMode", 1) });
				base.DirectModeEnable(true);
				return;
			}
		}
		else if (this.ovrControllerBehavior != null && this.IsGrabMode())
		{
			MenuToolXR.changeMode.Invoke(this.ovrControllerBehavior, new object[] { Settings.Instance.GetIntValue("DefaultMode", 1) });
			base.DirectModeEnable(true);
		}
	}

}
#endif
