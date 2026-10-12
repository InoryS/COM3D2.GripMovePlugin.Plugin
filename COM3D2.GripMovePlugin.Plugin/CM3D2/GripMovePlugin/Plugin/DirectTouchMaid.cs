using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class DirectTouchMaid : MonoBehaviour
	{
		public void AddTouchArea(MaidTouchAreaName areaName, string boneName, float radius, Vector3 position)
		{
			Transform transform = CMT.SearchObjName(this.maid.body0.transform, boneName, true);
			if (transform == null)
			{
				Console.WriteLine("AddTouchArea failed. Bone [" + boneName + "] not found.");
			}
			TouchArea touchArea = new TouchArea(areaName, transform, radius, position)
			{
				maid = this.maid,
				dtm = this
			};
			this.touchAreas.Add(touchArea);
		}

		public void AddTouchArea(string customName, string boneName, float radius, Vector3 position)
		{
			Transform transform = CMT.SearchObjName(this.maid.body0.transform, boneName, true);
			if (transform == null)
			{
				Console.WriteLine("AddTouchArea failed. Bone [" + boneName + "] not found.");
				return;
			}
			TouchArea touchArea = new TouchArea(customName, transform, radius, position)
			{
				maid = this.maid,
				dtm = this
			};
			this.touchAreas.Add(touchArea);
		}

		private void Awake()
		{
			this.maid = base.gameObject.GetComponent<TBody>().maid;
		}

		private void Destroy()
		{
			DirectTouchTool.Instance.RemoveDirectTouchMaid(this);
		}

		public Maid maid;

		public List<TouchArea> touchAreas = new List<TouchArea>();
	}
}
