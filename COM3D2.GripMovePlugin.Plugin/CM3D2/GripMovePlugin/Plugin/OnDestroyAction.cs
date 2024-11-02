using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200003B RID: 59
	internal class OnDestroyAction : MonoBehaviour
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600020A RID: 522 RVA: 0x00003800 File Offset: 0x00001A00
		// (set) Token: 0x0600020B RID: 523 RVA: 0x00003808 File Offset: 0x00001A08
		public Action Callback { get; set; }

		// Token: 0x0600020C RID: 524 RVA: 0x00003811 File Offset: 0x00001A11
		private void OnDestroy()
		{
			if (this.Callback != null)
			{
				this.Callback();
			}
		}
	}
}
