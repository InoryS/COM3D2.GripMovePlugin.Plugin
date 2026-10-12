using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

public class DirectTouchMaid : MonoBehaviour
{
	public Maid maid;

	public List<TouchArea> touchAreas = new();

	private void Awake() => maid = gameObject.GetComponent<TBody>().maid;

	public void AddTouchArea(MaidTouchAreaName areaName, string boneName, float radius,
		Vector3 position)
	{
		Transform transform = CMT.SearchObjName(maid.body0.transform, boneName);
		if (transform == null)
		{
			Console.WriteLine("AddTouchArea failed. Bone [" + boneName + "] not found.");
		}

		var touchArea = new TouchArea(areaName, transform, radius, position)
		{
			maid = maid, dtm = this
		};
		touchAreas.Add(touchArea);
	}

	public void AddTouchArea(string customName, string boneName, float radius, Vector3 position)
	{
		Transform transform = CMT.SearchObjName(maid.body0.transform, boneName);
		if (transform == null)
		{
			Console.WriteLine("AddTouchArea failed. Bone [" + boneName + "] not found.");
			return;
		}

		var touchArea = new TouchArea(customName, transform, radius, position)
		{
			maid = maid, dtm = this
		};
		touchAreas.Add(touchArea);
	}

	private void Destroy() => DirectTouchTool.Instance.RemoveDirectTouchMaid(this);
}
