#if COM3D25_UNITY_2022
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

public class DeviceXRController : DeviceWrapper
{
	const float PRESS_TH = 0.5f;

	//private float hapticEnd;

	private OvrMgr.OvrObject.Controller ovrController;
	private AVRController controller;
	private AVRControllerButtons controller_buttons;

	private UnityEngine.XR.InputDevice inputDevice;

	public static DeviceWrapper GetOrCreate(GameObject go, OvrMgr.OvrObject.Controller ovrController)
	{
		DeviceXRController deviceXRController = go.GetComponent<DeviceXRController>();
		if (deviceXRController == null) {
			deviceXRController = go.AddComponent<DeviceXRController>();
			deviceXRController.Init(ovrController);
		}
		return deviceXRController;
	}

	public void Init(OvrMgr.OvrObject.Controller ovrController)
	{
		this.ovrController = ovrController;
		this.controller = ovrController.controller;
		this.controller_buttons = ovrController.controller_buttons;
		if (this.controller.m_bHandL) {
			this.inputDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
		} else {
			this.inputDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
		}
	}

	public override Vector3 angularVelocity
	{
		get
		{
			this.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceAngularVelocity, out Vector3 deviceVelocity);
			return deviceVelocity;
		}
	}

	public override bool connected
	{
		get
		{
			return this.controller.enabled;
		}
	}

	public override bool hasTracking
	{
		get
		{
			return this.connected;
		}
	}

	public override bool outOfRange
	{
		get
		{
			return false;
		}
	}

	public override bool valid
	{
		get
		{
			return this.controller.enabled;
		}
	}

	public override Vector3 velocity
	{
		get
		{
			return this.controller.GetHandMoveVelocity();
		}
	}

	public override Vector2 GetAxis()
	{
		//return GameMain.Instance.OvrMgr.GetVRControllerButtons(this.controller.m_bHandL).GetAxis();
		this.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 axis);
		return axis;
	}

	public override bool GetAxisClick()
	{
		this.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool click);
		return click;
	}

	public override bool GetPress(AVRControllerButtons.BTN btn)
	{
		return this.controller_buttons.GetPress(btn);
	}

	public override bool GetPressDown(AVRControllerButtons.BTN btn)
	{
		return this.controller_buttons.GetPressDown(btn);
	}

	public override bool GetPressUp(AVRControllerButtons.BTN btn)
	{
		return this.controller_buttons.GetPressUp(btn);
	}

	public override bool GetTouch(AVRControllerButtons.TOUCH touch)
	{
		return this.controller_buttons.GetTouch(touch);
	}

	public override bool GetTouchDown(AVRControllerButtons.TOUCH touch)
	{
		return this.controller_buttons.GetTouchDown(touch);
	}

	public override bool GetTouchUp(AVRControllerButtons.TOUCH touch)
	{
		return this.controller_buttons.GetTouchUp(touch);
	}

	/*
	private IEnumerator StopHapticCo()
	{
		yield return new WaitForEndOfFrame();
		if (Time.time > this.hapticEnd)
		{
			OVRInput.SetControllerVibration(0f, 0f, this.rawController);
		}
		yield break;
	}

	public override void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
	{
		OVRInput.SetControllerVibration(0.3f, 0.8f, this.rawController);
		float num = (float)(durationMicroSec / 1000) / 1000f;
		this.hapticEnd = Time.time + num;
		base.StopAllCoroutines();
		base.StartCoroutine(this.StopHapticCo());
	}
	*/

}
#endif
