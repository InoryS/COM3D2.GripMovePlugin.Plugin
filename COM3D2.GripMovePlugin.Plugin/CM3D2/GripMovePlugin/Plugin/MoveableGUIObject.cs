using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000022 RID: 34
	public class MoveableGUIObject : MonoBehaviour
	{
		// Token: 0x06000126 RID: 294 RVA: 0x00008258 File Offset: 0x00006458
		public void OnMoved()
		{
			foreach (Action<MonoBehaviour> action in this.onMoveLister)
			{
				try
				{
					action(this);
				}
				catch
				{
				}
			}
		}

		// Token: 0x040000C9 RID: 201
		public List<Action<MonoBehaviour>> onMoveLister = new List<Action<MonoBehaviour>>();
	}
}
