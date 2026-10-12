#if !COM3D25_UNITY_2022
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

internal class MenuTool : MenuToolBase
{
	private static readonly MethodInfo changeMode =
		typeof(ViveControllerBehavior).GetMethod("ChangeMode",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.InvokeMethod);

	private static readonly MethodInfo changeMode2 =
		typeof(ViveControllerBehavior2).GetMethod("ChangeMode",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.InvokeMethod);

	private static readonly FieldInfo f_m_eMode = typeof(ViveControllerBehavior).GetField("m_eMode",
		BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.GetField);

	private static readonly FieldInfo f_m_eMode2 =
		typeof(ViveControllerBehavior2).GetField("m_eMode",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.GetField);

	private static readonly FieldInfo f_m_bMoveDrawMode =
		typeof(ViveControllerBehavior2).GetField("m_bMoveDrawMode",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.GetField);

	private int currentModelIndex;

	private DeviceViveController device;

	protected float menuPressStart;

	private VIVEModelReplaceTool modelReplaceTool;

	private Vector2 touchDownPosition;

	private SteamVR_TrackedObject tracked;

	private ViveController viveController;

	private ViveControllerBehavior viveControllerBehavior;

	private ViveControllerBehavior2 viveControllerBehavior2;

	private void Awake()
	{
	}

	protected override void ChangeToNextModel()
	{
		if (!modelReplaceTool.enabled)
		{
			return;
		}

		SteamVR_TrackedObject component = GameMain.Instance.OvrMgr.ovr_obj.left_controller
			.controller.gameObject.GetComponent<SteamVR_TrackedObject>();
		List<VIVEModelReplaceTool.ModelInfo> list =
			component == null || component.index != tracked.index
				? modelReplaceTool.models_R
				: modelReplaceTool.models_L;
		int num = currentModelIndex + 1;
		if (num >= list.Count())
		{
			num = 0;
		}

		currentModelIndex = num;
		modelReplaceTool.SetModel(tracked, list[currentModelIndex].id);
	}

	protected override bool CheckActivateDirectMode()
	{
		if (controller.GetPressDown(1))
		{
			menuPressStart = Time.time;
		}

		return (Gui == null || IsDirectModeActive()) && controller.GetPress(1) &&
		       Time.time - menuPressStart > activateOrResetSec;
	}

	protected override bool CheckSwitchCommandTool() =>
		controller.GetPressUp(1) && controller.GetPress(yotogiToolButton);

	protected override bool CheckSwitchDirectMode() => (IsGrabMode() || IsDirectModeActive()) &&
	                                                   controller.GetPressUp(1) &&
	                                                   Time.time - menuPressStart <=
	                                                   activateOrResetSec;

	protected override bool CheckSwitchIKTool() =>
		controller.GetPressUp(1) && controller.GetPress(IKToolButton);

	public override DeviceWrapper GetController()
	{
		if (device == null)
		{
			SteamVR_TrackedObject component = GetComponent<SteamVR_TrackedObject>();
			if (component != null)
			{
				device = gameObject.GetComponent<DeviceViveController>();
				if (device == null)
				{
					device = gameObject.AddComponent<DeviceViveController>();
					device.Init(component);
				}
			}
		}

		return device;
	}

	protected override OvrGripCollider GetOvrGripCollider()
	{
		if (viveController == null)
		{
			return null;
		}

		if (!viveController.m_bHandL)
		{
			return GameMain.Instance.OvrMgr.ovr_obj.right_controller.grip_collider;
		}

		return GameMain.Instance.OvrMgr.ovr_obj.left_controller.grip_collider;
	}

	protected override Text GetTextForMode() => GetComponentInChildren<Text>();

	public override bool IsDirectModeActive() =>
		Gui != null && viveController != null && !viveController.enabled;

	public override bool IsDrawMode() => viveControllerBehavior2 != null &&
	                                     viveControllerBehavior2.enabled &&
	                                     (bool)f_m_bMoveDrawMode.GetValue(viveControllerBehavior2);

	private bool IsGrabMode()
	{
		if (IsInNewMode())
		{
			if (viveControllerBehavior2 != null)
			{
				return (int)f_m_eMode2.GetValue(viveControllerBehavior2) == 0;
			}
		}
		else if (viveControllerBehavior != null)
		{
			return (int)f_m_eMode.GetValue(viveControllerBehavior) == 0;
		}

		return false;
	}

	protected override void OnDirectModeEnabledChanged(bool newMode)
	{
		if (viveController != null)
		{
			viveController.enabled = !newMode;
		}

		if (IsInNewMode())
		{
			if (viveControllerBehavior2 != null)
			{
				viveControllerBehavior2.enabled = !newMode;
			}
		}
		else if (viveControllerBehavior != null)
		{
			viveControllerBehavior.enabled = !newMode;
		}
	}

	protected override void OnFixedUpdate()
	{
		DeviceWrapper controller = this.controller;
		if (controller == null)
		{
			return;
		}

		if (!IsDirectModeActive() && (!IMGUIQuad.IsVisble || !IMGUIQuad.Instance.NowAttached))
		{
			return;
		}

		if (screenGrabbed || (!IsCommandToolActive() && !IsIKToolActive() &&
		                      !this.controller.GetPress(grabScreenButton)))
		{
			if (controller.GetPressDown(32))
			{
				pressLeft = controller.GetAxis().x < 0.3f;
				Win32API.MouseEvent(pressLeft ? 2 : 8);
				pressDownTime = Time.time;
			}

			if (controller.GetPressUp(32))
			{
				Win32API.MouseEvent(pressLeft ? 4 : 16);
				pressDownTime = 0f;
			}
		}
	}

	protected override void PostStart()
	{
		tracked = GetComponent<SteamVR_TrackedObject>();
		modelReplaceTool = VIVEModelReplaceTool.Instance;
	}

	protected override void PostUpdate()
	{
		base.PostUpdate();
		if (!screenGrabbed && !IsCommandToolActive() && !IsIKToolActive() &&
		    IsOvrTabletCtrlOutOfRange())
		{
			if (device.GetTouchDown(32))
			{
				touchDownPosition = device.GetAxis();
			}

			if (device.GetTouch(32) && Time.time - pressDownTime > 0.3f)
			{
				int num = 1280;
				int num2 = 720;
				this.cursorPosVR = UICameraUtil.GetOvrVirtualMousePos();
				Vector2 axis = device.GetAxis();
				Vector2 vector = axis - touchDownPosition;
				Vector3 cursorPosVR = this.cursorPosVR;
				cursorPosVR.x += vector.x * num * mouseCursorSpeed;
				cursorPosVR.y += vector.y * num2 * mouseCursorSpeed;
				UICameraUtil.SetOvrVirtualMousePos(cursorPosVR);
				touchDownPosition = axis;
			}
		}

		if (!screenGrabbed && IsCommandToolActive())
		{
			if (controller.GetTouchDown(32))
			{
				touchDownPosition = controller.GetAxis();
			}

			if (controller.GetTouch(32) && Time.time - pressDownTime > 0.3f)
			{
				Vector2 axis2 = controller.GetAxis();
				float x = (axis2 - touchDownPosition).x;
				commandTool.UpdateCommandListLocation(x);
				touchDownPosition = axis2;
			}

			if (controller.GetPressDown(32))
			{
				pressDownTime = Time.time;
			}

			if (controller.GetPressUp(32))
			{
				if (!moveToHeadEnabled || moveToHeadButton != (EVRButtonId)32 ||
				    !controller.GetPress(moveSelfButton))
				{
					commandTool.InvokeCurrentCommand();
				}

				pressDownTime = 0f;
			}
		}
	}

	protected override void PreUpdate()
	{
		if (viveController == null)
		{
			viveController = GetComponent<ViveController>();
		}

		if (viveControllerBehavior == null)
		{
			viveControllerBehavior = GetComponent<ViveControllerBehavior>();
		}

		if (viveControllerBehavior2 == null)
		{
			viveControllerBehavior2 = GetComponent<ViveControllerBehavior2>();
		}
	}

	protected override void RestoreLastDirectModeStatus()
	{
		OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
		OvrMgr.OvrObject.Controller left_controller = ovr_obj.left_controller;
		OvrMgr.OvrObject.Controller right_controller = ovr_obj.right_controller;
		if (left_controller.controller != null &&
		    left_controller.controller.gameObject == gameObject)
		{
			needActivateDirectMode = Settings.Instance.GetBoolValue("LastDirectMode_L", false);
			return;
		}

		if (right_controller.controller != null &&
		    right_controller.controller.gameObject == gameObject)
		{
			needActivateDirectMode = Settings.Instance.GetBoolValue("LastDirectMode_R", false);
		}
	}

	protected override void SaveLastDirectModeStatus(bool mode)
	{
		OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
		OvrMgr.OvrObject.Controller left_controller = ovr_obj.left_controller;
		OvrMgr.OvrObject.Controller right_controller = ovr_obj.right_controller;
		if (left_controller.controller != null &&
		    left_controller.controller.gameObject == gameObject)
		{
			Settings.Instance.SetBoolValue("LastDirectMode_L", mode);
			return;
		}

		if (right_controller.controller != null &&
		    right_controller.controller.gameObject == gameObject)
		{
			Settings.Instance.SetBoolValue("LastDirectMode_R", mode);
		}
	}

	protected override void TurnOffDirectMode()
	{
		if (IsInNewMode())
		{
			if (viveControllerBehavior2 != null)
			{
				changeMode2.Invoke(viveControllerBehavior2, new object[] { 1 });
			}
		}
		else if (viveControllerBehavior != null)
		{
			changeMode.Invoke(viveControllerBehavior, new object[] { 1 });
		}
	}

	protected override void TurnOnDirectMode()
	{
		if (IsInNewMode())
		{
			if (viveControllerBehavior2 != null && IsGrabMode())
			{
				changeMode2.Invoke(viveControllerBehavior2, new object[] { 1 });
				DirectModeEnable(true);
			}
		}
		else if (viveControllerBehavior != null && IsGrabMode())
		{
			changeMode.Invoke(viveControllerBehavior, new object[] { 1 });
			DirectModeEnable(true);
		}
	}
}
#endif
