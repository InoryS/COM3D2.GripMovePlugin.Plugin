using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000018 RID: 24
	public class DirectTouchController : MonoBehaviour
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x00002A17 File Offset: 0x00000C17
		public SteamVR_Controller.Device vrcontroller
		{
			get
			{
				if (this._trackedObject == null)
				{
					return null;
				}
				return SteamVR_Controller.Input((int)this._trackedObject.index);
			}
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00002A78 File Offset: 0x00000C78
		private void Awake()
		{
			this._trackedObject = base.GetComponent<SteamVR_TrackedObject>();
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00005D38 File Offset: 0x00003F38
		public void HitTest(List<TouchArea> touchAreas)
		{
			List<TouchArea> list = new List<TouchArea>();
			List<TouchArea> list2 = new List<TouchArea>();
			List<TouchArea> list3 = new List<TouchArea>();
			List<TouchArea> list4 = new List<TouchArea>();
			Vector3 vector = base.transform.TransformDirection(this.position) * this.pos_len;
			vector += base.transform.position;
			foreach (TouchArea touchArea in touchAreas)
			{
				if (!touchArea.TestHit(vector, this.radius))
				{
					if (this.lastOnHitTAs.Contains(touchArea))
					{
						this.TriggerStateChange(touchArea, false);
						list4.Add(touchArea);
					}
				}
				else
				{
					list.Add(touchArea);
					if (this.lastOnHitTAs.Contains(touchArea))
					{
						list3.Add(touchArea);
					}
					else
					{
						this.TriggerStateChange(touchArea, true);
						list2.Add(touchArea);
					}
				}
			}
			this.lastOnHitTAs = list;
			this.lastAddedTAs = list2;
			this.lastRemainTAs = list3;
			this.lastRemovedTAs = list4;
			foreach (TouchArea touchArea2 in this.lastRemainTAs)
			{
				this.TriggerStay(touchArea2);
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00002A86 File Offset: 0x00000C86
		private void OnDestroy()
		{
			DirectTouchTool.Instance.controllers.Remove(this);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00002A99 File Offset: 0x00000C99
		public void SetPosition(Vector3 pos)
		{
			this.position = pos.normalized;
			this.pos_len = pos.magnitude;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00002AB5 File Offset: 0x00000CB5
		private void TriggerStateChange(TouchArea ta, bool onHit)
		{
			if (this.onHitHandler != null)
			{
				this.onHitHandler(this, ta, onHit);
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00002ACD File Offset: 0x00000CCD
		private void TriggerStay(TouchArea ta)
		{
			if (this.onStayHandler != null)
			{
				this.onStayHandler(this, ta);
			}
		}

		// Token: 0x04000064 RID: 100
		public float radius;

		// Token: 0x04000065 RID: 101
		private Vector3 position = Vector3.zero;

		// Token: 0x04000066 RID: 102
		private float pos_len;

		// Token: 0x04000067 RID: 103
		public ControllerTouchAreaName areaName;

		// Token: 0x04000068 RID: 104
		public List<TouchArea> lastOnHitTAs = new List<TouchArea>();

		// Token: 0x04000069 RID: 105
		public List<TouchArea> lastAddedTAs = new List<TouchArea>();

		// Token: 0x0400006A RID: 106
		public List<TouchArea> lastRemainTAs = new List<TouchArea>();

		// Token: 0x0400006B RID: 107
		public List<TouchArea> lastRemovedTAs = new List<TouchArea>();

		// Token: 0x0400006C RID: 108
		public Action<DirectTouchController, TouchArea, bool> onHitHandler;

		// Token: 0x0400006D RID: 109
		public Action<DirectTouchController, TouchArea> onStayHandler;

		// Token: 0x0400006E RID: 110
		private SteamVR_TrackedObject _trackedObject;

		// Token: 0x0400006F RID: 111
		public DeviceWrapper vrdeviceWrapper;
	}
}
