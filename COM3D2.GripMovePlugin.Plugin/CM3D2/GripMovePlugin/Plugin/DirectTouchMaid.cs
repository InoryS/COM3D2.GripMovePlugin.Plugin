using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000015 RID: 21
	public class DirectTouchMaid : MonoBehaviour
	{
		// Token: 0x06000079 RID: 121 RVA: 0x0000510C File Offset: 0x0000330C
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

		// Token: 0x0600007A RID: 122 RVA: 0x0000517C File Offset: 0x0000337C
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

		// Token: 0x0600007B RID: 123 RVA: 0x0000284A File Offset: 0x00000A4A
		private void Awake()
		{
			this.maid = base.gameObject.GetComponent<TBody>().maid;
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002862 File Offset: 0x00000A62
		private void Destroy()
		{
			DirectTouchTool.Instance.RemoveDirectTouchMaid(this);
		}

		// Token: 0x04000047 RID: 71
		public Maid maid;

		// Token: 0x04000048 RID: 72
		public List<TouchArea> touchAreas = new List<TouchArea>();
	}
}
