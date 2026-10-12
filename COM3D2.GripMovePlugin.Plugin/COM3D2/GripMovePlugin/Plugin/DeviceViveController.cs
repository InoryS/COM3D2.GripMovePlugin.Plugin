using UnityEngine;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

public class DeviceViveController : DeviceWrapper
{
	private SteamVR_TrackedObject trackedObject;

	public override Vector3 angularVelocity
	{
		get
		{
			if (!trackedObject.isValid)
			{
				return Vector3.zero;
			}

			return device.angularVelocity;
		}
	}

	public override bool connected => trackedObject.isValid && device.connected;

	private SteamVR_Controller.Device device => SteamVR_Controller.Input((int)trackedObject.index);

	public override bool hasTracking => trackedObject.isValid && device.hasTracking;

	public override bool outOfRange => trackedObject.isValid && device.outOfRange;

	public override bool valid => trackedObject.isValid && device.valid;

	public override Vector3 velocity
	{
		get
		{
			if (!trackedObject.isValid)
			{
				return Vector3.zero;
			}

			return device.velocity;
		}
	}

	public override Vector2 GetAxis(EVRButtonId buttonId = (EVRButtonId)32)
	{
		if (!trackedObject.isValid)
		{
			return Vector2.zero;
		}

		return device.GetAxis(buttonId);
	}

	public static DeviceWrapper GetOrCreate(GameObject go, SteamVR_TrackedObject trackedObj)
	{
		DeviceViveController deviceViveController = go.GetComponent<DeviceViveController>();
		if (deviceViveController == null)
		{
			deviceViveController = go.AddComponent<DeviceViveController>();
			deviceViveController.Init(trackedObj);
		}

		return deviceViveController;
	}

	public override bool GetPress(ulong buttonMask) =>
		trackedObject.isValid && device.GetPress(buttonMask);

	public override bool GetPress(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetPress(buttonId);

	public override bool GetPressDown(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetPressDown(buttonId);

	public override bool GetPressDown(ulong buttonMask) =>
		trackedObject.isValid && device.GetPressDown(buttonMask);

	public override bool GetPressUp(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetPressUp(buttonId);

	public override bool GetPressUp(ulong buttonMask) =>
		trackedObject.isValid && device.GetPressUp(buttonMask);

	public override bool GetTouch(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetTouch(buttonId);

	public override bool GetTouch(ulong buttonMask) =>
		trackedObject.isValid && device.GetTouch(buttonMask);

	public override bool GetTouchDown(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetTouchDown(buttonId);

	public override bool GetTouchDown(ulong buttonMask) =>
		trackedObject.isValid && device.GetTouchDown(buttonMask);

	public override bool GetTouchUp(ulong buttonMask) =>
		trackedObject.isValid && device.GetTouchUp(buttonMask);

	public override bool GetTouchUp(EVRButtonId buttonId) =>
		trackedObject.isValid && device.GetTouch(buttonId);

	public void Init(SteamVR_TrackedObject trackedObject) => this.trackedObject = trackedObject;

	public override void TriggerHapticPulse(ushort durationMicroSec = 500,
		EVRButtonId buttonId = (EVRButtonId)32)
	{
		if (!trackedObject.isValid)
		{
			return;
		}

		device.TriggerHapticPulse(durationMicroSec, buttonId);
	}
}
