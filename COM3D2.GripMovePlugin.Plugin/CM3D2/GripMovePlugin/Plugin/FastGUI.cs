using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class FastGUI : MonoBehaviour
	{
		private void OnGUI()
		{
			GUI.depth = 1000;
			if (Event.current.type == (EventType)7)
			{
				base.SendMessage("OnBeforeGUI");
			}
		}
	}
}
