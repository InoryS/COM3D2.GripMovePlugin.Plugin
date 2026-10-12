using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class UIButtonDestroyListerner : MonoBehaviour
	{
		private void OnDestroy()
		{
			this.tool.OnUIButtonDestroy(base.gameObject);
		}

		public YotogiCommandTool tool;
	}
}
