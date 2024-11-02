using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200001C RID: 28
	internal class FastGUI : MonoBehaviour
	{
		// Token: 0x060000CF RID: 207 RVA: 0x00002C14 File Offset: 0x00000E14
		private void OnGUI()
		{
			GUI.depth = 1000;
			if (Event.current.type == EventType.Repaint)
			{
				base.SendMessage("OnBeforeGUI");
			}
		}
	}
}
