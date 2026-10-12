using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class TouchArea
	{
		public string name
		{
			get
			{
				if (this.areaName == MaidTouchAreaName.CUSTOM)
				{
					return this.customName;
				}
				return this.areaName.ToString();
			}
		}

		public TouchArea(MaidTouchAreaName areaName, Transform base_t, float len, Vector3 position)
		{
			this.areaName = areaName;
			this.customName = null;
			this.base_t = base_t;
			this.len = len;
			this.lenxlen = len * len;
			this.position = position.normalized;
			this.pos_len = position.magnitude;
		}

		public TouchArea(string customName, Transform base_t, float len, Vector3 position)
		{
			this.areaName = MaidTouchAreaName.CUSTOM;
			this.customName = customName;
			this.base_t = base_t;
			this.len = len;
			this.lenxlen = len * len;
			this.position = position.normalized;
			this.pos_len = position.magnitude;
		}

		public bool TestHit(Vector3 pos, float radius)
		{
			return (this.base_t.TransformDirection(this.position) * this.pos_len + this.base_t.position - pos).sqrMagnitude < (radius + this.len) * (radius + this.len);
		}

		public MaidTouchAreaName areaName;

		public string customName;

		public Transform base_t;

		public Maid maid;

		public DirectTouchMaid dtm;

		public float len;

		public float lenxlen;

		public Vector3 position = Vector3.zero;

		public float pos_len;
	}
}
