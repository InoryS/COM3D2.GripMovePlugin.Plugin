#if !COM3D25_UNITY_2022
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

internal class MenuToolTouch : MenuToolBase
{
	private static readonly MethodInfo changeMode =
		typeof(OvrControllerBehavior).GetMethod("ChangeMode",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

	private static readonly MethodInfo changeMode2 =
		typeof(OvrControllerBehavior2).GetMethod("ChangeMode",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

	private static readonly FieldInfo f_m_eMode = typeof(OvrControllerBehavior).GetField("m_eMode",
		BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

	private static readonly FieldInfo f_m_eMode2 =
		typeof(OvrControllerBehavior2).GetField("m_eMode",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

	private static readonly FieldInfo f_m_bMoveDrawMode =
		typeof(OvrControllerBehavior2).GetField("m_bMoveDrawMode",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.GetField);

	public OVRInput.Controller ovrController;

	public OvrController ovr_controller;

	private DeviceOculusTouchController device;

	private OvrControllerBehavior ovrControllerBehavior;

	private OvrControllerBehavior2 ovrControllerBehavior2;

	private Vector3 pointerOffset = Vector3.zero;

	private float resetTimeStart;

	private void Awake()
	{
	}

	protected override void ChangeToNextModel()
	{
	}

	protected override bool CheckActivateDirectMode()
	{
		if (controller.GetPressDown((EVRButtonId)1))
		{
			resetTimeStart = Time.time;
		}

		return (Gui == null || IsDirectModeActive()) && controller.GetPress((EVRButtonId)1) &&
		       Time.time - resetTimeStart > activateOrResetSec;
	}

	protected override bool CheckSwitchCommandTool() =>
		!device.GetPress((EVRButtonId)33) && device.GetPress((EVRButtonId)2) &&
		device.GetPressUp((EVRButtonId)1);

	protected override bool CheckSwitchDirectMode() => (IsDirectModeActive() || IsGrabMode()) &&
	                                                   controller.GetPressUp((EVRButtonId)1) &&
	                                                   Time.time - resetTimeStart <=
	                                                   activateOrResetSec;

	protected override bool CheckSwitchIKTool() =>
		!device.GetPress((EVRButtonId)33) && device.GetPress((EVRButtonId)2) &&
		device.GetPressUp((EVRButtonId)1);

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

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (ovr_controller == null)
		{
			return null;
		}

		if (ovr_controller.m_bHandL)
		{
			return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
		}

		return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
	}

	protected override Text GetTextForMode() => GetComponentInChildren<Text>();

	public override bool IsDirectModeActive() =>
		Gui != null && ovr_controller != null && !ovr_controller.enabled;

	public override bool IsDrawMode() => ovrControllerBehavior2 != null &&
	                                     ovrControllerBehavior2.enabled &&
	                                     (bool)f_m_bMoveDrawMode.GetValue(ovrControllerBehavior2);

	private bool IsGrabMode()
	{
		if (IsInNewMode())
		{
			if (ovrControllerBehavior2 != null)
			{
				return (int)f_m_eMode2.GetValue(ovrControllerBehavior2) == 0;
			}
		}
		else if (ovrControllerBehavior != null)
		{
			return (int)f_m_eMode.GetValue(ovrControllerBehavior) == 0;
		}

		return false;
	}

	protected override void OnDirectModeEnabledChanged(bool newMode)
	{
		if (ovr_controller != null)
		{
			ovr_controller.enabled = !newMode;
		}

		if (IsInNewMode())
		{
			if (ovrControllerBehavior2 != null)
			{
				ovrControllerBehavior2.enabled = !newMode;
			}
		}
		else if (ovrControllerBehavior != null)
		{
			ovrControllerBehavior.enabled = !newMode;
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

		if (controller.GetPressUp((EVRButtonId)32) && pressDownTime != 0f)
		{
			Win32API.MouseEvent(4);
			pressDownTime = 0f;
		}

		if (ovrController == (OVRInput.Controller)2 &&
		    OVRInput.GetUp((OVRInput.Button)8388608, (OVRInput.Controller)3))
		{
			Win32API.MouseEvent(16);
			pressDownTime = Time.time;
		}

		if (ovrController == (OVRInput.Controller)1 &&
		    OVRInput.GetUp((OVRInput.Button)32768, (OVRInput.Controller)3))
		{
			Win32API.MouseEvent(16);
			pressDownTime = Time.time;
		}

		if (!IsDirectModeActive())
		{
			if (!IMGUIQuad.IsVisble || !IMGUIQuad.Instance.NowAttached)
			{
				return;
			}
		}
		else
		{
			if (!controller.GetPress((EVRButtonId)1))
			{
				resetTimeStart = float.MaxValue;
			}

			if (!screenGrabbed && !IsCommandToolActive() && !IsIKToolActive() &&
			    IsOvrTabletCtrlOutOfRange())
			{
				Vector2 axis = controller.GetAxis();
				if (axis != Vector2.zero)
				{
					int num = 1280;
					int num2 = 720;
					this.cursorPosVR = UICameraUtil.GetOvrVirtualMousePos();
					Vector2 vector = axis;
					Vector3 cursorPosVR = this.cursorPosVR;
					cursorPosVR.x += vector.x * num * mouseCursorSpeed;
					cursorPosVR.y += vector.y * num2 * mouseCursorSpeed;
					UICameraUtil.SetOvrVirtualMousePos(cursorPosVR);
				}
			}

			if (commandTool != null && commandTool.headTransform == null)
			{
				commandTool.headTransform = VRContextUtil.FindOVRHeadTransform();
			}

			if (!screenGrabbed && IsCommandToolActive())
			{
				Vector2 axis2 = this.controller.GetAxis();
				if (axis2 != Vector2.zero)
				{
					commandTool.UpdateCommandListLocation(axis2.y * 0.2f);
				}

				if (this.controller.GetPressUp((EVRButtonId)32) && (!moveToHeadEnabled ||
				                                       moveToHeadButton != (EVRButtonId)32 ||
				                                       !this.controller.GetPress(moveSelfButton)))
				{
					commandTool.InvokeCurrentCommand();
				}
			}
		}

		if (screenGrabbed || (!IsCommandToolActive() && !IsIKToolActive() &&
		                      !this.controller.GetPress(grabScreenButton)))
		{
			if (controller.GetPressDown((EVRButtonId)32))
			{
				pressLeft = true;
				Win32API.MouseEvent(2);
				pressDownTime = Time.time;
			}

			if (ovrController == (OVRInput.Controller)2 &&
			    OVRInput.GetDown((OVRInput.Button)8388608, (OVRInput.Controller)3))
			{
				pressLeft = false;
				Win32API.MouseEvent(8);
				pressDownTime = Time.time;
				return;
			}

			if (ovrController == (OVRInput.Controller)1 &&
			    OVRInput.GetDown((OVRInput.Button)32768, (OVRInput.Controller)3))
			{
				pressLeft = false;
				Win32API.MouseEvent(8);
				pressDownTime = Time.time;
			}
		}
	}

	protected override void PreUpdate()
	{
		if (ovr_controller == null)
		{
			ovr_controller = GetComponent<OvrController>();
		}

		if (ovrControllerBehavior == null)
		{
			ovrControllerBehavior = GetComponent<OvrControllerBehavior>();
		}

		if (ovrControllerBehavior2 == null)
		{
			ovrControllerBehavior2 = GetComponent<OvrControllerBehavior2>();
		}

		if (commandTool != null && commandTool.headTransform == null)
		{
			commandTool.headTransform = VRContextUtil.FindOVRHeadTransform();
		}

		if (IsDirectModeActive() && pointer == null)
		{
			CreatePointer();
		}

		if (pointer != null)
		{
			pointer.transform.localPosition = Vector3.zero;
		}
	}

	protected override void RestoreLastDirectModeStatus()
	{
	}

	protected override void SaveLastDirectModeStatus(bool mode)
	{
		if (ovrController == (OVRInput.Controller)1)
		{
			Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
			return;
		}

		if (ovrController == (OVRInput.Controller)2)
		{
			Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
		}
	}

	protected override void TurnOffDirectMode()
	{
		if (IsInNewMode())
		{
			if (ovrControllerBehavior2 != null)
			{
				changeMode2.Invoke(ovrControllerBehavior2, new object[] { 1 });
			}
		}
		else if (ovrControllerBehavior != null)
		{
			changeMode.Invoke(ovrControllerBehavior, new object[] { 1 });
		}
	}

	protected override void TurnOnDirectMode()
	{
		if (IsInNewMode())
		{
			if (ovrControllerBehavior2 != null && IsGrabMode())
			{
				changeMode2.Invoke(ovrControllerBehavior2, new object[] { 1 });
				DirectModeEnable(true);
			}
		}
		else if (ovrControllerBehavior != null && IsGrabMode())
		{
			changeMode.Invoke(ovrControllerBehavior, new object[] { 1 });
			DirectModeEnable(true);
		}
	}
}
#endif
