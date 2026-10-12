using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class DirectTouchController : MonoBehaviour
	{
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

		private void Awake()
		{
			this._trackedObject = base.GetComponent<SteamVR_TrackedObject>();
		}

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

		private void OnDestroy()
		{
			DirectTouchTool.Instance.controllers.Remove(this);
		}

		public void SetPosition(Vector3 pos)
		{
			this.position = pos.normalized;
			this.pos_len = pos.magnitude;
		}

		private void TriggerStateChange(TouchArea ta, bool onHit)
		{
			if (this.onHitHandler != null)
			{
				this.onHitHandler(this, ta, onHit);
			}
		}

		private void TriggerStay(TouchArea ta)
		{
			if (this.onStayHandler != null)
			{
				this.onStayHandler(this, ta);
			}
		}

		public float radius;

		private Vector3 position = Vector3.zero;

		private float pos_len;

		public ControllerTouchAreaName areaName;

		public List<TouchArea> lastOnHitTAs = new List<TouchArea>();

		public List<TouchArea> lastAddedTAs = new List<TouchArea>();

		public List<TouchArea> lastRemainTAs = new List<TouchArea>();

		public List<TouchArea> lastRemovedTAs = new List<TouchArea>();

		public Action<DirectTouchController, TouchArea, bool> onHitHandler;

		public Action<DirectTouchController, TouchArea> onStayHandler;

		private SteamVR_TrackedObject _trackedObject;

		public DeviceWrapper vrdeviceWrapper;
	}
}
