using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

public class TouchArea
{
	public MaidTouchAreaName areaName;

	public Transform base_t;

	public string customName;

	public DirectTouchMaid dtm;

	public float len;

	public float lenxlen;

	public Maid maid;

	public float pos_len;

	public Vector3 position = Vector3.zero;

	public TouchArea(MaidTouchAreaName areaName, Transform base_t, float len, Vector3 position)
	{
		this.areaName = areaName;
		customName = null;
		this.base_t = base_t;
		this.len = len;
		lenxlen = len * len;
		this.position = position.normalized;
		pos_len = position.magnitude;
	}

	public TouchArea(string customName, Transform base_t, float len, Vector3 position)
	{
		areaName = MaidTouchAreaName.CUSTOM;
		this.customName = customName;
		this.base_t = base_t;
		this.len = len;
		lenxlen = len * len;
		this.position = position.normalized;
		pos_len = position.magnitude;
	}

	public string name
	{
		get
		{
			if (areaName == MaidTouchAreaName.CUSTOM)
			{
				return customName;
			}

			return areaName.ToString();
		}
	}

	public bool TestHit(Vector3 pos, float radius) =>
		(base_t.TransformDirection(position) * pos_len + base_t.position - pos).sqrMagnitude <
		(radius + len) * (radius + len);
}
