using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200001D RID: 29
	public class TouchArea
	{
		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x00002C38 File Offset: 0x00000E38
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

		// Token: 0x060000D1 RID: 209 RVA: 0x00006960 File Offset: 0x00004B60
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

		// Token: 0x060000D2 RID: 210 RVA: 0x000069C0 File Offset: 0x00004BC0
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

		// Token: 0x060000D3 RID: 211 RVA: 0x00006A24 File Offset: 0x00004C24
		public bool TestHit(Vector3 pos, float radius)
		{
			return (this.base_t.TransformDirection(this.position) * this.pos_len + this.base_t.position - pos).sqrMagnitude < (radius + this.len) * (radius + this.len);
		}

		// Token: 0x04000089 RID: 137
		public MaidTouchAreaName areaName;

		// Token: 0x0400008A RID: 138
		public string customName;

		// Token: 0x0400008B RID: 139
		public Transform base_t;

		// Token: 0x0400008C RID: 140
		public Maid maid;

		// Token: 0x0400008D RID: 141
		public DirectTouchMaid dtm;

		// Token: 0x0400008E RID: 142
		public float len;

		// Token: 0x0400008F RID: 143
		public float lenxlen;

		// Token: 0x04000090 RID: 144
		public Vector3 position = Vector3.zero;

		// Token: 0x04000091 RID: 145
		public float pos_len;
	}
}
