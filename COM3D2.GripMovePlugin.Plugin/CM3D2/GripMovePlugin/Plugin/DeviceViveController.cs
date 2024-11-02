using System;
using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200000F RID: 15
	public class DeviceViveController : DeviceWrapper
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000048 RID: 72 RVA: 0x000025C7 File Offset: 0x000007C7
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

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000049 RID: 73 RVA: 0x000025E7 File Offset: 0x000007E7
		public override bool connected
		{
			get
			{
				return this.trackedObject.isValid && this.device.connected;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00002603 File Offset: 0x00000803
		private SteamVR_Controller.Device device
		{
			get
			{
				return SteamVR_Controller.Input((int)this.trackedObject.index);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00002615 File Offset: 0x00000815
		public override bool hasTracking
		{
			get
			{
				return this.trackedObject.isValid && this.device.hasTracking;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00002631 File Offset: 0x00000831
		public override bool outOfRange
		{
			get
			{
				return this.trackedObject.isValid && this.device.outOfRange;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600004D RID: 77 RVA: 0x0000264D File Offset: 0x0000084D
		public override bool valid
		{
			get
			{
				return this.trackedObject.isValid && this.device.valid;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600004E RID: 78 RVA: 0x00002669 File Offset: 0x00000869
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

		// Token: 0x06000050 RID: 80 RVA: 0x00002691 File Offset: 0x00000891
		public override Vector2 GetAxis(EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			if (!this.trackedObject.isValid)
			{
				return Vector2.zero;
			}
			return this.device.GetAxis(buttonId);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000050E0 File Offset: 0x000032E0
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

		// Token: 0x06000052 RID: 82 RVA: 0x000026B2 File Offset: 0x000008B2
		public override bool GetPress(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPress(buttonMask);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000026CF File Offset: 0x000008CF
		public override bool GetPress(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPress(buttonId);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000026EC File Offset: 0x000008EC
		public override bool GetPressDown(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPressDown(buttonId);
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00002709 File Offset: 0x00000909
		public override bool GetPressDown(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPressDown(buttonMask);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002726 File Offset: 0x00000926
		public override bool GetPressUp(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetPressUp(buttonId);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00002743 File Offset: 0x00000943
		public override bool GetPressUp(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetPressUp(buttonMask);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00002760 File Offset: 0x00000960
		public override bool GetTouch(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonId);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x0000277D File Offset: 0x0000097D
		public override bool GetTouch(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonMask);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x0000279A File Offset: 0x0000099A
		public override bool GetTouchDown(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouchDown(buttonId);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000027B7 File Offset: 0x000009B7
		public override bool GetTouchDown(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouchDown(buttonMask);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000027D4 File Offset: 0x000009D4
		public override bool GetTouchUp(ulong buttonMask)
		{
			return this.trackedObject.isValid && this.device.GetTouchUp(buttonMask);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00002760 File Offset: 0x00000960
		public override bool GetTouchUp(EVRButtonId buttonId)
		{
			return this.trackedObject.isValid && this.device.GetTouch(buttonId);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000027F1 File Offset: 0x000009F1
		public void Init(SteamVR_TrackedObject trackedObject)
		{
			this.trackedObject = trackedObject;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000027FA File Offset: 0x000009FA
		public override void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			if (!this.trackedObject.isValid)
			{
				return;
			}
			this.device.TriggerHapticPulse(durationMicroSec, buttonId);
		}

		// Token: 0x0400002F RID: 47
		private SteamVR_TrackedObject trackedObject;
	}
}
