using System;
using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class DeviceViveController : DeviceWrapper
	{
		public override Vector3 angularVelocity
		{
			get
			{
				if (!this.trackedObject.isValid)
				{
					return Vector3.zero;
				}
				return this.device.angularVelocity;
			}
		}

		public override bool connected
		{
			get
			{
				return this.trackedObject.isValid && this.device.connected;
			}
		}

		private SteamVR_Controller.Device device
		{
			get
			{
				return SteamVR_Controller.Input((int)this.trackedObject.index);
			}
		}

		public override bool hasTracking
		{
			get
			{
				return this.trackedObject.isValid && this.device.hasTracking;
			}
		}

		public override bool outOfRange
		{
			get
			{
				return this.trackedObject.isValid && this.device.outOfRange;
			}
		}

		public override bool valid
		{
			get
			{
				return this.trackedObject.isValid && this.device.valid;
			}
		}

		public override Vector3 velocity
		{
			get
			{
				if (!this.trackedObject.isValid)
				{
					return Vector3.zero;
				}
				return this.device.velocity;
			}
		}

		public override Vector2 GetAxis(EVRButtonId buttonId = (EVRButtonId)32)
		{
			if (!this.trackedObject.isValid)
			{
				return Vector2.zero;
			}
			return this.device.GetAxis(buttonId);
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

		public override bool GetPress(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPress(buttonMask);
		}

		public override bool GetPress(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPress(buttonId);
		}

		public override bool GetPressDown(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPressDown(buttonId);
		}

		public override bool GetPressDown(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPressDown(buttonMask);
		}

		public override bool GetPressUp(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPressUp(buttonId);
		}

		public override bool GetPressUp(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPressUp(buttonMask);
		}

		public override bool GetTouch(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonId);
		}

		public override bool GetTouch(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonMask);
		}

		public override bool GetTouchDown(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouchDown(buttonId);
		}

		public override bool GetTouchDown(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouchDown(buttonMask);
		}

		public override bool GetTouchUp(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouchUp(buttonMask);
		}

		public override bool GetTouchUp(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonId);
		}

		public void Init(SteamVR_TrackedObject trackedObject)
		{
			this.trackedObject = trackedObject;
		}

		public override void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = (EVRButtonId)32)
		{
			if (!this.trackedObject.isValid)
			{
				return;
			}
			this.device.TriggerHapticPulse(durationMicroSec, buttonId);
		}

		private SteamVR_TrackedObject trackedObject;
	}
}
