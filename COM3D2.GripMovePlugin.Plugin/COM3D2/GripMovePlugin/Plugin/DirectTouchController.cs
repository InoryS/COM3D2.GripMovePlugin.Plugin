using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

public class DirectTouchController : MonoBehaviour
{
	public float radius;

	public ControllerTouchAreaName areaName;

	public DeviceWrapper vrdeviceWrapper;

	private SteamVR_TrackedObject _trackedObject;

	public List<TouchArea> lastAddedTAs = new();

	public List<TouchArea> lastOnHitTAs = new();

	public List<TouchArea> lastRemainTAs = new();

	public List<TouchArea> lastRemovedTAs = new();

	public Action<DirectTouchController, TouchArea, bool> onHitHandler;

	public Action<DirectTouchController, TouchArea> onStayHandler;

	private float pos_len;

	private Vector3 position = Vector3.zero;

	public SteamVR_Controller.Device vrcontroller
	{
		get
		{
			if (_trackedObject == null)
			{
				return null;
			}

			return SteamVR_Controller.Input((int)_trackedObject.index);
		}
	}

	private void Awake() => _trackedObject = GetComponent<SteamVR_TrackedObject>();

	private void OnDestroy() => DirectTouchTool.Instance.controllers.Remove(this);

	public void HitTest(List<TouchArea> touchAreas)
	{
		var list = new List<TouchArea>();
		var list2 = new List<TouchArea>();
		var list3 = new List<TouchArea>();
		var list4 = new List<TouchArea>();
		Vector3 vector = transform.TransformDirection(position) * pos_len;
		vector += transform.position;
		foreach (TouchArea touchArea in touchAreas)
		{
			if (!touchArea.TestHit(vector, radius))
			{
				if (lastOnHitTAs.Contains(touchArea))
				{
					TriggerStateChange(touchArea, false);
					list4.Add(touchArea);
				}
			}
			else
			{
				list.Add(touchArea);
				if (lastOnHitTAs.Contains(touchArea))
				{
					list3.Add(touchArea);
				}
				else
				{
					TriggerStateChange(touchArea, true);
					list2.Add(touchArea);
				}
			}
		}

		lastOnHitTAs = list;
		lastAddedTAs = list2;
		lastRemainTAs = list3;
		lastRemovedTAs = list4;
		foreach (TouchArea touchArea2 in lastRemainTAs)
		{
			TriggerStay(touchArea2);
		}
	}

	public void SetPosition(Vector3 pos)
	{
		position = pos.normalized;
		pos_len = pos.magnitude;
	}

	private void TriggerStateChange(TouchArea ta, bool onHit)
	{
		if (onHitHandler != null)
		{
			onHitHandler(this, ta, onHit);
		}
	}

	private void TriggerStay(TouchArea ta)
	{
		if (onStayHandler != null)
		{
			onStayHandler(this, ta);
		}
	}
}
