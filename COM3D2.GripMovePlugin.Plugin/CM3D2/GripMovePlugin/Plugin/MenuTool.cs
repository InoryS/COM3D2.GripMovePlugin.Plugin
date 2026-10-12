using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class MenuTool : MenuToolBase
	{
		private void Awake()
		{
		}

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

		protected override bool CheckActivateDirectMode()
		{
			if (this.controller.GetPressDown(1))
			{
				this.menuPressStart = Time.time;
			}
			return (this.Gui == null || this.IsDirectModeActive()) && (this.controller.GetPress(1) && Time.time - this.menuPressStart > this.activateOrResetSec);
		}

		protected override bool CheckSwitchCommandTool()
		{
			return this.controller.GetPressUp(1) && this.controller.GetPress(this.yotogiToolButton);
		}

		protected override bool CheckSwitchDirectMode()
		{
			return (this.IsGrabMode() || this.IsDirectModeActive()) && (this.controller.GetPressUp(1) && Time.time - this.menuPressStart <= this.activateOrResetSec);
		}

		protected override bool CheckSwitchIKTool()
		{
			return this.controller.GetPressUp(1) && this.controller.GetPress(this.IKToolButton);
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

		public override bool IsDirectModeActive()
		{
			return this.Gui != null && this.viveController != null && !this.viveController.enabled;
		}

		public override bool IsDrawMode()
		{
			return this.viveControllerBehavior2 != null && this.viveControllerBehavior2.enabled && (bool)MenuTool.f_m_bMoveDrawMode.GetValue(this.viveControllerBehavior2);
		}

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
				if (controller.GetPressDown(32))
				{
					this.pressLeft = controller.GetAxis((EVRButtonId)32).x < 0.3f;
					Win32API.MouseEvent(this.pressLeft ? 2 : 8);
					this.pressDownTime = Time.time;
				}
				if (controller.GetPressUp(32))
				{
					Win32API.MouseEvent(this.pressLeft ? 4 : 16);
					this.pressDownTime = 0f;
				}
			}
		}

		protected override void PostStart()
		{
			this.tracked = base.GetComponent<SteamVR_TrackedObject>();
			this.modelReplaceTool = VIVEModelReplaceTool.Instance;
		}

		protected override void PostUpdate()
		{
			base.PostUpdate();
			if (!this.screenGrabbed && !base.IsCommandToolActive() && !base.IsIKToolActive() && base.IsOvrTabletCtrlOutOfRange())
			{
				if (this.device.GetTouchDown(32))
				{
					this.touchDownPosition = this.device.GetAxis((EVRButtonId)32);
				}
				if (this.device.GetTouch(32) && Time.time - this.pressDownTime > 0.3f)
				{
					int num = 1280;
					int num2 = 720;
					this.cursorPosVR = UICameraUtil.GetOvrVirtualMousePos();
					Vector2 axis = this.device.GetAxis((EVRButtonId)32);
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
				if (this.controller.GetTouchDown(32))
				{
					this.touchDownPosition = this.controller.GetAxis((EVRButtonId)32);
				}
				if (this.controller.GetTouch(32) && Time.time - this.pressDownTime > 0.3f)
				{
					Vector2 axis2 = this.controller.GetAxis((EVRButtonId)32);
					float x = (axis2 - this.touchDownPosition).x;
					this.commandTool.UpdateCommandListLocation(x);
					this.touchDownPosition = axis2;
				}
				if (this.controller.GetPressDown(32))
				{
					this.pressDownTime = Time.time;
				}
				if (this.controller.GetPressUp(32))
				{
					if (!this.moveToHeadEnabled || this.moveToHeadButton != (EVRButtonId)32 || !this.controller.GetPress(this.moveSelfButton))
					{
						this.commandTool.InvokeCurrentCommand();
					}
					this.pressDownTime = 0f;
				}
			}
		}

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

		private DeviceViveController device;

		private SteamVR_TrackedObject tracked;

		private ViveController viveController;

		private ViveControllerBehavior viveControllerBehavior;

		private ViveControllerBehavior2 viveControllerBehavior2;

		private VIVEModelReplaceTool modelReplaceTool;

		private int currentModelIndex;

		protected float menuPressStart;

		private static MethodInfo changeMode = typeof(ViveControllerBehavior).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		private static MethodInfo changeMode2 = typeof(ViveControllerBehavior2).GetMethod("ChangeMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		private static FieldInfo f_m_eMode = typeof(ViveControllerBehavior).GetField("m_eMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		private static FieldInfo f_m_eMode2 = typeof(ViveControllerBehavior2).GetField("m_eMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		private Vector2 touchDownPosition;

		private static FieldInfo f_m_bMoveDrawMode = typeof(ViveControllerBehavior2).GetField("m_bMoveDrawMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
	}
}
