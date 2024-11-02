using System;
using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000010 RID: 16
	public abstract class DeviceWrapper : MonoBehaviour
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000060 RID: 96 RVA: 0x00002817 File Offset: 0x00000A17
		public virtual Vector3 angularVelocity
		{
			get
			{
				return Vector3.zero;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000061 RID: 97 RVA: 0x0000281E File Offset: 0x00000A1E
		public virtual bool connected
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000062 RID: 98 RVA: 0x0000281E File Offset: 0x00000A1E
		public virtual bool hasTracking
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000063 RID: 99 RVA: 0x00002821 File Offset: 0x00000A21
		public virtual bool outOfRange
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000064 RID: 100 RVA: 0x0000281E File Offset: 0x00000A1E
		public virtual bool valid
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00002817 File Offset: 0x00000A17
		public virtual Vector3 velocity
		{
			get
			{
				return Vector3.zero;
			}
		}

		// Token: 0x06000067 RID: 103
		public abstract Vector2 GetAxis(EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0);

		// Token: 0x06000068 RID: 104
		public abstract bool GetPress(ulong buttonMask);

		// Token: 0x06000069 RID: 105
		public abstract bool GetPress(EVRButtonId buttonId);

		// Token: 0x0600006A RID: 106
		public abstract bool GetPressDown(EVRButtonId buttonId);

		// Token: 0x0600006B RID: 107
		public abstract bool GetPressDown(ulong buttonMask);

		// Token: 0x0600006C RID: 108
		public abstract bool GetPressUp(EVRButtonId buttonId);

		// Token: 0x0600006D RID: 109
		public abstract bool GetPressUp(ulong buttonMask);

		// Token: 0x0600006E RID: 110
		public abstract bool GetTouch(EVRButtonId buttonId);

		// Token: 0x0600006F RID: 111
		public abstract bool GetTouch(ulong buttonMask);

		// Token: 0x06000070 RID: 112
		public abstract bool GetTouchDown(EVRButtonId buttonId);

		// Token: 0x06000071 RID: 113
		public abstract bool GetTouchDown(ulong buttonMask);

		// Token: 0x06000072 RID: 114
		public abstract bool GetTouchUp(ulong buttonMask);

		// Token: 0x06000073 RID: 115
		public abstract bool GetTouchUp(EVRButtonId buttonId);

		// Token: 0x06000074 RID: 116
		public abstract void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0);
	}
}
