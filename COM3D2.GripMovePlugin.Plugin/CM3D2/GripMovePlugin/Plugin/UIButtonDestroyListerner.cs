using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000014 RID: 20
	internal class UIButtonDestroyListerner : MonoBehaviour
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00002824 File Offset: 0x00000A24
		private void OnDestroy()
		{
			this.tool.OnUIButtonDestroy(base.gameObject);
		}

		// Token: 0x04000046 RID: 70
		public YotogiCommandTool tool;
	}
}
