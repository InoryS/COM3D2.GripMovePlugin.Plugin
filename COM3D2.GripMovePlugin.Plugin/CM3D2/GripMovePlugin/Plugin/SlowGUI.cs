using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200001E RID: 30
	internal class SlowGUI : MonoBehaviour
	{
		// Token: 0x060000D5 RID: 213 RVA: 0x00002C5F File Offset: 0x00000E5F
		private void OnGUI()
		{
			GUI.depth = -1000;
			if (Event.current.type == EventType.Repaint)
			{
				base.SendMessage("OnAfterGUI");
			}
		}
	}
}
